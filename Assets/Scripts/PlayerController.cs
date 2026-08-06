using Unity.Burst.CompilerServices;
using Unity.Cinemachine;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour {

	[Header ("Miscelanious Components")]
	public	Animator playerAnimator;
			Rigidbody body;

	[Header ("Camera Controls")]
			CinemachineBrain cam_brain;
			CinemachineCamera active_cam, cam_to_turnoff;
	public	GameObject camera_tracking_point;
	const float angle_difference_for_cam_snap=5f;
	float   cached_input_angle;

	[Header ("Input Management")]
	Vector2 rawInput	, moveDir2;
	Vector3	camInput	, moveDir3;
	const float move_deadzone	=	 0.2f	,
				forceMultiple	=	10		, 
				groundDrag		=	 5f		,
				max_yaw_rotate	=    5.0f	;

	[Header ("Ground Check")]
	GROUND_STATE Ground_State = GROUND_STATE.FLAT;
	enum GROUND_STATE { FLAT, GENTLE, STEEP, AIR, STEP, HOP }
	RaycastHit  ground, asGround;
	const float ground_check_dist		= 2.0f	,
				player_standing_height	= 1.0f	;
	//	        as_ground_dist = 0.35f;

	const int	steep_threshhold		= 55	,
				vertical_threshhold		= 85	;

	//RaycastHit[] look_at_FSH;
	//				origin_feet_dist = 1.0f,
	//				ground_snap = 1.2f,
	//				slope_normal_projection_length =	  .5f	;

	[Header ("Rotation")]
	public  GameObject rotationBody;
	bool  rotate_when_moving=true;
	public  float lookAtAngle;
	private float currentAngle;

	[Header ("Wall Scans")]
	WALL_STATE Wall_State = WALL_STATE.FREE;
	enum WALL_STATE { FREE, CLIPPED_WINDERSHINS, HEAD_ON, OBSTRUCTED_WINDERSHINS, CLIPPED_CLOCKWISE, PINCHED, OBSTRUCTED_CLOCKWISE, OBSTRUCTED }
	RaycastHit[]  scanSweep, scanSlice;
	const float   scan_distance= .78f, scan_sweep_degree=15; 
	readonly float[] scan_slice_values= {0,.31f,.61f};


	[Header ("Movement")]
	MOVE_STATE Move_State = MOVE_STATE.IDLE;
	enum MOVE_STATE			{ IDLE,		WALK,	SPRINT,		CROUCH,		PORT_WALL_SLIDING,	STAR_WALL_SLIDING,	FALL_UP,	FALL_DOWN		} //TODO: Seporate FALL_UP and FALL_DOWN into own enum ? ;
	float[] move_speed =	{ 0f,		1.9f,	3f,			1.5f,		1.5f,				1.5f,				5f,			3f				};
	bool    moving=false;

	
	//	const int       step_frames				=  8	, hop_frame_delay =45, hop_frame_draw=50, hop_frame_up=15,hop_frame_linger=15;
	//		  int		step_hop_frame_target	= -1	, step_hop_frame_current= -1 ;







	public void Start() {
		getComponentFields();
		initializeNonComponentFields();

	}
		private void getComponentFields() {		
			body=GetComponent<Rigidbody>();
			
			cam_brain = FindFirstObjectByType<CinemachineBrain>();
			active_cam = (CinemachineCamera)cam_brain.ActiveVirtualCamera;
			cam_to_turnoff = active_cam;

		}	
		private void initializeNonComponentFields() {
			lookAtAngle = rotationBody.transform.rotation.eulerAngles.y + (Mathf.PI / 2f);;
		}

	public void Update() {
		handleInput();
		Wall_State = defineWallState();
		Ground_State = defineGroundState();
		Move_State = defineMoveState();


		Debug.LogFormat("State: {0}, {1}, {2}.",Wall_State,Ground_State,Move_State);
		


	}
		private void handleInput() { camInput= calcCamInput();	}
			private Vector3 calcCamInput() {
				rawInput= Input.GetAxisRaw("Vertical")*Vector2.up +Input.GetAxisRaw("Horizontal")*Vector2.left;
				Vector2		input		= rawInput.normalized;
				float		inputAngle	= 90+SharedLib.vectorToAngle(input);

				float		camYaw		= active_cam.transform.eulerAngles.y;
				Vector3		camYawVec	= SharedLib.angleToVector3(camYaw);

				float		adjAngle	= camYaw-inputAngle;
				Vector3		adjVec	= SharedLib.angleToVector3(adjAngle);

				//Debugging
				if (false && Input.anyKeyDown) {
				
					Vector3 flatPos= SharedLib.vectorFlatten(body.position)+Vector3.up*.4f;
					Vector3 flatCam= SharedLib.vectorFlatten(active_cam.transform.position)+Vector3.up*.4f;
					Vector3 flatCam2= flatCam+Vector3.right*.2f;

					Debug.LogFormat("i:{0}, iA{1}, cY:{2}  cYv:{3}, aA:{4}, aAv", input, inputAngle, camYaw, camYawVec, adjAngle, adjVec);

					Debug.DrawRay( flatPos, SharedLib.vector2to3(input)	, Color.red		, 3f	);
					Debug.DrawRay( flatCam, camYawVec*150				, Color.blue	, 10f	);
					Debug.DrawRay( flatPos, adjVec*4					, Color.green	, 5f	);
				
				}
				return adjVec.normalized;
			}
		
		











		private GROUND_STATE defineGroundState() {
			Physics.Raycast( new Ray(body.position, Vector3.down), out ground, ground_check_dist, LayerMask.GetMask("Default"), QueryTriggerInteraction.UseGlobal);

			asGround=ground; //TODO Determine As GROUND By LOOK_AT;

			float cg = asGround.normal.Equals(Vector3.up) ? 90:SharedLib.vectorToGrade(asGround.normal);
			GROUND_STATE niave_state =	cg==90								? GROUND_STATE.FLAT		:
			/**/						cg < steep_threshhold				? GROUND_STATE.GENTLE	:
			/**/						cg < vertical_threshhold			? GROUND_STATE.STEEP	:
			/**/                        /**/								  GROUND_STATE.AIR		;

			return niave_state;

			//	GROUND_STATE determineGroundState(bool drawCast=false) { 
		//		if (doingStepOrHop()) return Ground_State;

		//		ground = checkGround(drawCast);
		//		if (ground.collider == null) return GROUND_STATE.AIR;

		//		float[] arg = new float[3] { origin_feet_dist, origin_feet_dist - step_max, origin_feet_dist - hop_max };
		//		look_at_FSH = SharedLib.verticalScan(body.position, Vector3.ProjectOnPlane(SharedLib.angleToVector3(rotationBody.transform.eulerAngles.y), ground.normal), arg, check_wall_dist, true, drawCast);

		//		float f_grade= SharedLib.vectorToGrade(look_at_FSH[0].normal); 
		//		if (f_grade==float.NaN) f_grade=100;

		//		//Check if the Surface visible from player's feet should be considered as ground instead of ground entroth. Then, if so return.
		//		consider_f_as_ground =	look_at_FSH[0].collider!=null						&& 
		//		/**/					f_grade < vertical_threshhold						&& 
		//		/**/					look_at_FSH[0].distance<consider_f_as_ground_dist	 ;
		//		RaycastHit ground_considered = consider_f_as_ground? look_at_FSH[0]:ground;
		//		float cg = consider_f_as_ground? f_grade: SharedLib.vectorToGrade(ground_considered.normal);
		//		
		//		if (consider_f_as_ground || f_grade < steep_threshhold) return niave_state;

		//		//Check if the Player should step up or hop if not, return niave state.
		//		GROUND_STATE niave_step_hop=GROUND_STATE.AIR;
		//		if (look_at_FSH[0].collider != null) {
		//			if (look_at_FSH[2].collider==null && look_at_FSH[0].distance<step_hop_dist)  niave_step_hop= GROUND_STATE.HOP ;
		//			if (look_at_FSH[1].collider==null && look_at_FSH[0].distance<step_hop_dist)  niave_step_hop= GROUND_STATE.STEP;

		//		}

		//		return niave_step_hop!=GROUND_STATE.AIR? niave_step_hop: niave_state;

		}

		private MOVE_STATE defineMoveState() {
			moving= rawInput.magnitude > move_deadzone;
			//playerAnimator.SetBool("isWalking", moving);
			
			if (!moving) return Ground_State==GROUND_STATE.AIR ? ( body.linearVelocity.y>0?MOVE_STATE.FALL_UP:MOVE_STATE.FALL_DOWN ) : MOVE_STATE.IDLE ;

			return MOVE_STATE.WALK;

		//MOVE_STATE handleMovementInput_to_DetermineWalkState()  {

	//		//Take raw arrow movements and check against deadzone. If against deadzone choose exit early betwixt IDLE, FALL_UP, and FALL_DOWN

	//		if (!moving) return (body.linearVelocity.y>0) ? MOVE_STATE.FALL_UP: ((body.linearVelocity.y<0)? MOVE_STATE.FALL_DOWN: MOVE_STATE.IDLE);

	//		//So, now you know you're moving in the XZ. Seporate your magnatude from your direction.
	//		float   inputMag	= inputRaw.magnitude;
	//		Vector2 inputNorm	= inputRaw.normalized;

	//		//Use the Input to pan the camera if necessary

	//		if (GlobalSettings.get().useModernControls) panCameraModern(inputNorm);
	//		else                                        panCameraTank  (inputNorm);

	//		//Use the Camera to skew the input to it if necessary


	//		//ToDo Impliment this.
	//		MOVE_STATE modifierMoveState = handleMovementModifierInput();
	//		switch (modifierMoveState){	default: break;	}

	//		if (Ground_State==GROUND_STATE.AIR ) return (moveVec3.y>0?MOVE_STATE.FALL_UP:MOVE_STATE.FALL_DOWN);
	//		switch (Wall_State) {
	//			case WALL_STATE.CLIPPED_CLOCKWISE  : case WALL_STATE.OBSTRUCTED_CLOCKWISE							: return MOVE_STATE.STAR_WALL_SLIDING;
	//			case WALL_STATE.CLIPPED_WINDERSHINS: case WALL_STATE.OBSTRUCTED_WINDERSHINS: case WALL_STATE.PINCHED: return MOVE_STATE.PORT_WALL_SLIDING; 
	//			default:																							  return MOVE_STATE.WALK;
	//		}		

	//	} 

	//	MOVE_STATE handleMovementModifierInput() {return MOVE_STATE.IDLE;} //TODO: Impliment sprint/crouch keys if desired.

		}

		private WALL_STATE defineWallState() {
			Vector3[] sweepDirections= new Vector3[3] { SharedLib.angleToVector3(lookAtAngle - scan_sweep_degree), SharedLib.angleToVector3(lookAtAngle), SharedLib.angleToVector3(lookAtAngle+scan_sweep_degree)};
			Vector3[] slicePositions = new Vector3[3] { body.position-Vector3.up*(player_standing_height-scan_slice_values[0]),
														body.position-Vector3.up*(player_standing_height-scan_slice_values[1]),
														body.position-Vector3.up*(player_standing_height-scan_slice_values[2])  };
		    scanSweep = SharedLib.scanSweep(body.position, sweepDirections, scan_distance);
			scanSlice = SharedLib.scanSlice(slicePositions, Vector3.ProjectOnPlane(SharedLib.angleToVector3(lookAtAngle),asGround.normal), scan_distance );

			bool[] scanSweepHit = new bool[3];
			scanSweepHit[0] = scanSweep[0].collider != null;
			scanSweepHit[1] = scanSweep[1].collider != null;
			scanSweepHit[2] = scanSweep[2].collider != null;

			return (WALL_STATE)( (scanSweepHit[0]?1:0) + (scanSweepHit[1]?2:0) + (scanSweepHit[2]?4:0)) ;
			}





	



	private void FixedUpdate() {
		FacePlayer();
		MovePlayer();
		RotatePlayer();

	}

	private void FacePlayer() {
		if(rotate_when_moving && moving) lookAtAngle= Quaternion.LookRotation( camInput ).eulerAngles.y;
	}

	private void MovePlayer() {
		moveDir2= SharedLib.vector3to2(camInput);
		Vector2 respectfulMoveDir2= movementRespectsWalls();
		Vector3 respectfulMoveDir3= movementRespectsGround(respectfulMoveDir2);
		
		//
		// !!!! THIS IS WHAT YOU'RE WORKING ON !!!!
		//
		
		moveDir3= SharedLib.vector2to3(moveDir2)+Vector3.up*body.linearVelocity.y;
		//moveDir3= respectfulMoveDir3;


		body.AddForce(moveDir3 * forceMultiple * move_speed[(int)Move_State] , ForceMode.Force);

		body.linearDamping = groundDrag; //TODO MOVE THIS TO GROUND_STATE switch


		//Speed Control
		Vector3 normalizedVelocity;
		switch (Ground_State){
			case GROUND_STATE.GENTLE: case GROUND_STATE.STEEP:
				normalizedVelocity = body.linearVelocity.normalized;
				break;
			default:
				Vector2 flatVelocity = SharedLib.vector3to2 (body.linearVelocity).normalized;
				normalizedVelocity= SharedLib.vector2to3(flatVelocity) + Vector3.up*body.linearVelocity.y;
				break;

		}

	}

	//TODO Make Character Rotate in direction of travel when sliding
	private void RotatePlayer() {
		

		Quaternion qFrom = rotationBody.transform.rotation;
		Quaternion qToward = Quaternion.AngleAxis(lookAtAngle, Vector3.up);

//		rotationBody.transform.rotation = qToward;
		rotationBody.transform.rotation = Quaternion.RotateTowards(qFrom, qToward, max_yaw_rotate );
		currentAngle= rotationBody.transform.rotation.eulerAngles.y;
		
		Debug.DrawRay(body.position, SharedLib.angleToVector3(currentAngle), Color.red , 0.5f);
		Debug.DrawRay(body.position, SharedLib.angleToVector3(lookAtAngle), Color.green, 0.5f);

	}



		Vector3 movementRespectsWalls() {
			return moveDir2;
			//float[]   rayAngles= new float[3] { lookAtAngle - scan_sweep_degree, lookAtAngle,lookAtAngle+scan_sweep_degree };
			//Ray[] raySweep  = SharedLib.rayAngleSweep(body.position, rayAngles);
			//Vector3 projectdir3 = SharedLib.vector2to3(moveDir2);
			//switch (Wall_State) {
			//	case WALL_STATE.CLIPPED_WINDERSHINS		: projectdir3 = Vector3.ProjectOnPlane(raySweep[2].direction, scanSweep[0].normal); break;
			//	case WALL_STATE.CLIPPED_CLOCKWISE		: projectdir3 = Vector3.ProjectOnPlane(raySweep[1].direction, scanSweep[2].normal); break;
			//	case WALL_STATE.OBSTRUCTED_WINDERSHINS	: projectdir3 = Vector3.ProjectOnPlane(raySweep[2].direction, scanSweep[1].normal); break;
			//	case WALL_STATE.OBSTRUCTED_CLOCKWISE	: projectdir3 = Vector3.ProjectOnPlane(raySweep[0].direction, scanSweep[1].normal); break;
			//	case WALL_STATE.HEAD_ON					: projectdir3 = Vector3.ProjectOnPlane(raySweep[0].direction, scanSweep[1].normal); break;
			//	case WALL_STATE.OBSTRUCTED				: projectdir3 = Vector3.ProjectOnPlane(raySweep[1].direction, scanSweep[1].normal); break;
			//	case WALL_STATE.PINCHED:
			//	case WALL_STATE.FREE:
			//	default: break;
			//}
			//return new Vector2(projectdir3.x,projectdir3.z).normalized;

		}

		Vector3 movementRespectsGround(Vector2 dir2) { return Vector3.ProjectOnPlane( SharedLib.vector2to3(dir2), asGround.normal.normalized );	}

	//		bool isDistanceToSlopeLessThanK(float distance, float SlopeGradient) { return slope_normal_projection_length*Mathf.Cos(Mathf.Deg2Rad*SlopeGradient) < distance; }

	//	void applyGroundStateConsequences() {
	//		switch (Ground_State) {
	//			case GROUND_STATE.AIR:
	//				body.useGravity = true;
	//				hitbox.height= hitbox_height;
	//				break;

	//			case GROUND_STATE.FLAT:
	//				body.useGravity = false;
	//				hitbox.height= hitbox_height;
	//				snapToGround(ground);
	//				break;

	//			case GROUND_STATE.GENTLE:
	//				body.useGravity = false;
	//				hitbox.height= hitbox_height;
	//				break;

	//			case GROUND_STATE.STEEP:
	//				//TODO define Steep Slope Behavior.
	//				body.useGravity = true;
	//				hitbox.height= hitbox_height;
	//				break;
	//			case GROUND_STATE.STEP:
	//				body.useGravity= false;
	//				hitbox.height= hitbox_step_height;
	//				iterateStepHop(true);
	//				if (step_hop_frame_current==step_hop_frame_target) cleanUpStepHop(true);
	//				break;
	//			case GROUND_STATE.HOP :
	//				body.useGravity= true;
	//				iterateStepHop(false);
	//				if		(step_hop_frame_current==step_hop_frame_target					 )	cleanUpStepHop(false);
	//				else if (step_hop_frame_current<hop_frame_delay+hop_frame_draw && !moving)	cleanUpStepHop(false);
	//				//HitboxHeight handled in DoHop();
	//				break;

	//			default: break;
	//		}

	//	}

	//		void snapToGround (RaycastHit ground) {
	//			float snapDist = ground.distance - origin_feet_dist;

	//			Vector3 snapPos = body.position;
	//			snapPos.y -= snapDist;
	//			body.position = snapPos;

	//			Vector3 linV = body.linearVelocity;
	//			linV.y = Mathf.Max(0f, linV.y);
	//			body.linearVelocity = linV;
	//		}

	#region Camera Controls

	void OnTriggerEnter(Collider other) {
		//CameraSwitchTrigger cwt = other.GetComponent<CameraSwitchTrigger>();
		//if (cwt != null) {
		//	cam_to_turnoff.gameObject.SetActive(false);
		//	cwt.cam.gameObject.SetActive(true);
		//	cam_to_turnoff = cwt.cam;

		//	if (cwt.trackTarget)
		//		cwt.cam.Target.TrackingTarget = camera_tracking_point.transform;
		//	if (cwt.lookAtTarget)
		//		cwt.cam.LookAt = camera_tracking_point.transform;
		//}
	}

	void panCameraModern(Vector2 panDir) {
		//float input_angle = SharedLib.angleBetweenVectors(panDir, Vector2.right);
		//if (active_cam == (CinemachineCamera)cam_brain.ActiveVirtualCamera) cached_input_angle = input_angle;
		//if (Mathf.Abs(cached_input_angle - input_angle) >= angle_difference_for_cam_snap) active_cam = (CinemachineCamera)cam_brain.ActiveVirtualCamera;
	}


	#endregion


}