using NUnit.Framework.Constraints;
using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
	const float move_speed = 3f, max_rotate_degree = 5f, move_deadzone = .2f, turn_deadzone = 40f, angle_difference_for_cam_snap = 35;
	public GameObject rotationBody; float targetAngle; bool doRotation = true; float cached_input_angle;
	CinemachineBrain cam_brain; CinemachineCamera active_cam; CinemachineCamera cam_to_turnoff;
	public GameObject camera_tracking_point;



	Rigidbody body;
	const float ground_check_dist = 2f, ground_snap = 1.2f, ground_snap_min =1.15f, ground_snap_offset = 1.0f, high_slope_threshhold = 50f, slope_normal_projection_length = .5f;
	const float check_wall_dist = .78f, check_wall_deg= 35.0f, wall_slide_penalty=.5f;

	public enum Ground_State { GROUND_FLAT, GROUND_GENTLE_SLOPE, GROUND_STEEP_SLOPE, AIR }
	public Ground_State groundState = Ground_State.GROUND_FLAT;

	public enum Wall_State { FREE, CLIPPED_WINDERSHINS, HEAD_ON, OBSTRUCTED_WINDERSHINS, CLIPPED_CLOCKWISE, PINCHED, OBSTRUCTED_CLOCKWISE, OBSTRUCTED }
	public Wall_State wallState = Wall_State.FREE;

	void Start()
	{
		body = GetComponent<Rigidbody>();
		cam_brain = FindFirstObjectByType<CinemachineBrain>();
		active_cam = (CinemachineCamera)cam_brain.ActiveVirtualCamera;
		cam_to_turnoff = active_cam;
		if (rotationBody != null)
			targetAngle = rotationBody.transform.rotation.eulerAngles.y + (Mathf.PI / 2f);
	}


	void Update()
	{
		Vector3 bp = body.position;

		Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		Vector2 inputdir = input.normalized;
		bool moving = input.magnitude > move_deadzone;

		if (GlobalSettings.get().useModernControls)
		{
			float input_angle = SharedLib.angleBetweenVectors(Vector2.right, inputdir);


			//character movement follows the camera
			if (active_cam == (CinemachineCamera)cam_brain.ActiveVirtualCamera)
				cached_input_angle = input_angle;
			if (!moving || Mathf.Abs(cached_input_angle - input_angle) >= angle_difference_for_cam_snap)
				active_cam = (CinemachineCamera)cam_brain.ActiveVirtualCamera;

			Vector2 movedir2 = skewByCamera(inputdir).normalized;
			Vector3 movedir3 = new Vector3(movedir2.x, 0, movedir2.y);


			if (moving) updateTargetAngle(movedir2);


			movedir3 = slideAlongWall(true);

			body.linearVelocity = movedir3 * move_speed + Vector3.up * body.linearVelocity.y;

		}

		if (!moving) StopMoving();

		RaycastHit ground = checkGround();
		groundState = determineGroundState(ground);
		//Debug.Log("The groundState is: " + groundState);
		switch (groundState)
		{
			case Ground_State.AIR:
				body.useGravity = true;
				break;

			case Ground_State.GROUND_FLAT:
				body.useGravity = false;
				SnapToGround(ground);
				break;

			case Ground_State.GROUND_GENTLE_SLOPE:
				body.useGravity = false;
				break;

			case Ground_State.GROUND_STEEP_SLOPE:
				//TODO define Steep Slope Behavior.
				break;
			default: break;
		}

	}

	void StopMoving()
	{ 
		Vector3 linV = body.linearVelocity;
		linV.x = 0f;
		linV.y = Mathf.Min(0f, linV.y);
		linV.z = 0f;
		body.linearVelocity = linV;
	}

