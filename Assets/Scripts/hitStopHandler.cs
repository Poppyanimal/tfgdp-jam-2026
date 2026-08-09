using System.Collections;
using UnityEngine;

public class hitStopHandler : MonoBehaviour
{
    public float standard_duration = 2/60f;
    public float explosion_duration = 5/60f;
    void Start()
    {
        GlobalEvents.get().hitStop.AddListener(doRegHitStop);
        GlobalEvents.get().explosionHitStop.AddListener(doExplHitStop);
    }

    public void doRegHitStop()
    {
        doHitStop(standard_duration);
    }

    public void doExplHitStop()
    {
        doHitStop(explosion_duration);
    }

    void doHitStop(float t)
    {
        if(stopping != null)
        {
            StopCoroutine(stopping);
            Time.timeScale = ogScale;
        }
        stopping = StartCoroutine(hitStop(t));
    }

    Coroutine stopping;
    float ogScale;
    IEnumerator hitStop(float time)
    {
        ogScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(time);
        Time.timeScale = ogScale;
    }
}
