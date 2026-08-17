using UnityEngine;

public class breakableObject : MonoBehaviour
{
    public GameObject modelToHide;
    public sfxPlayer sfx;

    Rigidbody body; Collider col;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 9)
        {
            body.isKinematic = true;
            col.enabled = false;
            modelToHide.SetActive(false);
            sfx?.play();
            ParticleSystem[] parts = GetComponentsInChildren<ParticleSystem>();
            foreach(ParticleSystem part in parts)
            {
                part.Play();
            }
        }
    }
}
