using UnityEngine;

public class sfxPlayer : MonoBehaviour
{
    public AudioClip[] clips;
    [Range(.5f, 2f)]
    public float basePitch = 1f;
    const float pitchMin = .8f, pitchMax = 1.2f;
    AudioSource source;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        source = GetComponent<AudioSource>();
    }


    public void playRandomClip()
    {
        AudioClip c = clips[Mathf.RoundToInt(Random.Range(0, clips.Length-1))];
        source.Stop();
        source.clip = c;
        source.pitch = Random.Range(pitchMin * basePitch, pitchMax * basePitch);
        source.Play();
    }

    public void play() { playRandomClip(); }
}
