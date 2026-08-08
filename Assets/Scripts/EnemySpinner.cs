using UnityEngine;
using static Unity.Cinemachine.CinemachineSplineDolly;

public class EnemySpinner : Enemy
{
    const float base_rotation_speed=4;

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
        MoveInDirection(lastKnownPlayerLocation-body.position);
        
    }

    void Wander() {
        angleOfRotation+= (base_rotation_speed+Mathf.Sin(Time.frameCount))  *60f*Time.deltaTime; 
        RotateInAngleDirection(angleOfRotation);

//        //The little ball isn't rotating the way I want it to, ~~but it is rotating~~.
////        rotationBody.transform.rotation= Quaternion.LookRotation( Vector3.ProjectOnPlane(SharedLib.angleToVector3(AngleOfRotation), new Vector3(1,.5f,1).normalized).normalized*3 );
//        
    }







}
