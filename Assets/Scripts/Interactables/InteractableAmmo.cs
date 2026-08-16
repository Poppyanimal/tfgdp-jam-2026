using System;
using UnityEngine;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class InteractableAmmo: EmptyInteractionInteractable
{
	int ammo_amount = 2;
	
    public override void Start()
    {
		base.Start();
        Interaction_Prompt = "[E] Pick Up Lingering Charges";;
    }

    public override void activate() {

		FindFirstObjectByType<PlayerController>().ammoUp(ammo_amount);
		GlobalEvents.get().hidePrompt.Invoke();
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
