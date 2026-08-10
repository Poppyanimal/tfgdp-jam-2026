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
        if      (playerIsInSight)                                                             MoveTowardPlayer();
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
        anims.SetBool("walking", true);
        MoveInDirection  (  lastKnownPlayerLocation-body.position            );   

        RotateInDirection(( lastKnownPlayerLocation-body.position).normalized);
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

    void AssignWander() {
        Debug.Log("Wandering");
        wanderDir= new Vector3 ( Random.Range(-10,10), 0.0f, Random.Range(-10,10) ).normalized;
        if (wanderDir.Equals(Vector3.zero)) wanderDir= Vector3.forward;

        Wander();

        inWanderFrames=true;
        StartCoroutine(inWanderFrameTimer(wanderDuration));

    }
	public override void Wander() {
        anims.SetBool("walking", true);
		MoveInDirection(wanderDir);
        RotateInDirection(wanderDir);
	}

    
    IEnumerator inWanderFrameTimer(float duration) {
        yield return new WaitForSeconds(duration);
        inWanderFrames= false;
    }


}
