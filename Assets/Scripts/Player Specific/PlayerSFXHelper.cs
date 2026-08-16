using UnityEngine;

public class PlayerSFXHelper : MonoBehaviour
{
    public sfxPlayer leftFoot, rightFoot, umbrellaSwing, umbrellaCharge, umbrellaShoot, hurt, die;
    
    public void leftFootSFX() { leftFoot.play(); }
    public void rightFootSFX() { rightFoot.play(); }
    public void swingSFX() { umbrellaSwing.play(); }
    public void chargeSFX() { umbrellaCharge.play(); }
    public void shootSFX() { umbrellaShoot.play(); }
    public void hurtSFX() { hurt.play(); }
    public void dieSFX() { die.play(); }
}
