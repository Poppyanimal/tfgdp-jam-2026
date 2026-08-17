using UnityEngine;

public class ambientmusic : MonoBehaviour
{
    public float activationRange = 50f;
    PlayerController player;
    bool isActive = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        toggle(Vector3.Distance(player.transform.position, transform.position) <= activationRange);
    }

    void toggle(bool t)
    {
        if(t == isActive)
            return;
        
        AudioSource[] sources = GetComponentsInChildren<AudioSource>();
        
        isActive = t;

        if(isActive)
        {
            foreach(AudioSource s in sources)
            {
                s.Play();
            }
        }
        else
        {
            foreach(AudioSource s in sources)
            {
                s.Stop();
            }
        }

    }
}
