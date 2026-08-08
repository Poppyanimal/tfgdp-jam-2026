using UnityEngine;
using static Unity.Cinemachine.CinemachineSplineDolly;

public class EnemySpinner : Enemy
{
    const float base_rotation_speed=360;


	public override void Start() {
        base.Start();
	    force_multiplier    =  14f    ;
        motion_drag 		=  4.8f	  ; 
        move_speed          =  0.62f  ;
	}


    override public void Update() {
        base.Update();
        decideMovement();
    }

	override public void doInheritorSpecificUpdate() {
        decideMovement();
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
        targetAngle+= base_rotation_speed   *Time.deltaTime;//+Mathf.Sin(Time.frameCount); 
        RotateInAngleDirection(targetAngle);

//        //The little ball isn't rotating the way I want it to, ~~but it is rotating~~.
////        rotationBody.transform.rotation= Quaternion.LookRotation( Vector3.ProjectOnPlane(SharedLib.angleToVector3(AngleOfRotation), new Vector3(1,.5f,1).normalized).normalized*3 );
//        
    }







}
