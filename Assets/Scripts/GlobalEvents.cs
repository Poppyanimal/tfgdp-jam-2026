using UnityEngine.Events;
using Unity;

public class GlobalEvents
{
    static GlobalEvents single;
    public UnityEvent playerAttackResolved;

    public static GlobalEvents get()
    {
        if(single == null)
            single = new();
        return single;
    }

    GlobalEvents()
    {
        playerAttackResolved = new();
    }

}
