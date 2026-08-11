using System.Collections;
using UnityEngine;
using static Unity.Cinemachine.CinemachineSplineDolly;

public class EnemyPersistent : EnemySpinner
{
    const float wanderDuration=10;
    public bool inWanderFrames=false;
    Vector3 wanderDir;



	public override void Start() {
        base.Start();
	    force_multiplier    =  10f    ;
        motion_drag 		=   6.8f  ; 
        move_speed          =   0.12f ;
	}

    override public void Update() {
        base.Update();
        if (!inStunFrames) decideMovement();
    }

    protected override void decideMovement() { 
        if      (playerIsInSight)                                                             MoveTowardPlayer();
        else if ((lastKnownPlayerLocation-body.position).magnitude< track_player_distance )   MoveTowardPlayer();
        else if (!inWanderFrames)                                                             AssignWander();
        else                                                                                  Wander();
	}

    override public void MoveTowardPlayer(){
        MoveInDirection  (  lastKnownPlayerLocation-body.position            );   

        RotateInDirection(( lastKnownPlayerLocation-body.position).normalized);
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
		MoveInDirection(wanderDir);
        RotateInDirection(wanderDir);
	}

    
    IEnumerator inWanderFrameTimer(float duration) {
        yield return new WaitForSeconds(duration);
        inWanderFrames= false;
    }


}
