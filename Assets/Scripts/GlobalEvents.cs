using UnityEngine.Events;
using Unity;

public class GlobalEvents
{
    static GlobalEvents single;

    public UnityEvent paused;
    public UnityEvent unpaused;
    public UnityEvent playerAttackResolved;

    public static GlobalEvents get()
    {
        if(single == null)
            single = new();
        return single;
    }

    GlobalEvents()
    {
        paused = new();
        unpaused = new();
        playerAttackResolved = new();
    }

    public static void setPauseState(bool state)
    {
        if(state)
            get().paused.Invoke();
        else
            get().unpaused.Invoke();
    }
}
