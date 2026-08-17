using System;
using UnityEngine;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class InteractableMemory: EmptyInteractionInteractable
{
	public ScriptableMemoryScript memoryScript;
    public override void Start()
    {
		base.Start();
		Interaction_Prompt = "[Z] Probe Memory";
    }


	public override void activate()
	{
		Control.incrementMemorysSeen();
		
		Control.playMemory(memoryScript);
		deactivateAllParticles();
		disableCollision(GetComponent<Collider>());
		ambience?.Stop();
	}


}
