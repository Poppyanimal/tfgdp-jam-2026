using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Rigidbody body;
    public GameObject rotationBody;

    RaycastHit[] scanSweep;
    const float half_fov=90, scan_distance=50;
    const int scan_ray_density=10;

    public bool playerIsInSight;
    GameObject PlayerSeen { get; set; }
    public Vector3 lastKnownPlayerLocation= new Vector3(1000,1000,1000); // The default vector is this as a hacky way of making sure the enemy doesn't start pathfinding to the default LKPL immediately
    public const float find_player_distance = 30,
                       track_player_distance= 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        getComponentFields();
    }

    void getComponentFields() {
        body=GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        scanEnvironment();
        lookForPlayer();
        inheritorSpecificUpdate();
    }

    void scanEnvironment() {
        float lookAtAngle= body.rotation.eulerAngles.y;
        float[] scanAngles   = new float[scan_ray_density*2+1];

        for (int ii = 0; ii<scan_ray_density; ii+=1) { scanAngles[                       ii]= lookAtAngle - ((scan_ray_density-ii)*half_fov/scan_ray_density); }
        for (int ii = 0; ii<scan_ray_density; ii+=1) { scanAngles[ (scan_ray_density+1) +ii]= lookAtAngle + (                   ii*half_fov/scan_ray_density); }
        scanAngles[scan_ray_density]= lookAtAngle;


        LayerMask maskLayer= LayerMask.GetMask("Default")+ LayerMask.GetMask("Player") ;
        scanSweep = SharedLib.scanAngleSweep(body.position, scanAngles, scan_distance, maskLayer );

        //int itt = 0;
        //foreach (float angle in scanAngles) {
        //    Debug.DrawRay(body.position, SharedLib.angleToVector3(angle) * Mathf.Max(1,scanSweep[itt].distance), Color.HSVToRGB(itt * 5 / 360f, 1, 1));
        //    itt += 1;
        //}

    }

    void lookForPlayer() {
        playerIsInSight=false;
        //Debug.Log("Start Scan");

        foreach (RaycastHit hit in scanSweep){
            if (hit.collider!=null && 1<<hit.transform.gameObject.layer==LayerMask.GetMask("Player") && hit.distance<find_player_distance) {
                playerIsInSight=true; 
                PlayerSeen=hit.transform.gameObject;    
            }   
        }
        if (playerIsInSight) {
            Rigidbody playerBody;
            PlayerSeen.TryGetComponent<Rigidbody>(out playerBody);
            lastKnownPlayerLocation= playerBody.position;
        }
        else { PlayerSeen=null;}
    }

    virtual public void inheritorSpecificUpdate() { }

}
