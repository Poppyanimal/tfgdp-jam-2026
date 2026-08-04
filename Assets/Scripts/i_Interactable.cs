using UnityEngine;

public abstract class a_Interactable : MonoBehaviour { 
	public enum   INTERACTION_STATE { UNTARGETED, TARGETING, UNTARGETING, TARGETED, ACTIVATING, DEACTIVATING, ACTIVE, FORCE_CANCEL_ACTIVE, EXHAUSTED}
	public INTERACTION_STATE Interaction_State { set; get; } = INTERACTION_STATE.UNTARGETING;

	public KeyCode InteractionKey   { set; get;} = KeyCode.E;

	public string InteractionPrompt { get; } = "Press E (lying) to Interact";

	public void target()              { Interaction_State= INTERACTION_STATE.TARGETING				;}
	public void endTarget()           { Interaction_State= INTERACTION_STATE.UNTARGETING			;}
	public void interact()            { Interaction_State= INTERACTION_STATE.ACTIVATING				;}
	public void continueInteract()    { Interaction_State= INTERACTION_STATE.ACTIVE					;}
	public void endInteract()         { Interaction_State= INTERACTION_STATE.DEACTIVATING			;}
	public void forceCancelInteract() { Interaction_State= INTERACTION_STATE.FORCE_CANCEL_ACTIVE	;}

	public void stepInteractionState() {
		switch (Interaction_State) {
			case INTERACTION_STATE.UNTARGETING          : detarget(); Interaction_State   = INTERACTION_STATE.UNTARGETED		; break;
			case INTERACTION_STATE.TARGETING            : retarget(); Interaction_State   = INTERACTION_STATE.TARGETED			; break;
			case INTERACTION_STATE.ACTIVATING           : activate(); Interaction_State   = INTERACTION_STATE.ACTIVE			; break;
			case INTERACTION_STATE.DEACTIVATING         : deactivate(); Interaction_State = INTERACTION_STATE.TARGETED			; break;
			case INTERACTION_STATE.FORCE_CANCEL_ACTIVE  : abruptDeactivate(); Interaction_State = INTERACTION_STATE.UNTARGETED	; break;

			case INTERACTION_STATE.UNTARGETED			: 
			case INTERACTION_STATE.ACTIVE				:
			default: break;
		}
	}
	
	public abstract void detarget();
	public abstract void retarget();
	public abstract void activate();
	public abstract void deactivate();
	public void abruptDeactivate() { deactivate(); detarget(); }


}

//public abstract class a_Interactable : i_interactable{ 
//	public KeyCode InteractionKey   { set; get;} = KeyCode.E;
//	public string InteractionPrompt { get; } = "Press E to Interact";

//	public void interact(){ 
//		Debug.Log( this.ToString() +" was interacted with");
//			}
	
	

//	}
