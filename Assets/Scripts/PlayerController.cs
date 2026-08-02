using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const float move_speed = 5f, max_rotate_speed = 20f, move_deadzone = .2f, turn_deadzone = 40f, angle_difference_for_cam_snap = 15;
    public GameObject rotationBody; float targetAngle; bool doRotation = true; float cached_input_angle;
    CinemachineBrain cam_brain; CinemachineCamera active_cam; CinemachineCamera cam_to_turnoff;
    public GameObject camera_tracking_point;



    Rigidbody body;
    const float ground_check_dist = 2f, ground_snap = 1.2f, ground_snap_min =1.15f, ground_snap_offset = 1.0f, high_slope_threshhold = 50f, slope_normal_projection_length = .5f;

    public enum Ground_State { GROUND_FLAT, GROUND_GENTLE_SLOPE, GROUND_STEEP_SLOPE, AIR }
    public Ground_State groundState = Ground_State.GROUND_FLAT;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        cam_brain = FindFirstObjectByType<CinemachineBrain>();
        active_cam = (CinemachineCamera)cam_brain.ActiveVirtualCamera;
        cam_to_turnoff = active_cam;
        if (rotationBody != null)
            targetAngle = rotationBody.transform.rotation.eulerAngles.y;
    }


    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector2 inputdir = input.normalized;
        bool moving = input.magnitude > move_deadzone;

        if (GlobalSettings.get().useModernControls)
        {
            //preserve direction between cameras unless turn too much / stop moving
            float input_angle = SharedLib.angleToTarget(Vector2.up, inputdir);
            if (active_cam == (CinemachineCamera)cam_brain.ActiveVirtualCamera)
                cached_input_angle = input_angle;
            if (!moving || Mathf.Abs(cached_input_angle - input_angle) >= angle_difference_for_cam_snap)
                active_cam = (CinemachineCamera)cam_brain.ActiveVirtualCamera;

            Vector2 movementdir = skewByCamera(inputdir).normalized;

            //anim.SetBool("isMoving", moving);

            if (moving)
                updateTargetAngle(movementdir);

            body.linearVelocity = new Vector3(movementdir.x, 0f, movementdir.y) * move_speed + Vector3.up * body.linearVelocity.y;
        }
        else //tank controls
        {
            if (SharedLib.angleToTarget(Vector2.up, inputdir) <= turn_deadzone)
            {
                //do forward
            }
            else if (SharedLib.angleToTarget(Vector2.down, inputdir) <= turn_deadzone)
            {
                //and if started from neutral
                //do quickturn
            }
            else
            {
                //do turn
            }
        }

        if (!moving)
        {
            Vector3 linV = body.linearVelocity;
            linV.x = 0f;
            linV.y = Mathf.Min(0f, linV.y);
            linV.z = 0f;
            body.linearVelocity = linV;
        }


        //if grounded, keep grounded and remove y velocity, snap if within certain distance of ground

        RaycastHit ground = checkGround();
        groundState = determineGroundState(ground);
        Debug.Log("The groundState is: " + groundState);
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



        // if(ground.collider != null && ground.distance-snap_offset>snap_min) CheckAndDoGroundSnap(ground);

    }

    RaycastHit checkGround()
    {
        RaycastHit ground;
        Ray r = new Ray(body.position, Vector3.down);
        Physics.Raycast(r, out ground, ground_check_dist, LayerMask.GetMask("Default"), QueryTriggerInteraction.UseGlobal);
        return ground;
    }
    Ground_State determineGroundState(RaycastHit ground)
    {
        float slopeGradient = normalToDegGrade(ground.normal);

        //No Ground In Range
        if (ground.collider == null)                return Ground_State.AIR;

        //Ground Below is flat                              //Ground is far away            
        if (ground.normal.Equals(Vector3.up))       return (ground.distance > ground_snap) ? Ground_State.AIR : Ground_State.GROUND_FLAT;

        //Ground Below is steeply sloped
        if (slopeGradient > high_slope_threshhold)  return Ground_State.GROUND_STEEP_SLOPE;

        //If still here, by elimination, the ground below exists and is gently sloped.
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
        if(doRotation)
        {
            Vector3 rotation = rotationBody.transform.rotation.eulerAngles;
            float curAngle = SharedLib.simplifyEuler(rotation.y);
            float dif = targetAngle - curAngle; 

            if(dif > 180f)
                dif -= 360f;
            else if(dif < -180f)
                dif+= 360f;

            if(dif > max_rotate_speed)
                dif = max_rotate_speed;
            else if(dif < -max_rotate_speed)
                dif = -max_rotate_speed;

            rotation.y = curAngle + dif;
            rotationBody.transform.rotation = Quaternion.Euler(rotation);
        }
    }

    //
    //
    //

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

    //
    //
    //

    Vector2 skewByCamera(Vector2 inputs)
    {
        float camAngle = 360f - active_cam.transform.eulerAngles.y;
        return SharedLib.rotateVector2eul(camAngle, inputs);
    }
    void updateTargetAngle(Vector2 dir)
    {
        float newAngle = 180f - SharedLib.angleToTarget(Vector2.up, dir) * 2f;
        targetAngle = SharedLib.simplifyEuler(newAngle);
    }

}
