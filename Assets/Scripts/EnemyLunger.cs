using System.Collections;
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
    bool inLungeCooldown = false;


	public override void Start() {
        base.Start();
	    force_multiplier    =  10f    ;
        motion_drag 		=   6.8f  ; 
        move_speed          =   1f   ;
	}

    override public void Update() {
        base.Update();

        if (!inStunFrames) decideMovement();
    }

    void decideMovement() { 
        if      (inLungeCooldown)                                                             continueLunge();
        else if (playerIsInSight)                                                             MoveTowardPlayer();
        else if ((lastKnownPlayerLocation-body.position).magnitude< track_player_distance )   MoveTowardPlayer();
        else if (!inWanderFrames && !waitingOnWanderPause)                                    StartCoroutine(pauseThenWander(wanderPause));
        else if (!waitingOnWanderPause)                                                       Wander();
        else
        {
            anims.SetBool("walking", false);
            body.linearVelocity = Vector3.zero;
        }
	}

    override public void MoveTowardPlayer(){
        if(Vector3.Distance(lastKnownPlayerLocation, scanPoint.transform.position) <= lungeAtPlayerDistance && !inLungeCooldown)
        {
            //Debug.Log("lungecode check");
            if(playerInFace(lungeAtPlayerDistance))
            {
                startAttack();
                return;  
            } 
        }
        //Debug.Log("MoveTowardPlayer" + Time.realtimeSinceStartup);
        anims.SetBool("walking", true);

        Vector3 movementVector = lastKnownPlayerLocation-body.position;
        //Debug.Log("movementVector:"+movementVector+", "+movementVector.magnitude);

        MoveInDirection(movementVector);   

        RotateInDirection(movementVector.normalized);
    }

    void startAttack()
    {
        inLungeCooldown = true;
        Debug.Log("Lunge Triggered");
        anims.SetBool("walking", false);
        anims.SetTrigger("Attack");

        //TODO: prevent angle from being updated + move toward point + play animation
    }

    void continueLunge()
    {
        Debug.Log("Lunge continues");
        //todo
        //lunge movement
    }
    
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
        RotateInDirection(wanderDir);
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


}
