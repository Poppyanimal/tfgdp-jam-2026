using UnityEngine;

public class wolfAnimHelper : MonoBehaviour
{
    public EnemyLunger lunger;
    public void resolveAttack() { lunger.resolveAttack(); }
    public void resolveStun() { lunger.resolveStun(); }
    public void doLungeMovement() { lunger.doLungeMovement(); }
    public void stopLungeMovement() { lunger.stopLungeMovement(); }
    public void resolveLunge() { lunger.resolveLunge(); }
}
