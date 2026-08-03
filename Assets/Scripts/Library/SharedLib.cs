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
    public static float angleBetweenVectors(Vector2 aa, Vector2 bb)
    {
        return Mathf.Atan2(CrossProduct(aa,bb), DotProduct(aa,bb)) ;
    }
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
    
    public static Vector3 angleToVector(float angle)
    {
        return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

    }

}
