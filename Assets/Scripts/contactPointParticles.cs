using UnityEngine;

public class contactPointParticles : MonoBehaviour
{
    public ParticleSystem hitSparks;
    int triggerLayer = 2;
    public bool ignorePlayer = true;
    void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.layer == triggerLayer || (ignorePlayer && collider.gameObject.layer == LayerMask.GetMask("Player")))
            return;
            
        Debug.Log(LayerMask.LayerToName(collider.gameObject.layer) + ", "+Time.timeAsRational + ", "+ collider.gameObject.layer);

        Vector3 posApprox = collider.ClosestPoint(transform.position);
        Vector3 boundingApprox = collider.ClosestPointOnBounds(transform.position);
        Quaternion rotApprox = Quaternion.FromToRotation(posApprox, boundingApprox);
        
        hitSparks.transform.position = posApprox;
        hitSparks.transform.rotation = rotApprox;
        hitSparks.Play();
    }
}
