using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const float move_speed = 5f, max_rotate_speed = 20f, move_deadzone = .2f, turn_deadzone = 40f, angle_difference_for_cam_snap = 15;
    public GameObject rotationBody; float targetAngle; bool doRotation = true; float cached_input_angle;
    CinemachineBrain cam_brain; CinemachineCamera active_cam; CinemachineCamera cam_to_turnoff;
    public GameObject camera_tracking_point, ground_raycast_point;

    const float groundCheck = 1.5f, snap_offset = 1f, snap_min=.12f, snap_max=.15f;


    Rigidbody body;

  
    
    void Start()
    {
        body = GetComponent<Rigidbody>();
        cam_brain = FindFirstObjectByType<CinemachineBrain>();
        active_cam = (CinemachineCamera)cam_brain.ActiveVirtualCamera;
        cam_to_turnoff = active_cam;
        if(rotationBody != null)
            targetAngle = rotationBody.transform.rotation.eulerAngles.y;
    }


    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector2 inputdir = input.normalized;
        bool moving = input.magnitude > move_deadzone;

        if(GlobalSettings.get().useModernControls)
        {
            //preserve direction between cameras unless turn too much / stop moving
            float input_angle = SharedLib.angleToTarget(Vector2.up, inputdir);
            if(active_cam == (CinemachineCamera)cam_brain.ActiveVirtualCamera)
                cached_input_angle = input_angle;
            if(!moving || Mathf.Abs(cached_input_angle - input_angle) >= angle_difference_for_cam_snap)
                active_cam = (CinemachineCamera)cam_brain.ActiveVirtualCamera;

            Vector2 movementdir = skewByCamera(inputdir).normalized;

            //anim.SetBool("isMoving", moving);
            
            if(moving) 
                updateTargetAngle(movementdir);

            body.linearVelocity = new Vector3(movementdir.x, 0f, movementdir.y)  * move_speed + Vector3.up * body.linearVelocity.y;
        }
        else //tank controls
        {
            if(SharedLib.angleToTarget(Vector2.up, inputdir) <= turn_deadzone)
            {
                //do forward
            }
            else if(SharedLib.angleToTarget(Vector2.down, inputdir) <= turn_deadzone)
            {
                //and if started from neutral
                //do quickturn
            }
            else
            {
                //do turn
            }
        }

        if(!moving)
        {
            Vector3 linV = body.linearVelocity;
            linV.x = 0f;
            linV.y = 0f;
            body.linearVelocity = linV;
        }

        
        //if grounded, keep grounded and remove y velocity, snap if within certain distance of ground

        RaycastHit ground;
        Ray r = new Ray(body.position, Vector3.down);
        Physics.Raycast(r, out ground, groundCheck, LayerMask.GetMask("Default"), QueryTriggerInteraction.UseGlobal);
        if(ground.collider != null && ground.distance-snap_offset>snap_min) CheckAndDoGroundSnap(ground);
 
    }

    void CheckAndDoGroundSnap(RaycastHit ground)
    {
        if (ground.distance == 0) return;
        if (ground.normal.Equals(Vector3.up))
        {
            if (ground.distance - snap_offset < snap_max) SnapToGround(ground.distance - snap_offset);
        }
        else
        {
            float slopeDist = CalculateSlopeSnap(ground);
            if (slopeDist == 0) StayOnGround(); 
            else if (slopeDist < snap_max) SnapToGround(slopeDist);
            

        }   

    }

    void SnapToGround(float snap_dist)
    {
        Vector3 snap_pos = body.position;
        snap_pos.y -= snap_dist;
        body.position = snap_pos;

        Vector3 linV = body.linearVelocity;
        linV.y = Mathf.Max(0f, linV.y);
        body.linearVelocity = linV;
    }
    void StayOnGround()
    {
        Vector3 linV = body.linearVelocity;
        linV.y = Mathf.Max(0f, linV.y);
        body.linearVelocity = linV;
    }

    float CalculateSlopeSnap(RaycastHit ground)
    {
        
        float stair_size = .25f;

        //float xz = Mathf.Sqrt((ground.normal.x * ground.normal.x) + (ground.normal.z * ground.normal.z));
        //float beta_rad = Mathf.Atan(ground.normal.y / xz);
        //float alpha_deg = 90 - Mathf.Rad2Deg*beta_rad;
        //float k_one = stair_size * Mathf.Tan(Mathf.Deg2Rad*alpha_deg);
        //float feet_dist_adjust = ground.distance - k_one;
        
        Vector3 feet_pos = body.position; feet_pos.y -= snap_offset;
        Debug.DrawRay(feet_pos,      Vector3.down * (ground.distance - snap_offset), Color.red , 10f);
        Debug.DrawRay(ground.point,  ground.normal*stair_size                      , Color.blue, 10f);

        Vector3 secondpoint = ground.point;
        secondpoint += ground.normal * stair_size;
        Debug.DrawRay(secondpoint, Vector3.right * stair_size*2, Color.green, 10f);

        float imaginary_floor_dist = feet_pos.y - secondpoint.y;

        return imaginary_floor_dist;
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
