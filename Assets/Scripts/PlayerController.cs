using NUnit.Framework.Constraints;
using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class PlayerController : MonoBehaviour
{
	CinemachineBrain	cam_brain; 
	CinemachineCamera	active_cam	, cam_to_turnoff;
	public GameObject	camera_tracking_point;


	GROUND_STATE Ground_State = GROUND_STATE.FLAT;
	enum GROUND_STATE { FLAT, GENTLE, STEEP, AIR }
	RaycastHit		ground;
	const float		ground_check_dist				=	 2.0f	, 
					ground_snap						=	 1.2f	, 
					ground_snap_min					=	 1.15f	, 
					ground_snap_offset				=	 1.0f	, 
					high_slope_threshhold			=	50.0f	, 
					slope_normal_projection_length	=	  .5f	;



	WALL_STATE Wall_State = WALL_STATE.FREE;
	enum WALL_STATE { FREE, CLIPPED_WINDERSHINS, HEAD_ON, OBSTRUCTED_WINDERSHINS, CLIPPED_CLOCKWISE, PINCHED, OBSTRUCTED_CLOCKWISE, OBSTRUCTED }
	RaycastHit[] lookingAtWFC;
	const float		check_wall_dist		=   .78f	, 
					check_wall_deg		= 35.0f		, 
					wall_slide_penalty	=   .5f		;



	MOVE_STATE Move_State= MOVE_STATE.IDLE;
	enum MOVE_STATE			{ IDLE,		WALK,	SPRINT,		CROUCH,		PORT_WALL_SLIDING,	STAR_WALL_SLIDING,	FALL_UP,	FALL_DOWN	}
	float[] move_speed =	{ 0f,		10f,	10f,		4f,			4.5f,				4.5f,				5f,			3f			};
	const float move_deadzone      =  .2f	; 
	const int   stop_lerp_duration = 30	    ;
		  int   stop_lerp_elapsed  = 0		;
		  bool  moving             =false	;
	Vector2 movedir2;
	Vector3 movedir3;







	Rigidbody body;
	public GameObject rotationBody; float targetAngle; bool doRotation = true; float cached_input_angle;
	const float max_rotate_degree = 5f, turn_deadzone = 40f, angle_difference_for_cam_snap = 35;
	
	


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

	}

	#region Simple States which don't need to consider inputs

	void determineSimpleState() {
		Ground_State	=	determineGroundState(true);
		Wall_State		=	determineWallState(true);
	}

	GROUND_STATE determineGroundState(bool drawCast=false) { 
		ground = checkGround(drawCast);

		if (ground.collider==null) return GROUND_STATE.AIR;

		float slopeGradient = SharedLib.vectorToGrade(ground.normal);

		if (ground.normal.Equals(Vector3.up)) return (ground.distance > ground_snap) ? GROUND_STATE.AIR : GROUND_STATE.FLAT;
		if (slopeGradient > high_slope_threshhold) return GROUND_STATE.STEEP;

		//If still here, by elimination, the ground below exists and it is gently sloped.
		// Check that the character is within a reasonable distance of the slope.
		return (isDistanceToSlopeLessThanK(ground.distance - ground_snap_offset, slopeGradient)) ? GROUND_STATE.AIR : GROUND_STATE.GENTLE;
	
		}
	
	WALL_STATE determineWallState(bool drawCast=false ){
		lookingAtWFC = SharedLib.castWFC(body.position, rotationBody.transform.eulerAngles.y, check_wall_deg, check_wall_dist, drawCast);

		//Check which rays found objects then use binary addition to determine wallState.
		bool[] FCWsuccesses = new bool[3];
		FCWsuccesses[0] = lookingAtWFC[0].collider != null;
		FCWsuccesses[1] = lookingAtWFC[1].collider != null;
		FCWsuccesses[2] = lookingAtWFC[2].collider != null;
		return (WALL_STATE) ( (FCWsuccesses[0]?1:0) + (FCWsuccesses[1]?2:0) + (FCWsuccesses[2]?4:0)) ;

	}

	RaycastHit checkGround(bool drawCast=false) { 
		return drawCast?
			SharedLib.castInDirection(body.position,Vector3.down, ground_check_dist, Color.red	)	:
			SharedLib.castInDirection(body.position,Vector3.down* ground_check_dist				)	;
	}

	bool isDistanceToSlopeLessThanK(float distance, float SlopeGradient) { return slope_normal_projection_length*Mathf.Cos(Mathf.Deg2Rad*SlopeGradient) < distance; }

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
		float input_angle = SharedLib.vectorToFlatAngle(panDir);
		if (active_cam == (CinemachineCamera)cam_brain.ActiveVirtualCamera)						cached_input_angle = input_angle;
		if ( Mathf.Abs(cached_input_angle - input_angle) >= angle_difference_for_cam_snap)		active_cam = (CinemachineCamera)cam_brain.ActiveVirtualCamera;	
	}

	void panCameraTank(Vector2 panDir) { return; } //TODO: impliment?

	Vector2 rotateVectorToCamera(Vector2 inputs) {
		float camAngle = 360f - active_cam.transform.eulerAngles.y; //eulerAngles.y acts as yaw in unity.
		return SharedLib.rotateVector2(camAngle, inputs);
	}

	#endregion

	void applyTransforms() {
		applyRotation();
		if (moving)	applyMovement();
		else        stopMovement();
	}
	
	void applyRotation() {
		targetAngle-=.1f;
		rotationBody.transform.rotation = Quaternion.AngleAxis(targetAngle,Vector3.down);
	}

	void applyMovement() {
		movedir2            = respectWalls();
		movedir3.x= movedir2.x * move_speed[(int)Move_State];
		movedir3.z= movedir2.y * move_speed[(int)Move_State];

		body.linearVelocity = movedir3;

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


	
	









//			body.linearVelocity = movedir3 * move_speed + Vector3.up * body.linearVelocity.y;

//		}

//		if (!moving) StopMoving();

//		
//		switch (groundState)
//		{
//			case GROUND_STATE.AIR:
//				body.useGravity = true;
//				break;

//			case GROUND_STATE.FLAT:
//				body.useGravity = false;
//				SnapToGround(ground);
//				break;

//			case GROUND_STATE.GENTLE:
//				body.useGravity = false;
//				break;

//			case GROUND_STATE.STEEP:
//				//TODO define Steep Slope Behavior.
//				break;
//			default: break;
//		}

//	}

//	//Takes a plane's normal and returns the plane's Grade(angle) in degrees.


//	// Return whether a distance is greater than the altitude of a right triangle with this angle.whether the 


//	void SnapToGround (RaycastHit ground)
//	{
//		if (ground.distance < ground_snap_min) return;

//		float snapDist = ground.distance - ground_snap_offset;

//		Vector3 snapPos = body.position;
//		snapPos.y -= snapDist;
//		body.position = snapPos;

//		Vector3 linV = body.linearVelocity;
//		linV.y = Mathf.Max(0f, linV.y);
//		body.linearVelocity = linV;

//	}


//void FixedUpdate() {
//	if (doRotation) {
//		Debug.Log("target angle: " + targetAngle);
//		Quaternion quatStart = rotationBody.transform.rotation;
//		Quaternion quatTarget = Quaternion.LookRotation(SharedLib.angleToVector3(Mathf.Deg2Rad*targetAngle));
//		Quaternion quatNew = Quaternion.RotateTowards(quatStart, quatTarget, max_rotate_degree);
//		rotationBody.transform.rotation = quatNew;
//	}
//}

}

//	void OnTriggerEnter(Collider other)
//	{
//		CameraSwitchTrigger cwt = other.GetComponent<CameraSwitchTrigger>();
//		if(cwt != null)
//		{
//			cam_to_turnoff.gameObject.SetActive(false);
//			cwt.cam.gameObject.SetActive(true);
//			cam_to_turnoff = cwt.cam;

//			if(cwt.trackTarget)
//				cwt.cam.Target.TrackingTarget = camera_tracking_point.transform;
//			if(cwt.lookAtTarget)
//				cwt.cam.LookAt = camera_tracking_point.transform;
//		}
//	}


//	Vector2 skewByCamera(Vector2 inputs)
//	{
//		float camAngle = 360f - active_cam.transform.eulerAngles.y; //eulerAngles.y in typical math represent pitch, but because Unity:tm:, eulerAngles.y is acting as yaw.
//		return SharedLib.rotateVector2(camAngle, inputs);
//	}
//	void updateTargetAngle(Vector2 dir) { targetAngle = SharedLib.angleBetweenVectors(Vector2.right, dir); }

//}
