using NUnit.Framework.Constraints;
using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    const float move_speed = 5f, max_rotate_degree = 15f, move_deadzone = .2f, turn_deadzone = 40f, angle_difference_for_cam_snap = 35;
    public GameObject rotationBody; float targetAngle; bool doRotation = true; float cached_input_angle;
    CinemachineBrain cam_brain; CinemachineCamera active_cam; CinemachineCamera cam_to_turnoff;
    public GameObject camera_tracking_point;



    Rigidbody body;
    const float ground_check_dist = 2f, ground_snap = 1.2f, ground_snap_min =1.15f, ground_snap_offset = 1.0f, high_slope_threshhold = 50f, slope_normal_projection_length = .5f;
    const float check_wall_dist = .78f, check_wall_deg= 35.0f;

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
        Vector3 bp = body.position;
        //Debug.DrawRay(bp - Vector3.up * .2f, Vector3.right, Color.black, .1f);
        //Debug.DrawRay(bp - Vector3.up * .2f, Vector3.forward, Color.grey, .1f);
        Debug.DrawRay(body.position, SharedLib.angleToVector(targetAngle), Color.cyan, .1f);

        if (Input.GetKey("k"))
            updateTargetAngle();

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

            movedir3 = slideAlongWall(movedir3, false);

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




    //        body.linearVelocity = movementdir3 * move_speed + Vector3.up * body.linearVelocity.y;
    //    }
    //    else //tank controls
    //    {
    //        if (SharedLib.angleToTarget(Vector2.up, inputdir) <= turn_deadzone)
    //        {
    //            //do forward
    //        }
    //        else if (SharedLib.angleToTarget(Vector2.down, inputdir) <= turn_deadzone)
    //        {
    //            //and if started from neutral
    //            //do quickturn
    //        }
    //        else
    //        {
    //            //do turn
    //        }
    //    }




    //Determine ground_state then switch upon it.
    

Vector3 slideAlongWall(Vector3 movedir3, bool drawCast=false)
    {
        Vector3 dir3clockwise   = SharedLib.angleToVector(Mathf.Deg2Rad * (Mathf.Rad2Deg*targetAngle - check_wall_deg));
        Vector3 dir3windershins = SharedLib.angleToVector(Mathf.Deg2Rad * (Mathf.Rad2Deg*targetAngle + check_wall_deg));

        RaycastHit wallBlockC = castInDirection(dir3clockwise  , check_wall_dist, drawCast, Color.green);
        RaycastHit wallBlockW = castInDirection(dir3windershins, check_wall_dist, drawCast, Color.blue);

        Vector3 projectClockwise = Vector3.zero, projectWindershins = Vector3.zero;
        if (wallBlockC.collider != null) projectClockwise   = Vector3.ProjectOnPlane(dir3windershins, wallBlockC.normal);
        if (wallBlockW.collider != null) projectWindershins = Vector3.ProjectOnPlane(dir3clockwise  , wallBlockW.normal);
        Vector3 projectSum = (projectClockwise + projectWindershins).normalized;


        if (drawCast)
        {
            Debug.DrawRay(wallBlockW.point, wallBlockW.normal, Color.yellow, 1.0f);
            
            Debug.DrawRay(wallBlockC.point, projectClockwise  , Color.cyan, 1.0f);
            Debug.DrawRay(wallBlockW.point, projectWindershins, Color.magenta, 1.0f);
        }

        Vector3 toReturn = (projectSum.Equals(Vector3.zero) ? movedir3 : projectSum).normalized;
        if (drawCast) Debug.DrawRay(body.position, toReturn, Color.red, 1.0f);

        return toReturn;

        //return movedir3;

        //if   (projectClockwise.Equals(Vector3.zero)) return (projectWindershins.Equals(Vector3.zero))                                                  ? dir3true         : projectWindershins;       
        //else                                         return (projectWindershins.Equals(Vector3.zero) || (projectClockwise.Equals(projectWindershins))) ? projectClockwise : dir3true;
    }


    RaycastHit castInDirection(Vector3 direction, float checkDist, bool drawCast = false, Color castColor = default(Color))
    {
        RaycastHit hit;
        Ray r = new Ray(body.position, direction);
        Physics.Raycast(r, out hit, checkDist, LayerMask.GetMask("Default"), QueryTriggerInteraction.UseGlobal);

        if (drawCast) 
        {
            Debug.DrawRay(body.position, direction * ((hit.collider == null) ? checkDist : hit.distance), castColor, .5f);
            Debug.DrawRay(hit.point, hit.normal, Color.yellow, .5f);        
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
    void updateTargetAngle(){ targetAngle += .01f; }
    void updateTargetAngle(Vector2 dir) { targetAngle = SharedLib.angleBetweenVectors(Vector2.right, dir); }

}