Vector3 slideAlongWall( bool drawCast=false)
	{
		float clockwiseAngle   = Mathf.Deg2Rad * (Mathf.Rad2Deg * targetAngle - check_wall_deg);
		float windershinsAngle = Mathf.Deg2Rad * (Mathf.Rad2Deg * targetAngle + check_wall_deg);

		Vector3 dir3facing      = SharedLib.angleToVector(targetAngle);
		Vector3 dir3clockwise   = SharedLib.angleToVector(clockwiseAngle);
		Vector3 dir3windershins = SharedLib.angleToVector(windershinsAngle);

		RaycastHit wallHitFacing      = castInDirection(dir3facing     , check_wall_dist, drawCast, Color.cyan );
		RaycastHit wallHitClockwise   = castInDirection(dir3clockwise  , check_wall_dist, drawCast, Color.green);
		RaycastHit wallHitWindershins = castInDirection(dir3windershins, check_wall_dist, drawCast, Color.blue );

		//Check which rays found objects then use binary addition to determine wallState.
		bool[] FCWsuccesses = new bool[3];
		FCWsuccesses[0] = wallHitWindershins.collider != null;
		FCWsuccesses[1] = wallHitFacing.collider      != null;
		FCWsuccesses[2] = wallHitClockwise.collider   != null;
		wallState= (Wall_State) ( (FCWsuccesses[0]?1:0) + (FCWsuccesses[1]?2:0) + (FCWsuccesses[2]?4:0)) ;

		//Switch treatment of to return based on wallstate.
		Vector3 toReturn=dir3facing;
		switch (wallState)
		{
			case Wall_State.CLIPPED_WINDERSHINS		:	toReturn=Vector3.ProjectOnPlane(dir3clockwise	, wallHitWindershins.normal	);	break;
			case Wall_State.CLIPPED_CLOCKWISE		:	toReturn=Vector3.ProjectOnPlane(dir3windershins	, wallHitClockwise.normal	);	break;
			case Wall_State.OBSTRUCTED_WINDERSHINS	:	toReturn=Vector3.ProjectOnPlane(dir3clockwise	, wallHitFacing.normal		);	break;
			case Wall_State.OBSTRUCTED_CLOCKWISE	:	toReturn=Vector3.ProjectOnPlane(dir3windershins	, wallHitFacing.normal		);	break;
			case Wall_State.HEAD_ON					:	toReturn=Vector3.ProjectOnPlane(dir3windershins	, wallHitFacing.normal		);	break;
			case Wall_State.OBSTRUCTED				:	toReturn=Vector3.ProjectOnPlane(dir3facing		, wallHitFacing.normal		);	break;
			case Wall_State.PINCHED:
			case Wall_State.FREE: 
			default: break;
		}
		return toReturn.normalized* (wallState.Equals(Wall_State.FREE)?1:wall_slide_penalty);

	}


	RaycastHit castInDirection(Vector3 direction, float checkDist, bool drawCast = false, Color castColor = default(Color))
	{
		RaycastHit hit;
		Ray r = new Ray(body.position, direction);
		bool success = Physics.Raycast(r, out hit, checkDist, LayerMask.GetMask("Default"), QueryTriggerInteraction.UseGlobal);

		if (drawCast) 
		{
			Debug.DrawRay(body.position, direction * (success ? hit.distance:checkDist), castColor,.5f);     
		}
		return hit;

	}
	RaycastHit checkGround() { return castInDirection(Vector3.down, ground_check_dist); }


	Ground_State determineGroundState(RaycastHit ground)
	{
		float slopeGradient = normalToDegGrade(ground.normal);

		if (ground.collider == null)                return Ground_State.AIR;                              //Ground is far away            
		if (ground.normal.Equals(Vector3.up))       return (ground.distance > ground_snap) ? Ground_State.AIR : Ground_State.GROUND_FLAT;
		if (slopeGradient > high_slope_threshhold)  return Ground_State.GROUND_STEEP_SLOPE;

		//If still here, by elimination, the ground below exists and it is gently sloped.
		// Check that the character is within a reasonable distance of the slope.
		return (isDistanceToSlopeLessThanK(ground.distance - ground_snap_offset, slopeGradient)) ? Ground_State.AIR : Ground_State.GROUND_GENTLE_SLOPE;

	}

	//Takes a plane's normal and returns the plane's Grade(angle) in degrees.
	float normalToDegGrade(Vector3 normal)
	{
		float xz = Mathf.Sqrt((normal.x * normal.x) + (normal.z * normal.z));
		float beta_rad = Mathf.Atan(normal.y / xz);
		float alpha_deg = 90 - Mathf.Rad2Deg * beta_rad;
		return alpha_deg;
	}

	// Return whether a distance is greater than the altitude of a right triangle with this angle.whether the 
	bool isDistanceToSlopeLessThanK(float distance, float SlopeGradient)
	{
		float kk = slope_normal_projection_length * Mathf.Cos(SlopeGradient);
		return kk < distance;
	}

	void SnapToGround (RaycastHit ground)
	{
		if (ground.distance < ground_snap_min) return;

		float snapDist = ground.distance - ground_snap_offset;

		Vector3 snapPos = body.position;
		snapPos.y -= snapDist;
		body.position = snapPos;

		Vector3 linV = body.linearVelocity;
		linV.y = Mathf.Max(0f, linV.y);
		body.linearVelocity = linV;

	}


	void FixedUpdate()
	{
		if (doRotation)
		{
			Debug.Log("target angle: "+targetAngle);
			Quaternion quatStart = rotationBody.transform.rotation;
			Quaternion quatTarget = Quaternion.LookRotation(SharedLib.angleToVector(targetAngle));
			Quaternion quatNew = Quaternion.RotateTowards(quatStart, quatTarget, max_rotate_degree);
			rotationBody.transform.rotation = quatNew;
		}
	}



	void OnTriggerEnter(Collider other)
	{
		CameraSwitchTrigger cwt = other.GetComponent<CameraSwitchTrigger>();
		if(cwt != null)
		{
			cam_to_turnoff.gameObject.SetActive(false);
			cwt.cam.gameObject.SetActive(true);
			cam_to_turnoff = cwt.cam;

			if(cwt.trackTarget)
				cwt.cam.Target.TrackingTarget = camera_tracking_point.transform;
			if(cwt.lookAtTarget)
				cwt.cam.LookAt = camera_tracking_point.transform;
		}
	}


	Vector2 skewByCamera(Vector2 inputs)
	{
		float camAngle = 360f - active_cam.transform.eulerAngles.y; //eulerAngles.y in typical math represent pitch, but because Unity:tm:, eulerAngles.y is acting as yaw.
		return SharedLib.rotateVector2(camAngle, inputs);
	}
	void updateTargetAngle(Vector2 dir) { targetAngle = SharedLib.angleBetweenVectors(Vector2.right, dir); }

}
