using Unity.Cinemachine;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
	CinemachineBrain	cam_brain; 
	CinemachineCamera	active_cam	, cam_to_turnoff;
	public GameObject	camera_tracking_point;

	public Animator playerAnimator;


	WALL_STATE Wall_State = WALL_STATE.FREE;
	enum WALL_STATE { FREE, CLIPPED_WINDERSHINS, HEAD_ON, OBSTRUCTED_WINDERSHINS, CLIPPED_CLOCKWISE, PINCHED, OBSTRUCTED_CLOCKWISE, OBSTRUCTED }
	RaycastHit[] lookingAtWFC;               
	const float		check_wall_dist		=   .78f	, 
					check_wall_deg		= 35.00f	;

			
	//TODO Implement 'SlideDirLock when near camera 
	GROUND_STATE Ground_State = GROUND_STATE.FLAT;
	enum GROUND_STATE { FLAT, GENTLE, STEEP, AIR, STEP, HOP }
	RaycastHit		ground;
	RaycastHit[]    lookingAtFSH;
	const float		ground_check_dist				=	 2.0f	,
					origin_feet_dist				=	 1.0f	,  
					ground_snap						=	 1.2f	,
					high_slope_threshhold			=	50.0f	, 
					slope_normal_projection_length	=	  .5f	;

	const float		step_max			=   .24f	, hop_max	=   .60f	,
		            step_hop_dist       =   .50f	;
	const int       step_frame				=  3	, hop_frame				=  7 ;
		  int		step_hop_frame_target	= -1	, step_hop_frame_curr	= -1 ;



	MOVE_STATE Move_State= MOVE_STATE.IDLE;
	enum MOVE_STATE			{ IDLE,		WALK,	SPRINT,		CROUCH,		PORT_WALL_SLIDING,	STAR_WALL_SLIDING,	FALL_UP,	FALL_DOWN		} //TODO: Seporate FALL_UP and FALL_DOWN into own enum;
	float[] move_speed =	{ 0f,		1.9f,	3f,			1.5f,		1.5f,				1.5f,				5f,			3f				};
	const float move_deadzone      =  .2f	; 
	const int   stop_lerp_duration = 30	    ;
		  int   stop_lerp_elapsed  = 0		;
		  bool  moving             =false	;
	Vector2 movedir2;
	Vector3 movedir3;



	Rigidbody body;
	
	public GameObject rotationBody; 
	const float rotate_body_max			= 6.5f;
		  float targetAngle				= 0.0f; 
		  bool  rotate_toward_move_dir	= true;
	
	float cached_input_angle;
	const float turn_deadzone = 40f, angle_difference_for_cam_snap = 35;
	
	


	void Start() {
		body = GetComponent<Rigidbody>();
		cam_brain = FindFirstObjectByType<CinemachineBrain>();
		active_cam = (CinemachineCamera)cam_brain.ActiveVirtualCamera;
		cam_to_turnoff = active_cam;
		if (rotationBody != null)
			targetAngle = rotationBody.transform.rotation.eulerAngles.y + (Mathf.PI / 2f);
	}


	void Update() {
		determineSimpleState();
		handleInputs();
		applyTransforms();
		applyGroundStateConsequences();
		
	}

	#region Simple States which don't need to consider inputs

	void determineSimpleState() {
		Wall_State		=	determineWallState(true);
		Ground_State	=	determineGroundState();
		
	}

	#region Wall State

	WALL_STATE determineWallState(bool drawCast=false ){
		lookingAtWFC = SharedLib.castWFC(body.position, rotationBody.transform.eulerAngles.y, check_wall_deg, check_wall_dist, drawCast);

		//Check which rays found objects then use binary addition to determine wallState.
		bool[] WFCsuccesses = new bool[3];
		WFCsuccesses[0] = lookingAtWFC[0].collider != null;
		WFCsuccesses[1] = lookingAtWFC[1].collider != null;
		WFCsuccesses[2] = lookingAtWFC[2].collider != null;

		return (WALL_STATE)( (WFCsuccesses[0]?1:0) + (WFCsuccesses[1]?2:0) + (WFCsuccesses[2]?4:0)) ;

	}

	#endregion

	#region Ground State

	GROUND_STATE determineGroundState(bool drawCast=false) { 
		ground = checkGround(drawCast);
		if (ground.collider==null) return GROUND_STATE.AIR;

		//calculate 

		float[] arg=new float[3] {origin_feet_dist, step_max, hop_max};
		lookingAtFSH = SharedLib.verticalScan(body.position, Vector3.ProjectOnPlane(SharedLib.angleToVector3(rotationBody.transform.eulerAngles.y), ground.normal), arg, check_wall_dist, true, drawCast);
		
		float gradeAtFeet        =                                   SharedLib.vectorToGrade(ground.normal         )   ;
		float gradeInFrontOfFeet = (lookingAtFSH[0].collider!=null)? SharedLib.vectorToGrade(lookingAtFSH[0].normal):0f;


		//if (ground.normal.Equals(Vector3.up)) return (ground.distance > ground_snap) ? 
		//if (gradeAtFeet > high_slope_threshhold) return GROUND_STATE.STEEP;

		////If still here, by elimination, the ground below exists and it is gently sloped.
		//// Check that the character is within a reasonable distance of the slope.
		//return (isDistanceToSlopeLessThanK(ground.distance - origin_feet_dist, gradeAtFeet)) ? GROUND_STATE.AIR : GROUND_STATE.GENTLE;
	

		//Be mindful of downward slopes? This is Probably Bugged
		bool ignoreFSH = Mathf.Max(gradeInFrontOfFeet, gradeInFrontOfFeet+(gradeAtFeet*((body.linearVelocity.y>0)?-1f:1f))) < high_slope_threshhold || lookingAtFSH[0].collider==null;
		
		if (ignoreFSH && ground.normal.Equals(Vector3.up) ) return (ground.distance> ground_snap) ? GROUND_STATE.AIR : GROUND_STATE.FLAT;

		else return GROUND_STATE.AIR;

















	}
	
		RaycastHit checkGround(bool drawCast=false) { return SharedLib.castInDirection(body.position,Vector3.down, ground_check_dist, drawCast? Color.red: default(Color));	}

		bool isDistanceToSlopeLessThanK(float distance, float SlopeGradient) { return slope_normal_projection_length*Mathf.Cos(Mathf.Deg2Rad*SlopeGradient) < distance; }


	void applyGroundStateConsequences() {
		switch (Ground_State) {
			case GROUND_STATE.AIR:
				body.useGravity = true;
				break;

			case GROUND_STATE.FLAT:
				body.useGravity = false;
				SnapToGround(ground);
				break;

			case GROUND_STATE.GENTLE:
				body.useGravity = false;
				break;

			case GROUND_STATE.STEEP:
				//TODO define Steep Slope Behavior.
				break;
			default: break;
		}

	}

		void SnapToGround (RaycastHit ground) {
			float snapDist = ground.distance - origin_feet_dist;

			Vector3 snapPos = body.position;
			snapPos.y -= snapDist;
			body.position = snapPos;

			Vector3 linV = body.linearVelocity;
			linV.y = Mathf.Max(0f, linV.y);
			body.linearVelocity = linV;
		}

	#endregion

	#endregion

	
	#region Input Processing
	void handleInputs() {
		Move_State= handleMovementInput_to_DetermineWalkState();

	}

	//TODO: Impliment sprint/crouch keys if desired.
	MOVE_STATE handleMovementInput_to_DetermineWalkState()  {

		//Take raw arrow movements and check against deadzone. If against deadzone choose exit early betwixt IDLE, FALL_UP, and FALL_DOWN
		Vector2 inputRaw = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		moving = inputRaw.magnitude > move_deadzone;
		playerAnimator.SetBool("isWalking", moving);
		if (!moving) return (body.linearVelocity.y>0) ? MOVE_STATE.FALL_UP: ((body.linearVelocity.y<0)? MOVE_STATE.FALL_DOWN: MOVE_STATE.IDLE);
		stop_lerp_elapsed=0;

		//So, now you know you're moving in the XZ. Seporate your magnatude from your direction.
		float   inputMag	= inputRaw.magnitude;
		Vector2 inputNorm	= inputRaw.normalized;

		//Use the Input to pan the camera if necessary
	
		if (GlobalSettings.get().useModernControls) panCameraModern(inputNorm);
		else                                        panCameraTank  (inputNorm);

		//Use the Camera to skew the input to it if necessary
		movedir2 = rotateVectorToCamera(inputNorm).normalized;
		movedir3 = new Vector3(movedir2.x, body.linearVelocity.y, movedir2.y);

		//ToDo Impliment this.
		MOVE_STATE modifierMoveState = handleMovementModifierInput();
		switch (modifierMoveState){	default: break;	}

		if (Ground_State==GROUND_STATE.AIR ) return (movedir3.y>0?MOVE_STATE.FALL_UP:MOVE_STATE.FALL_DOWN);
		switch (Wall_State) {
			case WALL_STATE.CLIPPED_CLOCKWISE  : case WALL_STATE.OBSTRUCTED_CLOCKWISE							: return MOVE_STATE.STAR_WALL_SLIDING;
			case WALL_STATE.CLIPPED_WINDERSHINS: case WALL_STATE.OBSTRUCTED_WINDERSHINS: case WALL_STATE.PINCHED: return MOVE_STATE.PORT_WALL_SLIDING; 
			default:																							  return MOVE_STATE.WALK;
		}		
		
	} 

	MOVE_STATE handleMovementModifierInput() {return MOVE_STATE.IDLE;} //TODO: Impliment sprint/crouch keys if desired.

	void panCameraModern(Vector2 panDir) { 
		float input_angle = SharedLib.angleBetweenVectors(panDir,Vector2.right);
		if (active_cam == (CinemachineCamera)cam_brain.ActiveVirtualCamera)						cached_input_angle = input_angle;
		if ( Mathf.Abs(cached_input_angle - input_angle) >= angle_difference_for_cam_snap)		active_cam = (CinemachineCamera)cam_brain.ActiveVirtualCamera;	
	}

	void panCameraTank(Vector2 panDir) { return; } //TODO: impliment?

	Vector2 rotateVectorToCamera(Vector2 inputs) {
		float camAngle = 360f - active_cam.transform.eulerAngles.y; //eulerAngles.y acts as yaw in unity.
		return SharedLib.rotateVector2(camAngle, inputs);
	}

	#endregion

	#region Applying Movement

	void applyTransforms() {
		applyRotation();
		if (moving)	applyMovement();
		else        stopMovement();
	}
	
	void applyRotation() {

		if (rotate_toward_move_dir && !movedir2.Equals(Vector2.zero)) targetAngle= Quaternion.LookRotation( new Vector3(movedir2.x, 0, movedir2.y)).eulerAngles.y; 

		Quaternion qFrom = rotationBody.transform.rotation;
		Quaternion qToward = Quaternion.AngleAxis(targetAngle, Vector3.up);

		rotationBody.transform.rotation = Quaternion.RotateTowards(qFrom,qToward,rotate_body_max * Time.deltaTime * 60f);;
	}

	void applyMovement() {
		


		Vector2 respectfulMovedir2            = respectWalls().normalized;
		respectfulMovedir2= (respectfulMovedir2 * move_speed[(int)Move_State]);
		if (Ground_State == GROUND_STATE.AIR) {
			movedir3.x = respectfulMovedir2.x;
			movedir3.z= respectfulMovedir2.y;
		}
		else {
			movedir3= respectGround(respectfulMovedir2);
			snapToGround();
		}

		Debug.DrawRay(body.position-Vector3.up*1, movedir3, Color.cyan, .5f);


		body.linearVelocity = movedir3;
	}

		Vector3 respectWalls() {
			Vector3[] dir3WFC= SharedLib.generateWFC(body.position, rotationBody.transform.eulerAngles.y, check_wall_deg);
			Vector3 projectdir3= movedir3;
			switch (Wall_State) {
				case WALL_STATE.CLIPPED_WINDERSHINS		: projectdir3 = Vector3.ProjectOnPlane(dir3WFC[2], lookingAtWFC[0].normal); break;
				case WALL_STATE.CLIPPED_CLOCKWISE		: projectdir3 = Vector3.ProjectOnPlane(dir3WFC[1], lookingAtWFC[2].normal); break;
				case WALL_STATE.OBSTRUCTED_WINDERSHINS	: projectdir3 = Vector3.ProjectOnPlane(dir3WFC[2], lookingAtWFC[1].normal); break;
				case WALL_STATE.OBSTRUCTED_CLOCKWISE	: projectdir3 = Vector3.ProjectOnPlane(dir3WFC[0], lookingAtWFC[1].normal); break;
				case WALL_STATE.HEAD_ON					: projectdir3 = Vector3.ProjectOnPlane(dir3WFC[0], lookingAtWFC[1].normal); break;
				case WALL_STATE.OBSTRUCTED				: projectdir3 = Vector3.ProjectOnPlane(dir3WFC[1], lookingAtWFC[1].normal); break;
				case WALL_STATE.PINCHED:
				case WALL_STATE.FREE:
				default: break;
			}
			return new Vector2(projectdir3.x,projectdir3.z).normalized;
		}

		Vector3 respectGround(Vector2 dir2) { return Vector3.ProjectOnPlane( SharedLib.vector2to3(dir2),ground.normal);	}

		Vector3 respectAndExecuteStepHop( bool drawCast=false) {
			//bool    isStep    = Wall_State==WALL_STATE.STEP;
			//float   lookAngle = rotationBody.transform.eulerAngles.y,
			//        dist      = lookingAtFSH[0].distance,
		 //           height    = (isStep?step_max:hop_max)*1.01f,
			//		magnitude = movedir2.magnitude;
		

			//return new Vector3 ( dist*Mathf.Sin(Mathf.Deg2Rad*lookAngle),0f, dist*Mathf.Cos(Mathf.Deg2Rad*lookAngle) ).normalized*magnitude+ Vector3.up*height;
			return Vector3.zero;

		}

		void snapToGround() {

		}



	void stopMovement() {
		Vector3 linV = body.linearVelocity;
		linV.x = 0f;
		linV.y = Mathf.Min(0f, linV.y);
		linV.z = 0f;
		body.linearVelocity = linV;

		//Vector3 currV = body.linearVelocity;
		//Vector3 targV = Vector3.zero;
		//Vector3 lerpV = new Vector3();

		//if (stop_lerp_elapsed <= stop_lerp_duration) {
		//	lerpV.x= Mathf.Lerp(currV.x,targV.x, stop_lerp_elapsed/stop_lerp_duration);
		//	lerpV.z= Mathf.Lerp(currV.z,targV.z, stop_lerp_elapsed/stop_lerp_duration);
		//	stop_lerp_elapsed+=1;
		//}
		//lerpV.y = Mathf.Min(0f, currV.y);

		//body.linearVelocity = lerpV;
	}

	

	#endregion

	#region Camera Controls

	void OnTriggerEnter(Collider other) {
		CameraSwitchTrigger cwt = other.GetComponent<CameraSwitchTrigger>();
		if (cwt != null) {
			cam_to_turnoff.gameObject.SetActive(false);
			cwt.cam.gameObject.SetActive(true);
			cam_to_turnoff = cwt.cam;

			if (cwt.trackTarget)
				cwt.cam.Target.TrackingTarget = camera_tracking_point.transform;
			if (cwt.lookAtTarget)
				cwt.cam.LookAt = camera_tracking_point.transform;
		}
	}

	#endregion


}