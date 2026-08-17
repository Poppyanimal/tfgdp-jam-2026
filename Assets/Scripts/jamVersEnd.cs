using System.Collections;
using UnityEngine;

public class jamVersEnd : MonoBehaviour
{
    public float delay = 1.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GlobalEvents.get().memory_threshold_reached.AddListener(end);
    }

    public void end()
    {
        StartCoroutine(endDelay());
    }

    IEnumerator endDelay()
    {
        yield return new WaitForSeconds(delay);
        GetComponent<Animator>().SetTrigger("end");
    }


    public void closeGame()
    {
        Application.Quit();
    }
}
