using UnityEngine;

public class EnemySpinner : Enemy
{

    const float force_multiplier	=	 8		, 
		        motion_drag 		=	 2.5f	,
                move_speed          =    0.8f   ;
    Vector3 AxisOfRotation= new Vector3 (-36.0f, 0, -20.7f);
    float AngleofRotation=0;
	



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
        Vector3 randVec= new Vector3 (Random.Range(-.1f, 0.3f),Random.Range(-.1f, .3f),Random.Range(-.1f,.3f) );
        Vector3 rbr= rotationBody.transform.rotation.eulerAngles;
        Vector3 sum= rbr+randVec;

        Quaternion randDelta = Quaternion.Euler(sum.x,sum.y, sum.z);

        //The little ball isn't rotating the way I want it to, ~~but it is rotating~~.
        rotationBody.transform.rotation= randDelta;
    }







}
