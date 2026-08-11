using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Cinemachine.CinemachineSplineDolly;

public class EnemyLunger : EnemySpinner
{
    //todo, if hit wall, cancel current wander and start a new one with reduced pause
    const float wanderDuration=10;
    public bool inWanderFrames=false;
    Vector3 wanderDir;
    public float wanderPause = 1f;
    public float wanderWallTurnTimer = 1f;
    public float lungeAtPlayerDistance = 1.5f;
    public float lungeSpeed = 2.5f;
    public float force_multiplier_override = 10f;
    bool inLungeCooldown = false;
    public List<ParticleSystem> particlesToStopOnDeath;

	public override void Start() {
        base.Start();
	    force_multiplier    =  force_multiplier_override    ;
        motion_drag 		=   6.8f  ; 
        move_speed          =   1f   ;
        health              =   2   ;
        player = FindFirstObjectByType<PlayerController>();
        playerBody = player.GetComponent<Rigidbody>();
        skipOriginalDecideMovement = true;
	}

    override public void Update() {
        base.Update();

        /*Debug.Log("lungeCooldown: "+inLungeCooldown+", playerinsight? "+playerIsInSight+", waitingonwanderpause? "+waitingOnWanderPause+", inwanderframes? "+inWanderFrames
            +", remember player? "+playerIsRemembered+", near last player location? "+((lastKnownPlayerLocation-scanPoint.transform.position).magnitude< track_player_distance));*/

        if (!inStunFrames && !isDead) decideMovement();
    }

    Coroutine pauseWanderCoro;
    protected override void decideMovement() { 
        if      (inLungeCooldown)                                                                               continueLunge();
        else if (playerIsInSight)                                                                               MoveTowardPlayer();
        else if ((lastKnownPlayerLocation-scanPoint.transform.position).magnitude< track_player_distance )      MoveTowardPlayer();
        else if (!inWanderFrames && !waitingOnWanderPause)                                                      pauseWanderCoro = StartCoroutine(pauseThenWander(wanderPause));
        else if (!waitingOnWanderPause)                                                                         Wander();
        else
        {
            anims.SetBool("walking", false);
            body.linearVelocity = Vector3.zero;
        }
	}

    Rigidbody playerBody;
    override public void MoveTowardPlayer(){
        Debug.Log("moving toward player, lunge state: "+inLungeCooldown+", distance: "+Vector3.Distance(lastKnownPlayerLocation, scanPoint.transform.position));
        if(Vector3.Distance(lastKnownPlayerLocation, scanPoint.transform.position) <= lungeAtPlayerDistance && !inLungeCooldown)
        {
            Debug.Log("lungecode check");
            if(playerInFace(lungeAtPlayerDistance))
            {
                startAttack();
                return;  
            } 
        }
        //Debug.Log("MoveTowardPlayer" + Time.realtimeSinceStartup);
        anims.SetBool("walking", true);

        Vector3 movementVector = playerBody.position-body.position;
        movementVector.y = 0;
        Debug.Log("moving toward player and failed lunge check, movementVector:"+movementVector+", "+movementVector.magnitude);

        MoveInDirection(movementVector);   

        RotateInDirectionY(movementVector.normalized);
    }

    Vector3 lungeDirection;
    void startAttack()
    {
        inLungeCooldown = true;
        Debug.Log("Lunge Triggered");
        anims.SetBool("walking", false);
        anims.SetTrigger("Lunge");
        lungeDirection = SharedLib.angleToVector3(lookAtAngle);

        //TODO: prevent angle from being updated + move toward point + play animation
    }

    bool doingLungeMovement = false;
    void continueLunge()
    {
        if(doingLungeMovement)
        {
            MoveInDirection(lungeDirection, lungeSpeed);
        }
        else
        {
            //todo
            //is doing nothing enough?
        }
    }

    //call these three from animator?
    public void doLungeMovement() { doingLungeMovement = true; }
    public void stopLungeMovement() { doingLungeMovement = false; }
    public void resolveLunge() { inLungeCooldown = false; doingLungeMovement = false; }
    //
    
    bool waitingOnWanderPause = false;
    IEnumerator pauseThenWander(float duration)
    {
        waitingOnWanderPause = true;
        anims.SetBool("walking", false);
        yield return new WaitForSeconds(duration);
        waitingOnWanderPause = false;
        AssignWander();
    }

    Coroutine wanderFrameCoro;
    void AssignWander(bool isWallTurn = false) {
        wanderDir= new Vector3 ( Random.Range(-10,10), 0.0f, Random.Range(-10,10) ).normalized;
        if (wanderDir.Equals(Vector3.zero)) wanderDir= Vector3.forward;

        if(!isWallTurn)
            Wander();

        inWanderFrames=true;
        if(wanderFrameCoro != null)
            StopCoroutine(wanderFrameCoro);
        wanderFrameCoro = StartCoroutine(inWanderFrameTimer(wanderDuration));

    }

    bool inTurnFromWallCooldown = false;
	public override void Wander() {
        anims.SetBool("walking", true);

        if(faceAgainstWall && !inTurnFromWallCooldown)
        {
            AssignWander(true);
            StartCoroutine(faceWallCooldown(wanderWallTurnTimer));
        }

		MoveInDirection(wanderDir);
        RotateInDirectionY(wanderDir);
	}

    IEnumerator faceWallCooldown(float duration)
    {
        inTurnFromWallCooldown = true;
        yield return new WaitForSeconds(duration);
        inTurnFromWallCooldown = false;
    }
    
    IEnumerator inWanderFrameTimer(float duration) {
        yield return new WaitForSeconds(duration);
        inWanderFrames= false;
    }

    public override void getHurt(GameObject attacker)
    {
        Debug.Log("Lunger got hurt");
        base.getHurt(attacker);
    }

    public override void die()
    {
        base.die();
        foreach(ParticleSystem p in particlesToStopOnDeath)
        {
            p.Stop();
        }
    }

}
