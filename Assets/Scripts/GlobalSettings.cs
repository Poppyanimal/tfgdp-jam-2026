using UnityEngine;

public class GlobalSettings
{
    static GlobalSettings single;

    public bool useModernControls = true;

    static public GlobalSettings get()
    {
        if(single == null)
            single = new();
        return single;
    }
}
