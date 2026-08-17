using System;
using UnityEngine;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class InteractableHeal: EmptyInteractionInteractable
{
    public override void Start()
    {
		base.Start();
		Interaction_Prompt = "[Z] Heal From Stray Font";
    }
	public override void activate()
	{
		FindFirstObjectByType<PlayerController>().heal();
		disableCollision(GetComponent<Collider>());
		deactivateAllParticles();
	}



}
