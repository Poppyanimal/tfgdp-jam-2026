using UnityEngine.Events;
using Unity;

public class GlobalEvents
{
    static GlobalEvents single;

    public UnityEvent whatever;

    public GlobalEvents get()
    {
        if(single == null)
            single = new();
        return single;
    }

    GlobalEvents()
    {
        
    }
}
