using UnityEngine.Events;
using Unity;

public class GlobalEvents
{
    static GlobalEvents single;
    public UnityEvent playerAttackResolved, ammoChanged, playerHeal, playerHurt, playerDied, useAmmo, playerSpawnProjectile;
    public UnityEvent hitStop, explosionHitStop;
    public UnityEvent doFade, endFade;

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
        playerHeal = new();
        playerHurt = new();
        playerDied = new();
        useAmmo = new();
        playerSpawnProjectile = new();

        hitStop = new();
        explosionHitStop = new();
        
        doFade = new();
        endFade = new();
    }

}
