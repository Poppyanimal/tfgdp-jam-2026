using UnityEngine;
using static Unity.Cinemachine.CinemachineSplineDolly;

public class EnemySpinner : Enemy
{

    const float force_multiplier	=	 14     , 
		        motion_drag 		=	 4.8f	,
                move_speed          =    0.62f  ,
                rotation_speed      =    0.5f   ;
    float AngleOfRotation=0;
	



	override public void inheritorSpecificUpdate() {
        decideMovement();
    }

     void decideMovement() { 
        if(playerIsInSight) { 
            MoveTowardPlayer();
        }
        Wander();
        
	}

    void MoveTowardPlayer() {
        body.AddForce(  (lastKnownPlayerLocation-body.position)*force_multiplier, ForceMode.Force  );
        body.linearDamping=motion_drag;
        
        if (body.linearVelocity.magnitude > move_speed) body.linearVelocity= body.linearVelocity.normalized*move_speed;
    }

    void Wander() {
        AngleOfRotation    = SharedLib.angleToBoundedDegrees(AngleOfRotation+rotation_speed );


        //The little ball isn't rotating the way I want it to, ~~but it is rotating~~.
//        rotationBody.transform.rotation= Quaternion.LookRotation( Vector3.ProjectOnPlane(SharedLib.angleToVector3(AngleOfRotation), new Vector3(1,.5f,1).normalized).normalized*3 );
        rotationBody.transform.rotation= Quaternion.AngleAxis(AngleOfRotation, Vector3.up);
    }







}
