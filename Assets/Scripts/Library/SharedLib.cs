using UnityEngine;
using UnityEngine.UIElements;

public class SharedLib
{
    //----------------------------- arrays

    public static int LoopIndex(int index, int length)
    {
        int newIndex = index;

        while (newIndex < 0)
            newIndex += length;
        if (newIndex >= length)
            newIndex = newIndex % length;

        return newIndex;
    }

    //----------------------------- 2D rotation in 3D space (ignore z)

    public static Vector2 rotateVector2(float angle, Vector2 startV) { return rotateVector2eul(angle, startV); }
    public static Vector2 rotateVector2eul(float angle, Vector2 startV) { return rotateVector2rad(Mathf.Deg2Rad * angle, startV); }

    //applies the rotation matrix ( [[cosA, -sinA], [sinA, cosA]] ) to the vector.
    public static Vector2 rotateVector2rad(float angleR, Vector2 startV)
    {
        return new Vector2(
            Mathf.Cos(angleR) * startV.x - Mathf.Sin(angleR) * startV.y,
            Mathf.Sin(angleR) * startV.x + Mathf.Cos(angleR) * startV.y);
    }


    //Gives the angle of from <0,1> to [target-Origin]
    public static float angleToTarget(Vector2 origin, Vector2 target)
    {
        return Mathf.Atan2(target.y - origin.y, target.x - origin.x) * Mathf.Rad2Deg + 90;
    }

    //

    //Returns the Angle between two vectors.
    public static float angleBetweenVectors(Vector2 aa, Vector2 bb) { return Mathf.Atan2(CrossProduct(aa,bb), DotProduct(aa,bb)) ; }
    private static float DotProduct  (Vector2 aa, Vector2 bb) { return aa.x * bb.x + aa.y * bb.y; }
    private static float CrossProduct(Vector2 aa, Vector2 bb) { return aa.x * bb.y - aa.y * bb.x; }


    //Clamps a angle to the degree bounds of 0<=x<360
    public static float simplifyEuler(float e)
    {
        if(e<0)
            e += 360f;
        e %= 360f;
        return e;
    }
    
    //Takes an angl(radians) and returns a vector of length 1 at that angle on the XZ plane)
    public static Vector3 angleToVector(float angle) { return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized; }


    //Takes a vector and returns that vector's grade in degrees (that is, the angle Y forms with the XZ plane.)
    public static float vectorToGrade(Vector3 normal) {
        float xz = Mathf.Sqrt((normal.x * normal.x) + (normal.z * normal.z));
        float beta_rad = Mathf.Atan(normal.y / xz);
        float alpha_deg = 90 - Mathf.Rad2Deg * beta_rad;
        return alpha_deg;
    }






    //takes an origin, a direction, and optionally a distance, layermask, and color.
    public static RaycastHit castInDirection(Vector3 origin, Vector3 direction                                                                           ) { return castInDirection( origin, direction, direction.magnitude , "Default"              ) ;}
    public static RaycastHit castInDirection(Vector3 origin, Vector3 direction,                                          Color castColor = default(Color)) { return castInDirection( origin, direction, direction.magnitude , "Default", castColor   ) ;}
    public static RaycastHit castInDirection(Vector3 origin, Vector3 direction, float dist,                              Color castColor = default(Color)) { return castInDirection( origin, direction, dist                , "Default", castColor   ) ;}
    public static RaycastHit castInDirection(Vector3 origin, Vector3 direction, float dist, string layerMask="Default",  Color castColor = default(Color)) {
        RaycastHit hit;
        Ray r = new Ray(origin, direction);
        bool success = Physics.Raycast(r, out hit, dist, LayerMask.GetMask(layerMask), QueryTriggerInteraction.UseGlobal);

        if (!castColor.Equals(default(Color))) Debug.DrawRay( origin, direction * (success?hit.distance:dist), castColor, .5f);

        return hit;
    }

    //Takes an origin, and facing angle, a fov angle, a distance and returns a raycastHit[3] of raycasts windershins to, at, and clockwise off the facing angle (seporated by the FOV
    public static RaycastHit[] castWFC(Vector3 origin, float facing, float fov, float dist, bool drawCast) { return castWFC(origin, facing, fov, dist, "Default", drawCast); }
    public static RaycastHit[] castWFC(Vector3 origin, float facing, float fov, float dist, string layerMask="Default", bool drawCast=false)
    {        
		float windershinsAngle = facing + fov;
        float clockwiseAngle   = facing - fov;

		Vector3 dir3windershins = angleToVector(Mathf.Deg2Rad * windershinsAngle    );
        Vector3 dir3facing      = angleToVector(Mathf.Deg2Rad * facing              );
		Vector3 dir3clockwise   = angleToVector(Mathf.Deg2Rad * clockwiseAngle      );
        
        RaycastHit[] hitWFC = new RaycastHit[3];
        if (drawCast) {
            hitWFC[0] = castInDirection(origin, dir3windershins  , dist, layerMask, Color.blue);
		    hitWFC[1] = castInDirection(origin, dir3facing       , dist, layerMask, Color.cyan);
		    hitWFC[2] = castInDirection(origin, dir3clockwise    , dist, layerMask, Color.green);
        }
        else {
            hitWFC[0] = castInDirection(origin, dir3windershins  , dist, layerMask);
		    hitWFC[1] = castInDirection(origin, dir3facing       , dist, layerMask);
		    hitWFC[2] = castInDirection(origin, dir3clockwise    , dist, layerMask);
        }

        return hitWFC;
    }







}
