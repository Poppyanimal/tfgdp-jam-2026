using UnityEngine;
using static Unity.Cinemachine.CinemachineSplineDolly;

public class EnemySpinner : Enemy
{
    const float base_rotation_speed     =500    ,
                stun_duration           =  7    ;
    float stunTimer;

	public override void Start() {
        base.Start();
	    force_multiplier    =  20f    ;
        motion_drag 		=   3.8f  ; 
        move_speed          =   0.62f ;
	}

    override public void Update() {
        base.Update();
        if (stunTimer<=0) decideMovement();
    }

	public override void incrementCounters() {
		base.incrementCounters();
        if(stunTimer>0) stunTimer-= 1*Time.deltaTime;
	}

     void decideMovement() { 
        if(playerIsInSight) { 
            MoveTowardPlayer();
        }
        Wander();
        
	}

    void MoveTowardPlayer() {
        MoveInDirection(lastKnownPlayerLocation-body.position);   
    }

    void Wander() {
        TargetAngle+= base_rotation_speed   *Time.deltaTime;//+Mathf.Sin(Time.frameCount); 
        RotateInAngleDirection(TargetAngle);

//        //The little ball isn't rotating the way I want it to, ~~but it is rotating~~.
////        rotationBody.transform.rotation= Quaternion.LookRotation( Vector3.ProjectOnPlane(SharedLib.angleToVector3(AngleOfRotation), new Vector3(1,.5f,1).normalized).normalized*3 );
//        
    }

	public override void getHurt(GameObject attacker) {
        Debug.Log("Get Hurt");
        takeDamage(1);
        stun(stun_duration);
        knockback(attacker.gameObject.transform.position);
        
	}

    void stun(float duration) {
        if (stunTimer<0) stunTimer=0;
        stunTimer+=duration;
    }

    void knockback(Vector3 away) {
        Vector3 dir = (body.position-away).normalized;
        RaycastHit ground;
        Physics.Raycast(body.position, Vector3.down, out ground, 3f);

        body.AddForce( Vector3.ProjectOnPlane(dir,(ground.collider!=null?ground.normal:Vector3.up)).normalized*force_multiplier*0.75f, ForceMode.Impulse);
    }





}
