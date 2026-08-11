using System.Collections;
using UnityEngine;

public class dogLungerHurtDetectionHelper : MonoBehaviour
{
    public EnemyLunger thisLunger;
    public float timeBetweenHits = .01f;

    void OnTriggerEnter(Collider other)
    {
        if(inIframes)
            return;
        int layer= other.gameObject.layer;

        switch ( layer ) {
            case 9 : thisLunger.getHurt(other.gameObject); Debug.Log("Hit by Player") ; StartCoroutine(iframeCoro()); break;
            default   : break;  
        }
    }

    bool inIframes = false;
    IEnumerator iframeCoro()
    {
        inIframes = true;
        yield return new WaitForSeconds(timeBetweenHits);
        inIframes = false;
    }
}
