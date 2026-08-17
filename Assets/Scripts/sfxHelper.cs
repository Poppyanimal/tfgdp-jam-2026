using UnityEngine;

public class sfxHelper : MonoBehaviour
{
    public sfxPlayer leftFoot, rightFoot, umbrellaSwing, umbrellaCharge, umbrellaShoot, hurt, hurtalt, die,
    ready, jump, idle;
    public bool endIdleOnDeath = false;
    
    public void leftFootSFX() { leftFoot.play(); }
    public void rightFootSFX() { rightFoot.play(); }
    public void swingSFX() { umbrellaSwing.play(); }
    public void chargeSFX() { umbrellaCharge.play(); }
    public void shootSFX() { umbrellaShoot.play(); }
    public void hurtSFX() { hurt.play(); }
    public void hurtaltSFX() { hurtalt.play(); }
    public void dieSFX() { die.play(); if(endIdleOnDeath) idle.stop(); }

    public void readySFX() { ready.play(); }
    public void jumpSFX() { jump.play(); }
    public void idleSFX() { idle.play(); }
}
