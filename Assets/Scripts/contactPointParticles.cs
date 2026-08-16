using UnityEngine;

public class contactPointParticles : MonoBehaviour
{
    public ParticleSystem hitSparks;
    int triggerLayer = 2, interactableLayer = 14;
    public bool ignorePlayer = true;
    public sfxPlayer contactSFX;


    void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.layer == triggerLayer || collider.gameObject.layer == interactableLayer || (ignorePlayer && collider.gameObject.layer == LayerMask.GetMask("Player")))
            return;
            
        //Debug.Log(LayerMask.LayerToName(collider.gameObject.layer) + ", "+Time.timeAsRational + ", "+ collider.gameObject.layer);

        Vector3 posApprox = collider.ClosestPoint(transform.position);
        Vector3 boundingApprox = collider.ClosestPointOnBounds(transform.position);
        Quaternion rotApprox = Quaternion.FromToRotation(posApprox, boundingApprox);
        
        hitSparks.transform.position = posApprox;
        hitSparks.transform.rotation = rotApprox;
        hitSparks.Play();
        contactSFX.play();
    }
}
