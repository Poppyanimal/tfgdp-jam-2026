using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

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
    public  static Vector3 angleToVector3    (float angle) { return new Vector3(Mathf.Sin(Mathf.Deg2Rad * angle), 0f, Mathf.Cos(Mathf.Deg2Rad * angle)).normalized; }
    private static Vector3 angleToVector3_rad(float angle) { return new Vector3(Mathf.Sin(                angle), 0f, Mathf.Cos(                angle)).normalized; }
 

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
    public static Vector3 vector2to3(Vector2 v2, string axis="Y") { 
        switch (axis) {
            case "x": case "X": return new Vector3  (0f, v2.x,v2.y);
            case "z": case "Z": return    (Vector3) v2;
            case "y": case "Y": return new Vector3(v2.x,0f,v2.y);
            default: Debug.LogWarning("Unexpected String Value: defaulted to \"Y\""); return vector2to3(v2);
        }
         }


    //Takes a vector and restricts it to its planar components by removing an axis. Defaults to XZ.
    public static Vector2 vector3to2(Vector3 v3, string plane="XZ") {
        switch (plane) {
            case "xy": case "XY": return (Vector2) v3;
            case "yz": case "YZ": return new Vector2(v3.y, v3.z);
            case "xz": case "XZ": return new Vector2(v3.x, v3.z);;
            default: Debug.LogWarning("Unexpected String Value: defaulted to \"XZ\""); return vector3to2(v3);
        }
    }

    public static Vector3 vectorFlatten(Vector3 v3) { return new Vector3 (v3.x,0,v3.z);}


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



	#region create Rays
    public static Ray[] rayAngleSweep(Vector3 origin, float[] angles) {
        Ray[]     toReturn  = new Ray[angles.Length];
        for(int ii=0; ii<angles.Length; ii += 1) { 
            Vector3 direction= angleToVector3(angles[ii]);
            toReturn[ii]= new Ray(origin, direction);
            }
        return toReturn;
    }

	#endregion

	#region create RaycastHit :: 3 ~3

	//Takes an origin and an array of Angles and returns a horizontal sweep of Raycast Hits for the rays at those angles.
	public static RaycastHit[] scanAngleSweep(Vector3 origin, float[] angles, float dist,                              bool drawCast       ) { return scanAngleSweep(origin, angles, dist, "Default", drawCast);}
    public static RaycastHit[] scanAngleSweep(Vector3 origin, float[] angles, float dist, string layerMask="Default",  bool drawCast=false) {  return scanAngleSweep(origin, angles, dist, LayerMask.GetMask(layerMask), drawCast); }
    public static RaycastHit[] scanAngleSweep(Vector3 origin, float[] angles, float dist, LayerMask layerMask       ,  bool drawCast=false) {
        Vector3[] directions= new Vector3[angles.Length];
        for(int ii=0; ii<angles.Length; ii+=1) directions[ii]= angleToVector3(angles[ii]);       
        return scanSweep(origin, directions, dist, layerMask, drawCast);

    }

    //Takes an origin and an array of Directions and returns a horizontal sweep of Raycast Hits for the rays in those Directions*, *Y is floored at zero.
    public static RaycastHit[] scanSweep(Vector3 origin, Vector3[] directions, float dist,                              bool drawCast       ) { return scanSweep(origin, directions, dist, "Default", drawCast);}
    public static RaycastHit[] scanSweep(Vector3 origin, Vector3[] directions, float dist, string layerMask="Default",  bool drawCast=false ) { return scanSweep(origin, directions, dist, LayerMask.GetMask(layerMask), drawCast ); }
    public static RaycastHit[] scanSweep(Vector3 origin, Vector3[] directions, float dist, LayerMask layerMask,         bool drawCast=false ) {
        RaycastHit[] toReturn = new RaycastHit[directions.Length]; 

        for(int ii=0; ii<directions.Length; ii+=1) {
            directions[ii].y= 0f;
            Physics.Raycast( new Ray(origin, directions[ii]), out toReturn[ii], dist, layerMask, QueryTriggerInteraction.UseGlobal);
            if (drawCast) Debug.DrawRay(origin, directions[ii], Color.HSVToRGB(ii/4/directions.Length+.3f, .5f,.5f), .5f);
        }

        return toReturn;
    }


    //Takes an array of origins and a direction and returens a slice of Raycast Hits for rays in that direction starting from those origins.
    public static RaycastHit[] scanSlice(Vector3[] origins, Vector3 direction, float dist,                              bool drawCast       ) { return scanSlice(origins, direction, dist, "Default", drawCast);}
    public static RaycastHit[] scanSlice(Vector3[] origins, Vector3 direction, float dist, string layerMask="Default",  bool drawCast=false ) {
        RaycastHit[] toReturn = new RaycastHit[origins.Length]; 

        direction.y= Mathf.Max(0f,direction.y);
        for(int ii=0; ii<origins.Length; ii+=1) {
            Physics.Raycast( new Ray(origins[ii], direction), out toReturn[ii], dist, LayerMask.GetMask(layerMask), QueryTriggerInteraction.UseGlobal);
            if (drawCast) Debug.DrawRay(origins[ii], direction, Color.HSVToRGB(ii/4/origins.Length, .5f,.5f), .5f);
        }

        return toReturn;
    }













	#endregion

}
