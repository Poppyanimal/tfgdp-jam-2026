using System;
using UnityEngine;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class InteractableAmmo: EmptyInteractionInteractable
{
	int ammo_amount = 2;

	public override void activate() {

		PlayerController pc= Control.Player.GetComponent<PlayerController>();
		pc.ammoUp(ammo_amount);
	}

	//To Generalize this for special memory classes
	public override void deactivate() {
		if (Interaction_State != INTERACTION_STATE.PREVENT_EXHAUSTION) {
			Interaction_State = INTERACTION_STATE.EXHAUSTED;
		}
		else {
		Interaction_State = INTERACTION_STATE.TARGETING;
		}
	}


}
