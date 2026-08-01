using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    const float move_speed = 5f, max_rotate_speed = 20f, move_deadzone = .2f, turn_deadzone = 40f, angle_difference_for_cam_snap = 15;
    public GameObject rotationBody; float targetAngle; bool doRotation = true; float cached_input_angle;
    CinemachineBrain cam_brain; CinemachineCamera active_cam; CinemachineCamera cam_to_turnoff;
    public GameObject camera_tracking_point, ground_raycast_point; const float raycast_dist = .15f, raycast_offset = .1f;
    Rigidbody body;

    //
    //
    //
    
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

        RaycastHit hit;
        Ray r = new Ray(ground_raycast_point.transform.position, Vector3.down);
        Physics.Raycast(r, out hit, raycast_dist, LayerMask.GetMask("Default"), QueryTriggerInteraction.UseGlobal);
        if(hit.collider != null && hit.distance <= raycast_dist)
        {
            Vector3 pos = body.position;
            pos.y -= hit.distance - raycast_offset;
            body.position = pos;
            Vector3 linV = body.linearVelocity;
            linV.y = 0f;
            body.linearVelocity = linV;
        }

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
