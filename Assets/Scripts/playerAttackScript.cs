using UnityEngine;
using ge = GlobalEvents;

public class playerAttackScript : MonoBehaviour
{
    public void attackHasResolved()
    {
        ge.get().playerAttackResolved.Invoke();
    }
}
