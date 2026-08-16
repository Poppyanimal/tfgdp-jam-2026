using UnityEngine;

public class spinDollAnimHelper : MonoBehaviour
{
    public EnemySpinner spin;
    public void resolveAttack() { spin.resolveAttack(); }
    public void resolveStun() { spin.resolveStun(); }
}
