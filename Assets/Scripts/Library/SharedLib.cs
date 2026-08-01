using UnityEngine;

public class SharedLib
{
    //----------------------------- arrays

    public static int LoopIndex(int index, int length)
    {
        int newIndex = index;
        
        while(newIndex < 0)
            newIndex += length;
        if(newIndex >= length)
            newIndex = newIndex % length;

        return newIndex;
    }

    //----------------------------- 2D rotation in 3D space (ignore z)

    public static Vector2 rotateVector2(float angle, Vector2 startV)
    {
        return rotateVector2eul(angle, startV);
    }
    public static Vector2 rotateVector2eul(float angle, Vector2 startV)
    {
        float angleR = angle * Mathf.Deg2Rad;
        return rotateVector2rad(angleR, startV);
    }

    public static Vector2 rotateVector2rad(float angleR, Vector2 startV)
    {
        return new Vector2(
            Mathf.Cos(angleR)*startV.x - Mathf.Sin(angleR)*startV.y,
            Mathf.Sin(angleR)*startV.x + Mathf.Cos(angleR)*startV.y);
    }

    public static float angleToTarget(Vector2 origin, Vector2 target)
    {
        return Mathf.Atan2(target.y - origin.y, target.x - origin.x) * 180 / Mathf.PI + 90;
    }

    public static float simplifyEuler(float e)
    {
        if(e<0)
            e += 360f;
        e %= 360f;
        return e;
    }
    
}
