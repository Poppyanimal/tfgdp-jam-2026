using UnityEngine;

public class Enemy : MonoBehaviour
{
    Rigidbody body;

    const float force_multiplier	=	 8		, 
				motion_drag			=	 4.5f	,
                move_speed          =    0.8f   ;

    RaycastHit[] scanSweep;
    const float half_fov=90, scan_distance=50;
    const int scan_ray_density=10;

    bool playerSeen;
    GameObject playerInSight;
    Vector3 lastKnownPlayerLocation= new Vector3(1000,1000,1000);
    const float find_player_distance = 20,
                track_player_distance= 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        getComponentFields();
    }

    void getComponentFields() {
        body=GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        scanEnvironment();
        findPlayer();
        decideMovement();
    }

    void scanEnvironment() {
        float lookAtAngle= body.rotation.eulerAngles.y;
        float[] scanAngles   = new float[scan_ray_density*2+1];

        for (int ii = 0; ii<scan_ray_density; ii+=1) { scanAngles[                       ii]= lookAtAngle - ((scan_ray_density-ii)*half_fov/scan_ray_density); }
        for (int ii = 0; ii<scan_ray_density; ii+=1) { scanAngles[ (scan_ray_density+1) +ii]= lookAtAngle + (                   ii*half_fov/scan_ray_density); }
        scanAngles[scan_ray_density]= lookAtAngle;


        LayerMask maskLayer= LayerMask.GetMask("Default")+ LayerMask.GetMask("Player") ;
        scanSweep = SharedLib.scanAngleSweep(body.position, scanAngles, scan_distance, maskLayer );

        int itt = 0;
        foreach (float angle in scanAngles) {
            Debug.DrawRay(body.position, SharedLib.angleToVector3(angle) * Mathf.Max(1,scanSweep[itt].distance), Color.HSVToRGB(itt * 5 / 360f, 1, 1));
            itt += 1;
        }

    }

    void findPlayer() {
        playerSeen=false;
        //Debug.Log("Start Scan");

        foreach (RaycastHit hit in scanSweep){
            if (hit.collider!=null && 1<<hit.transform.gameObject.layer==LayerMask.GetMask("Player") && hit.distance<find_player_distance) {
                playerSeen=true; 
                playerInSight=hit.transform.gameObject;    
            }   
        }
        if (playerSeen) {
            Rigidbody playerBody;
            playerInSight.TryGetComponent<Rigidbody>(out playerBody);
            lastKnownPlayerLocation= playerBody.position;
        }
        else { playerInSight=null;}
    }

	void decideMovement() { 
        if(playerSeen) { 
            MoveTowardPlayer();
        }
        else {
            Wander();
        }
	}

    void MoveTowardPlayer() {
        body.AddForce(  (lastKnownPlayerLocation-body.position)*force_multiplier, ForceMode.Force  );
        body.linearDamping=motion_drag;
        
        if (body.linearVelocity.magnitude > move_speed) body.linearVelocity= body.linearVelocity.normalized*move_speed;
    }


    void Wander() {

    }


}
