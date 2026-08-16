using System;
using UnityEngine;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class InteractableHeal: EmptyInteractionInteractable
{
    public override void Start()
    {
		base.Start();
		Interaction_Prompt = "[E] Heal From Stray Font";
		GlobalEvents.get().hidePrompt.Invoke();
    }
	public override void activate() {
		FindFirstObjectByType<PlayerController>().heal();
	}

	//To Generalize this for special memory classes
	public override void deactivate() {
		deactivateAllParticles();
		if (Interaction_State != INTERACTION_STATE.PREVENT_EXHAUSTION) {
			Interaction_State = INTERACTION_STATE.EXHAUSTED;
		}
		else {
		Interaction_State = INTERACTION_STATE.TARGETING;
		}
	}


}
