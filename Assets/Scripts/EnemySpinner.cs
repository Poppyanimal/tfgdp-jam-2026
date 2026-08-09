using System.Collections;
using UnityEngine;
using static Unity.Cinemachine.CinemachineSplineDolly;

public class EnemySpinner : Enemy
{
    const float base_rotation_speed     =500    ,
                stun_duration           =  7    ;
    bool inStunFrames;

	public override void Start() {
        base.Start();
	    force_multiplier    =  20f    ;
        motion_drag 		=   3.8f  ; 
        move_speed          =   0.62f ;
	}

    override public void Update() {
        base.Update();
        if (!inStunFrames) decideMovement();
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
    }

	public override void getHurt(GameObject attacker) {
        Debug.Log("Get Hurt");
        takeDamage(1);
        stun(stun_duration);
        knockback(attacker.gameObject.transform.position); 
	}

	public override void hurtPlayer(GameObject player) {
		Debug.Log("Hurt Player");
        stun(stun_duration*0.32f);
        knockback(player.gameObject.transform.position, 0.42f);
	}

    void stun(float duration) {
        inStunFrames=true;
        StartCoroutine(inStunFrameTimer(duration));
    }

    IEnumerator inStunFrameTimer(float duration) {
        yield return new WaitForSeconds(duration);
        inStunFrames= false;
    }

    void knockback(Vector3 away, float scale=0.75f) {
        Vector3 dir = (body.position-away).normalized;
        RaycastHit ground;
        Physics.Raycast(body.position, Vector3.down, out ground, 3f);

        body.AddForce( Vector3.ProjectOnPlane(dir,(ground.collider!=null?ground.normal:Vector3.up)).normalized*force_multiplier*scale, ForceMode.Impulse);
    }



    


}
