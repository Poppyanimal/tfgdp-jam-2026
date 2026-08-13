using UnityEngine;
using ge = GlobalEvents;

public class PlayerVars
{
    static PlayerVars single;
    public int maxHealth;
    int health, ammo;

    public static PlayerVars get()
    {
        if(single == null)
            single = new();
        return single;
    }
    PlayerVars()
    {
        health = 5;
        maxHealth = health;
        ammo = 5;
        ge.get().useAmmo.AddListener(useAnAmmo);
    }

    public int getAmmo() { return ammo; }

    public void modAmmo(int a)
    {
        ammo += a;
        if(ammo <= 0)
            ammo = 0;

        ge.get().ammoChanged.Invoke();
    }

    public void useAnAmmo()
    {
        modAmmo(-1);
    }

    public int getHealth() { return health; }
    public float getHealthRatio() { return health / (float) maxHealth; }
    public void modhealth(int h)
    {
        health += h;
        if(health > maxHealth)
            health = maxHealth;
        if(health <= 0)
        {
            health = 0;
            ge.get().playerDied.Invoke();
        }
        Debug.Log("player health changed to: "+health);



    }

}
