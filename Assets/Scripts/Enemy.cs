using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Rigidbody body;
    public GameObject rotationBody;
    public float force_multiplier    =  8f     ,
                 motion_drag 		 =	4.8f   ,
                 move_speed          =  0.62f  ,
                 angles_per_second = 5f;
    public float lookAtAngle {get;set;} =0;
    public float currentAngle=0;

    RaycastHit[] scanSweep;
    public float half_fov=90, scan_distance=50;
    public int scan_ray_density=10;

    public bool playerIsInSight, playerIsRemembered;
    GameObject PlayerSeen { get; set; }
    public Vector3 lastKnownPlayerLocation= new Vector3(1000,1000,1000); // The default vector is this as a hacky way of making sure the enemy doesn't start pathfinding to the default LKPL immediately
    public const float find_player_distance = 30,
                       track_player_distance= 15,
                       track_player_duration=  5;

    PlayerController player;
    public float activationDistance = 5f;
    protected Animator anims;
    public GameObject scanPoint;
    protected bool faceAgainstWall = false;
    public float wallCheckDistance = .3f;

    protected int health = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    virtual public void Start() {
        player = FindAnyObjectByType<PlayerController>();
        getComponentFields();
    }

    void getComponentFields() {
        body=GetComponent<Rigidbody>();
        anims=GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    virtual public void Update() {
        if(Vector3.Distance(body.position, player.body.position) > activationDistance)
        {
            anims.SetBool("closeToPlayer", false);
            //play idle no one found animation
            return;
        }
        
        anims.SetBool("closeToPlayer", true);

        //play moving animation
        scanEnvironment();
        checkForWall();
        lookForPlayer();
    }

    void scanEnvironment() {
        lookAtAngle    = scanPoint.transform.rotation.eulerAngles.y;
        float[] scanAngles   = new float[scan_ray_density*2+1];

        for (int ii = 0; ii<scan_ray_density; ii+=1) { scanAngles[                       ii]= lookAtAngle - ((scan_ray_density-ii)*half_fov/scan_ray_density); }
        for (int ii = 0; ii<scan_ray_density; ii+=1) { scanAngles[ (scan_ray_density+1) +ii]= lookAtAngle + (                   ii*half_fov/scan_ray_density); }
        scanAngles[scan_ray_density]= lookAtAngle;


        LayerMask maskLayer= LayerMask.GetMask("Default")+ LayerMask.GetMask("Player") ;
        scanSweep = SharedLib.scanAngleSweep(scanPoint.transform.position, scanAngles, scan_distance, maskLayer);

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
            if (hit.collider!=null && hit.transform.gameObject.layer==LayerMask.NameToLayer("Player") && hit.distance<find_player_distance) {
                playerIsInSight=true; 
                PlayerSeen=hit.transform.gameObject;   
                Debug.Log("Sees Player");
                break; 
            }   
        }
        if (playerIsInSight) {
            Rigidbody playerBody;
            PlayerSeen.TryGetComponent<Rigidbody>(out playerBody);
            lastKnownPlayerLocation= playerBody.position;
            playerIsRemembered=true;
        }
        else
        {
            if(forgetCoro != null)
                StopCoroutine(forgetCoro);
            forgetCoro = StartCoroutine( forgetPlayerLocation(track_player_duration));
        }
    }

    void checkForWall()
    {
        faceAgainstWall= false;
        RaycastHit[] hits = SharedLib.scanAngleSweep(scanPoint.transform.position, new float[]{lookAtAngle}, scan_distance, true);
        foreach(RaycastHit hit in hits)
        {
            if(hit.collider == null)
                break;
            
            //Debug.Log((hit.collider != null ? hit.collider : "null") + ", "+hit.distance+", "+hit.transform.gameObject.layer);
            if(hit.transform.gameObject.layer==0 && hit.distance <= wallCheckDistance)
                faceAgainstWall = true;
        }

        if(faceAgainstWall)
            Debug.Log("face against wall");
    }


    protected bool playerInFace(float dist)
    {
        bool inface = false;
        RaycastHit[] hits = SharedLib.scanAngleSweep(scanPoint.transform.position, new float[]{lookAtAngle, lookAtAngle + 5, lookAtAngle - 5}, scan_distance, "Player", true);
        foreach(RaycastHit hit in hits)
        {
            if(hit.collider == null)
                continue;
            Debug.Log(hit.collider + ", "+hit.distance+", "+hit.collider.gameObject.layer);
            if(hit.collider.gameObject.layer==LayerMask.NameToLayer("Player") && hit.distance <= dist)
                inface = true;
        }
        return inface;
    }

    Coroutine forgetCoro;
    IEnumerator forgetPlayerLocation(float duration) {
        yield return new WaitForSeconds(duration);
        //Debug.Log("forgot player location");
        if (!playerIsInSight) { 
            playerIsRemembered= false;
            PlayerSeen=null;
            lastKnownPlayerLocation= new Vector3 (1000,1000,1000);
        }
    }


	public void MoveInDirection (Vector3 direction) {
		body.AddForce(  direction*force_multiplier *60f*Time.deltaTime, ForceMode.Force  );
        body.linearDamping=motion_drag;

        if (body.linearVelocity.magnitude > move_speed) body.linearVelocity= body.linearVelocity.normalized*move_speed;
	}

    public void RotateInDirection(Vector3 direction) {
       Quaternion qFrom = rotationBody.transform.rotation;

		Quaternion qVel = Quaternion.LookRotation( direction.magnitude==0? Vector3.forward: direction);
		float velAngle= qVel.eulerAngles.y;
		float lookTowardAngle= direction.magnitude>0? velAngle:lookAtAngle;

		Quaternion qToward = Quaternion.AngleAxis( lookTowardAngle, Vector3.up);

		rotationBody.transform.rotation = Quaternion.RotateTowards(qFrom, qToward, angles_per_second * Time.deltaTime * 60f );
		currentAngle= rotationBody.transform.rotation.eulerAngles.y;

    }
    public void RotateInAngleDirection(float angle ) {
        RotateInDirection(  SharedLib.angleToVector3(SharedLib.angleToBoundedDegrees(angle))  );
    }

    virtual public void hurtPlayer(GameObject player   ) {  }
    virtual public void getHurt   (GameObject attacker ) {  }
        virtual public void die() { }

    public void takeDamage(int amount) { health -= amount ; if (health<=0) die(); }

	void OnCollisionEnter(Collision other) {
        //hurting the player should be done through trigger collisions marked as enemy attacks, the player will check for this

        /*int layer= other.gameObject.layer;
        //Debug.LogFormat( "{0}({1}) {2}", LayerMask.LayerToName(layer), layer, "Enemy");

        switch ( layer ) {
            case 7 : hurtPlayer(other.gameObject); Debug.Log("Hurt Player") ; break;
            default   : break;  
        }*/
	}

    void OnTriggerEnter(Collider other) {
        int layer= other.gameObject.layer;
        //Debug.LogFormat( "{0}({1}) {2}", LayerMask.LayerToName(layer), layer, "Enemy");

        switch ( layer ) {
            case 9 : getHurt(other.gameObject); Debug.Log("Hit by Player") ; break;
            default   : break;  
        }
	}

}
