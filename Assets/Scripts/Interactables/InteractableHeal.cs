using System;
using UnityEngine;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class InteractableHeal: EmptyInteractionInteractable
{
	public override void activate() {
		PlayerController pc;
		if ( Control.Player.TryGetComponent<PlayerController>(out pc) )
			pc.heal();
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
