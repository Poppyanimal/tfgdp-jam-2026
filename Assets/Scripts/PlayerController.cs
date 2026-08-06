using Unity.Burst.CompilerServices;
using Unity.Cinemachine;
using Unity.VisualScripting;
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
				ground_snap_dist		= 0.1f	,
				slope_compensate_dist   = 0.4f	,
		        as_ground_dist			= 0.35f	;
	const int	steep_threshhold		= 55	,
				vertical_threshhold		= 85	;


	[Header ("Rotation")]
	public  GameObject rotationBody;
	bool  rotate_when_moving=true;
	public  float lookAtAngle;
	private float currentAngle;


	[Header ("Wall Scans")]
	WALL_STATE Wall_State = WALL_STATE.FREE;
	enum WALL_STATE { FREE, CLIPPED_WINDERSHINS, HEAD_ON, OBSTRUCTED_WINDERSHINS, CLIPPED_CLOCKWISE, PINCHED, OBSTRUCTED_CLOCKWISE, OBSTRUCTED, SKIP, HOP }
	RaycastHit[]  scanSweep, scanSlice;
	const float   scan_distance		= .78f, scan_sweep_degree=15, 
				  wall_block_dist	= .62f;
	readonly float[] scan_slice_values= {0,.31f,.61f};

	[Header ("Movement")]
	MOVE_STATE Move_State = MOVE_STATE.IDLE;
	enum MOVE_STATE			{ IDLE,		WALK,	SPRINT,		CROUCH,		PORT_WALL_SLIDING,	STAR_WALL_SLIDING,	FALL_UP,	FALL_DOWN		} //TODO: Seporate FALL_UP and FALL_DOWN into own enum ? ;
	float[] move_speed =	{ 0f,		1.9f,	3f,			1.5f,		1.5f,				1.5f,				1.7f,		2.1f			};
	bool    moving=false;


	[Header ("Hitbox")]
	CapsuleCollider hitbox;


	
	//	const int       step_frames				=  8	, hop_frame_delay =45, hop_frame_draw=50, hop_frame_up=15,hop_frame_linger=15;
	//		  int		step_hop_frame_target	= -1	, step_hop_frame_current= -1 ;



	public void Start() {
		getComponentFields();
		initializeNonComponentFields();

	}
		private void getComponentFields() {		
			body=GetComponent<Rigidbody>();
			hitbox=GetComponent<CapsuleCollider>();
			cam_brain = FindFirstObjectByType<CinemachineBrain>();
			active_cam = (CinemachineCamera)cam_brain.ActiveVirtualCamera;
			cam_to_turnoff = active_cam;

		}	
		private void initializeNonComponentFields() {
			lookAtAngle = rotationBody.transform.rotation.eulerAngles.y + (Mathf.PI / 2f);;
			
		}

	public void Update() {
		handleInput();
		handleCamera();
		scanEnvironment();

		Wall_State = defineWallState();
		Ground_State = defineGroundState();
		Move_State = defineMoveState();

		executeGroundState();

		animate();

		Debug.LogFormat("State: {0}, {1}, {2}.",Wall_State,Ground_State,Move_State);

	}
		private void handleInput() { camInput= calcCamInput();	}
			private Vector3 calcCamInput() {
				rawInput= Input.GetAxisRaw("Vertical")*Vector2.up +Input.GetAxisRaw("Horizontal")*Vector2.left;
				if (rawInput.magnitude<move_deadzone) return Vector3.zero;		
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
		
		private void handleCamera() {
			if (GlobalSettings.get().useModernControls) panCameraModern(rawInput.normalized);
			else                                        panCameraTank  (rawInput.normalized);
		}
			void panCameraModern(Vector2 panDir) { 
				float input_angle = SharedLib.angleBetweenVectors(panDir,Vector2.right);
				if (active_cam == (CinemachineCamera)cam_brain.ActiveVirtualCamera)						cached_input_angle = input_angle;
				if ( Mathf.Abs(cached_input_angle - input_angle) >= angle_difference_for_cam_snap)		active_cam = (CinemachineCamera)cam_brain.ActiveVirtualCamera;	
			}
			void panCameraTank(Vector2 panDir) { return; } //TODO: impliment?

		private void scanEnvironment() { 
			Vector3[] sweepDirections= new Vector3[3] { SharedLib.angleToVector3(lookAtAngle - scan_sweep_degree), SharedLib.angleToVector3(lookAtAngle), SharedLib.angleToVector3(lookAtAngle+scan_sweep_degree)};
			Vector3[] slicePositions = new Vector3[3] { body.position-Vector3.up*(hitbox.height/2-scan_slice_values[0]),
														body.position-Vector3.up*(hitbox.height/2-scan_slice_values[1]),
														body.position-Vector3.up*(hitbox.height/2-scan_slice_values[2])  };
		    scanSweep = SharedLib.scanSweep(body.position, sweepDirections, scan_distance);
			scanSlice = SharedLib.scanSlice(slicePositions, Vector3.ProjectOnPlane(SharedLib.angleToVector3(lookAtAngle),asGround.normal), scan_distance );
		}
	
	#region State Logic
		private WALL_STATE defineWallState() {
			bool[] scanSweepHit = new bool[3];
			scanSweepHit[0] = scanSweep[0].collider != null && scanSweep[0].distance < wall_block_dist ;
			scanSweepHit[1] = scanSweep[1].collider != null && scanSweep[1].distance < wall_block_dist ;
			scanSweepHit[2] = scanSweep[2].collider != null && scanSweep[2].distance < wall_block_dist ;

			return (WALL_STATE)( (scanSweepHit[0]?1:0) + (scanSweepHit[1]?2:0) + (scanSweepHit[2]?4:0)) ;
			
		}

		private GROUND_STATE defineGroundState() {
			Physics.Raycast( new Ray(body.position, Vector3.down), out ground, ground_check_dist, LayerMask.GetMask("Default"), QueryTriggerInteraction.UseGlobal);
			if (ground.collider==null) return GROUND_STATE.AIR;

			asGround= useScanSliceZeroAsGround()? scanSlice[0]:ground; //TODO Determine As GROUND By LOOK_AT;

			float cg = asGround.normal.Equals(Vector3.up) ? 90:SharedLib.vectorToGrade(asGround.normal);
			GROUND_STATE naive_state =	cg==90								? GROUND_STATE.FLAT		:
			/**/						cg < steep_threshhold				? GROUND_STATE.GENTLE	:
			/**/						cg < vertical_threshhold			? GROUND_STATE.STEEP	:
			/**/                        /**/								  GROUND_STATE.AIR		;

			if (ground.distance> ground_snap_dist+hitbox.height/2)	return GROUND_STATE.AIR;
			if (!isDistanceToSlopeLessThanK(ground.distance, cg)) return GROUND_STATE.AIR;

			if (naive_state!=GROUND_STATE.AIR) snapToGround();
			return naive_state;

		}
			private bool useScanSliceZeroAsGround() {
				float f_grade = SharedLib.vectorToGrade(scanSlice[0].normal);
				if (f_grade == float.NaN) f_grade = 100;
				return scanSlice[0].collider != null &&  f_grade < vertical_threshhold && scanSlice[0].distance < as_ground_dist;
			}	

			private bool isDistanceToSlopeLessThanK(float distance, float SlopeGradient) { return slope_compensate_dist*Mathf.Cos(Mathf.Deg2Rad*SlopeGradient) < distance; }

			

		private MOVE_STATE defineMoveState() {
			moving= rawInput.magnitude > move_deadzone;
			
			if (Ground_State==GROUND_STATE.AIR) return body.linearVelocity.y>0?MOVE_STATE.FALL_UP:MOVE_STATE.FALL_DOWN ;

			// ToDo Impliment this.
			//		MOVE_STATE modifierMoveState = handleMovementModifierInput();
			//		switch (modifierMoveState){	default: break;	}

			switch (Wall_State) {
				case WALL_STATE.CLIPPED_CLOCKWISE	: case WALL_STATE.OBSTRUCTED_CLOCKWISE								: return moving?MOVE_STATE.STAR_WALL_SLIDING:MOVE_STATE.IDLE;
				case WALL_STATE.CLIPPED_WINDERSHINS	: case WALL_STATE.OBSTRUCTED_WINDERSHINS: case WALL_STATE.PINCHED	: return moving?MOVE_STATE.PORT_WALL_SLIDING:MOVE_STATE.IDLE;
			default:
				return moving? MOVE_STATE.WALK: MOVE_STATE.IDLE;



			}
		}
			MOVE_STATE handleMovementModifierInput() {return MOVE_STATE.IDLE;} //TODO: Impliment sprint/crouch keys if desired.


	#endregion

		private void executeGroundState() {
			float asGrade= SharedLib.vectorToGrade(asGround.normal);
				  asGrade= (asGrade==float.NaN)? 90:asGrade;

			switch (Ground_State) {
				case GROUND_STATE.AIR:
					body.useGravity = true;
					hitbox.height=2;
					break;

				case GROUND_STATE.FLAT:
					body.useGravity = false;
					hitbox.height=2;
					snapToGround();
					break;

				case GROUND_STATE.GENTLE:
					body.useGravity = false;
					hitbox.height=1.98f;
					break;

				case GROUND_STATE.STEEP:
					//TODO define Steep Slope Behavior.
					body.useGravity = true;
					hitbox.height= 1+1*Mathf.Sin(Mathf.Deg2Rad*asGrade);
					break;
				default: break;
				}
				
				
		}

			private void snapToGround() {
				if (!ground.Equals(asGround)) return;
				float snapDist = ground.distance - hitbox.height/2;

				Vector3 snapPos = body.position;
				snapPos.y -= snapDist;
				body.position = snapPos;

				Vector3 linV =body.linearVelocity;
				linV.y = Mathf.Max(0f, linV.y);
				body.linearVelocity = linV;
			}

		private void animate() {
			switch (Move_State) { 
				case MOVE_STATE.IDLE:	case MOVE_STATE.FALL_UP:	case MOVE_STATE.FALL_DOWN	:		playerAnimator.SetBool("isWalking", false	); break;
				default																			:		playerAnimator.SetBool("isWalking", true	); break;
			}
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
		float speed = move_speed[(int)Move_State];
		moveDir2= SharedLib.vector3to2(camInput);
		Vector2 respectfulMoveDir2= movementRespectsWalls();
		Vector3 respectfulMoveDir3= movementRespectsGround(respectfulMoveDir2);
		
		moveDir3= respectfulMoveDir3;

		body.AddForce(moveDir3 * forceMultiple * speed , ForceMode.Force);

		body.linearDamping = groundDrag; //TODO MOVE THIS TO GROUND_STATE switch

		//Speed Control
		Vector3 capV;
		switch (Ground_State){
			case GROUND_STATE.GENTLE: case GROUND_STATE.STEEP:
				
				capV= body.linearVelocity.normalized*speed;
				break;
			default:
				Vector2 flatV = SharedLib.vector3to2 (body.linearVelocity).normalized;
				Vector2 flatCapV = flatV*speed;
				capV= SharedLib.vector2to3(flatCapV) + Vector3.up*body.linearVelocity.y;
				break;

		}
		body.linearVelocity=capV;




		Debug.DrawRay( body.position-Vector3.up*1, moveDir3.normalized*2, Color.cyan, 1);

	}

	//TODO Make Character Rotate in direction of travel when sliding
	private void RotatePlayer() {
		Quaternion qFrom = rotationBody.transform.rotation;
		Quaternion qToward = Quaternion.AngleAxis(lookAtAngle, Vector3.up);

		rotationBody.transform.rotation = Quaternion.RotateTowards(qFrom, qToward, max_yaw_rotate );
		currentAngle= rotationBody.transform.rotation.eulerAngles.y;
		
		//Debug.DrawRay(body.position, SharedLib.angleToVector3(currentAngle), Color.red , 0.5f);
		//Debug.DrawRay(body.position, SharedLib.angleToVector3(lookAtAngle), Color.green, 0.5f);

	}

		Vector3 movementRespectsWalls() {
			float[] rayAngles = new float[3] { lookAtAngle - scan_sweep_degree, lookAtAngle, lookAtAngle + scan_sweep_degree };
			Ray[] raySweep = SharedLib.rayAngleSweep(body.position, rayAngles);
			Vector3 projectdir3 = SharedLib.vector2to3(moveDir2);
			switch (Wall_State) {
				case WALL_STATE.CLIPPED_WINDERSHINS		: projectdir3 = Vector3.ProjectOnPlane(raySweep[2].direction, scanSweep[0].normal); break;
				case WALL_STATE.CLIPPED_CLOCKWISE		: projectdir3 = Vector3.ProjectOnPlane(raySweep[1].direction, scanSweep[2].normal); break;
				case WALL_STATE.OBSTRUCTED_WINDERSHINS	: projectdir3 = Vector3.ProjectOnPlane(raySweep[2].direction, scanSweep[1].normal); break;
				case WALL_STATE.OBSTRUCTED_CLOCKWISE	: projectdir3 = Vector3.ProjectOnPlane(raySweep[0].direction, scanSweep[1].normal); break;
				case WALL_STATE.HEAD_ON					: projectdir3 = Vector3.ProjectOnPlane(raySweep[0].direction, scanSweep[1].normal); break;
				case WALL_STATE.OBSTRUCTED				: projectdir3 = Vector3.ProjectOnPlane(raySweep[1].direction, scanSweep[1].normal); break;
				case WALL_STATE.PINCHED:
				case WALL_STATE.FREE:
				default: break;
			}
			return new Vector2(projectdir3.x, projectdir3.z).normalized;

	}

		Vector3 movementRespectsGround(Vector2 dir2) { return Vector3.ProjectOnPlane( SharedLib.vector2to3(dir2), asGround.normal.normalized );	}




	//	void applyGroundStateConsequences() {
	//		

	//	}

	


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


}