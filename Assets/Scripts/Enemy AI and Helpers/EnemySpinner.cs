using System.Collections;
using UnityEngine;
using static Unity.Cinemachine.CinemachineSplineDolly;

public class EnemySpinner : Enemy
{
    public const float  stun_duration   =  7    ;
    const float base_rotation_speed     =500    ;
    public bool inStunFrames;
    public float player_distance_deadzone = .5f;
    bool inAttackCooldown; protected bool isDead; 
    public Rigidbody bodyDisableOnDeath; public Collider colliderDisableOnDeath;
    protected bool skipOriginalDecideMovement = false;
    const float speed = .62f;
    const float attack_speed_mod = .6f;

	public override void Start() {
        base.Start();
	    force_multiplier    =  20f    ;
        motion_drag 		=   3.8f  ; 
        move_speed          =   speed ;
        health = 3;
	}

    override public void Update() {
        base.Update();
        if (!inStunFrames && !isDead && !skipOriginalDecideMovement && !toofarfromplayer()) decideMovement();
    }

    virtual protected void decideMovement() { 
        if(playerIsInSight) { 
            MoveTowardPlayer();
        }
        Wander();
	}

    virtual public void MoveTowardPlayer() {
        if(Vector3.Distance(lastKnownPlayerLocation, body.position) < player_distance_deadzone)
        {
            if(!inAttackCooldown)
                startAttack();
            return;   
        }
        MoveInDirection(lastKnownPlayerLocation-body.position, true);   
    }

    void startAttack()
    {
        inAttackCooldown = true;
        Debug.Log("starting Attack!");
        anims.SetTrigger("Attack");
        move_speed = speed * attack_speed_mod;
    }

    virtual public void Wander() {
        if(!inStunFrames)
            body.angularVelocity *= 1f - Time.deltaTime;
        if(inAttackCooldown || inStunFrames || Vector3.Distance(body.position, player.body.position) > activationDistance)
            return;
        lookAtAngle+= base_rotation_speed   *Time.deltaTime;//+Mathf.Sin(Time.frameCount); 
        RotateInAngleDirection(lookAtAngle);   
    }

	public override void getHurt(GameObject attacker) {
        anims.SetTrigger("getHit");
        GlobalEvents.get().hitStop.Invoke();
        Debug.Log("Get Hurt");
        takeDamage(1);
        stun(stun_duration);
        knockback(attacker.gameObject.transform.position); 
	}

	public override void hurtPlayer(GameObject player) {
		//Debug.Log("Hurt Player");
        //stun(stun_duration*0.32f);
        //player should determine if they were hurt
        knockback(player.gameObject.transform.position, 0.42f);
	}

    public override void die()
    {
        base.die();
        isDead = true;
        anims.SetBool("Dead", true);
        colliderDisableOnDeath.enabled = false;
        bodyDisableOnDeath.constraints = RigidbodyConstraints.FreezeAll;
    }



    Coroutine stunCoro;
    void stun(float duration) {
        if(stunCoro != null)
            StopCoroutine(stunCoro);
        inStunFrames=true;
        stunCoro =  StartCoroutine(inStunFrameTimer(duration));
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

    public void resolveAttack() { inAttackCooldown = false; move_speed = speed; }
    
    public void resolveStun()
    {
        if(stunCoro != null)
            StopCoroutine(stunCoro);

        inStunFrames = false;
    }


    


}
