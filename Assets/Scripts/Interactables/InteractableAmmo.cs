using System;
using UnityEngine;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class InteractableAmmo: EmptyInteractionInteractable
{
	int ammo_amount = 2;
	
    public override void Start()
    {
		base.Start();
        Interaction_Prompt = "[Z] Pick Up Lingering Charges";;
    }

    public override void activate()
	{

		FindFirstObjectByType<PlayerController>().ammoUp(ammo_amount);
		disableCollision(GetComponent<Collider>());
		deactivateAllParticles();
		ambience?.Stop();
	}

}
