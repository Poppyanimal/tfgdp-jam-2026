using UnityEngine.Events;
using Unity;

public class GlobalEvents
{
    static GlobalEvents single;
    public UnityEvent playerAttackResolved, ammoChanged, playerHurt, playerDied, useAmmo;

    public static GlobalEvents get()
    {
        if(single == null)
            single = new();
        return single;
    }

    GlobalEvents()
    {
        playerAttackResolved = new();
        ammoChanged = new();
        playerHurt = new();
        playerDied = new();
        useAmmo = new();
    }

}
