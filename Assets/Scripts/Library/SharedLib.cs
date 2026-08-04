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

    #region Angle to Angle :: 1

    //Bounds a angle to the degrees of 0 <= x < 360
    public static float angleToBoundedDegrees(float ee)  { return ((ee<0 ? 360f:0f) + ee) % 360f; }

	#endregion

    #region Angle to Vector :: 2 -1
    //Takes an angl(radians) and returns a vector of length 1 at that angle on the XZ plane)
    public static Vector3 angleToVector3     (float angle) { return new Vector3(Mathf.Sin(Mathf.Deg2Rad * angle), 0f, Mathf.Cos(Mathf.Deg2Rad * angle)).normalized; }
    private static Vector3 angleToVector3_rad(float angle) { return new Vector3(Mathf.Sin(                angle), 0f, Mathf.Cos(                angle)).normalized; }

    public static Vector3[] generateWFC(Vector3 origin, float facing, float fov) {
        float windershinsAngle = facing + fov;
        float clockwiseAngle = facing - fov;

        Vector3[] dir3WFC = new Vector3[3];
        dir3WFC[0] = angleToVector3_rad(Mathf.Deg2Rad * windershinsAngle);
        dir3WFC[1] = angleToVector3_rad(Mathf.Deg2Rad * facing);
        dir3WFC[2] = angleToVector3_rad(Mathf.Deg2Rad * clockwiseAngle);

        return dir3WFC;

    }

	#endregion

    #region Vector to Angle :: 3 - 2

    //takes 

    
    public static float vectorToAngle(Vector2 aa) { return angleBetweenVectors( aa, Vector2.right) ;}

    public static float angleBetweenVectors(Vector2 aa            ) { return Mathf.Rad2Deg * Mathf.Atan2(crossProduct(aa,Vector2.right), dotProduct(aa,Vector2.right)) ; }
    public static float angleBetweenVectors(Vector2 aa, Vector2 bb) { return Mathf.Rad2Deg * Mathf.Atan2(crossProduct(aa,bb           ), dotProduct(aa,bb           )) ; }
    private static float dotProduct  (Vector2 aa, Vector2 bb) { return aa.x * bb.x + aa.y * bb.y; }
    private static float crossProduct(Vector2 aa, Vector2 bb) { return aa.x * bb.y - aa.y * bb.x; }

    public static float vectorToGrade(Vector3 normal) {
        float xz = Mathf.Sqrt((normal.x * normal.x) + (normal.z * normal.z));
        float beta_rad = Mathf.Atan(normal.y / xz);
        float alpha_deg = 90 - Mathf.Rad2Deg * beta_rad;
        return alpha_deg;
    }

	#endregion

    #region Vector to Vector :: 2 ~2

    //Takes a vector and adds a third axis with a value of zero. Defaults to Y
    public static Vector3 vector2to3(Vector2 v2) { return new Vector3(v2.x,0f,v2.y); }
    public static Vector3 vector2to3(Vector2 v2, string axis) { 
        switch (axis) {
            case "x": case "X": return new Vector3  (0f, v2.x,v2.y);
            case "z": case "Z": return    (Vector3) v2;
            case "y": case "Y": return vector2to3(v2);
            default: Debug.LogWarning("Unexpected String Value: defaulted to \"Y\""); return vector2to3(v2);
        }
         }


    //Takes a vector and restricts it to its planar components by removing an axis. Defaults to XZ.
    public static Vector2 vector3to2(Vector3 v3) { return new Vector2(v3.x,   v3.z); }
    public static Vector2 vector3to2(Vector3 v3, string plane) {
        switch (plane) {
            case "xy": case "XY": return (Vector2) v3;
            case "yz": case "YZ": return new Vector2(v3.y, v3.z);
            case "xz": case "XZ": return vector3to2(v3);
            default: Debug.LogWarning("Unexpected String Value: defaulted to \"XZ\""); return vector3to2(v3);
        }
    }


	#endregion

	#region Angle on Angle :: 0

	#endregion

    #region Angle on Vector :: 1 -1

	//applies the rotation matrix ( [[cosA, -sinA], [sinA, cosA]] ) to the vector.
	public static Vector2 rotateVector2(float angle, Vector2 startV) { return rotateVector2rad(Mathf.Deg2Rad * angle, startV); }
    private static Vector2 rotateVector2rad(float angleR, Vector2 startV)
    {
        return new Vector2(
            Mathf.Cos(angleR) * startV.x - Mathf.Sin(angleR) * startV.y,
            Mathf.Sin(angleR) * startV.x + Mathf.Cos(angleR) * startV.y);
    }

	#endregion


    
    #region create RaycastHit :: 2 ~4

    //takes an origin, a direction, and optionally a distance, layermask, and color. Returns a Raycasthit for those values. Draws the ray if given a color.
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

    //Takes an origin, and facing angle, a fov angle, a distance. returns a RaycastHit[3] through generateWFC(...) and castInDirection(...). Draws all three of the Rays if passed a boolean.
    public static RaycastHit[] castWFC(Vector3 origin, float facing, float fov, float dist, bool drawCast) { return castWFC(origin, facing, fov, dist, "Default", drawCast); }
    public static RaycastHit[] castWFC(Vector3 origin, float facing, float fov, float dist, string layerMask="Default", bool drawCast=false)
    {        
		Vector3[] dir3WFC = generateWFC(origin, facing, fov);
        
        RaycastHit[] hitWFC = new RaycastHit[3];
        if (drawCast) {
            hitWFC[0] = castInDirection(origin, dir3WFC[0], dist, layerMask, Color.blue );
		    hitWFC[1] = castInDirection(origin, dir3WFC[1], dist, layerMask, Color.cyan );
		    hitWFC[2] = castInDirection(origin, dir3WFC[2], dist, layerMask, Color.green);
        }
        else {
            hitWFC[0] = castInDirection(origin, dir3WFC[0], dist, layerMask);
		    hitWFC[1] = castInDirection(origin, dir3WFC[1], dist, layerMask);
		    hitWFC[2] = castInDirection(origin, dir3WFC[2], dist, layerMask);
        }

        return hitWFC;
    }

	#endregion

}
