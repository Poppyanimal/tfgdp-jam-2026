using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class umbrellaProjectile : MonoBehaviour
{
    public Animator anim;
    public GameObject sphere_particles, explosion_particles;
    public float timeBeforeDecay = 10f, emergencyExplodeTimer = 3f;
    public CinemachineImpulseSource impulse;
    public float impulseForce = 3f;
    public sfxPlayer explodeSFX;

    Coroutine emergencyTimer;

    void Start()
    {
        emergencyTimer = StartCoroutine(fallbackExplode());
    }

    void OnCollisionEnter(Collision collision)
    {
        explode();
    }

    void explode()
    {
        if(emergencyTimer != null)
            StopCoroutine(emergencyTimer);
        
        stopSphereParticles();
        playExplosion();
        explodeSFX.play();
        StartCoroutine(decayAndDestroy());
        anim.SetTrigger("explode");
        gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        impulse.GenerateImpulseWithForce(impulseForce);
        GlobalEvents.get().explosionHitStop.Invoke();
    }

    void stopSphereParticles()
    {
        ParticleSystem[] parts = sphere_particles.GetComponentsInChildren<ParticleSystem>();
        foreach(ParticleSystem part in parts)
        {
            part.Stop();
        }
    }

    void playExplosion()
    {
        ParticleSystem[] parts = explosion_particles.GetComponentsInChildren<ParticleSystem>();
        foreach(ParticleSystem part in parts)
        {
            part.Play();
        }
    }

    IEnumerator decayAndDestroy()
    {
        yield return new WaitForSeconds(timeBeforeDecay);
        Destroy(gameObject);
    }

    IEnumerator fallbackExplode()
    {
        yield return new WaitForSeconds(emergencyExplodeTimer);
        explode();
    }
}
