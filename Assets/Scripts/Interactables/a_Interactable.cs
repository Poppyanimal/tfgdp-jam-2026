using UnityEngine;

public abstract class a_Interactable : MonoBehaviour { 
	
	protected InteractableController Control;

	public enum   INTERACTION_STATE { UNTARGETED, TARGETING, UNTARGETING, TARGETED, ACTIVATING, DEACTIVATING, ACTIVE, FORCE_CANCEL_ACTIVE, EXHAUSTED, PREVENT_EXHAUSTION}
	public INTERACTION_STATE Interaction_State  { set; get; } = INTERACTION_STATE.UNTARGETED;
	//TODO: change this to not be a key but the input system's button, get key from first binding of that button!!!
	public KeyCode           Interaction_Key    { set; get;}  = KeyCode.E;
	[HideInInspector]
	public string            Interaction_Prompt      = "Press E to Interact";

	
	virtual public void Start () { 
		Control= FindFirstObjectByType<InteractableController>(); 
		detarget();
	}
	virtual public void Update() { stepInteractionState();	}

	public void target()              { Interaction_State= isExhausted()? INTERACTION_STATE.EXHAUSTED:INTERACTION_STATE.TARGETING			;}
	public void endTarget()           { Interaction_State= isExhausted()? INTERACTION_STATE.EXHAUSTED:INTERACTION_STATE.UNTARGETING			;}
	public void interact()            { Interaction_State= isExhausted()? INTERACTION_STATE.EXHAUSTED:INTERACTION_STATE.ACTIVATING			;}
	public void endInteract()         { Interaction_State= isExhausted()? INTERACTION_STATE.EXHAUSTED:INTERACTION_STATE.DEACTIVATING		;}
	public void forceCancelInteract() { Interaction_State= isExhausted()? INTERACTION_STATE.EXHAUSTED:INTERACTION_STATE.FORCE_CANCEL_ACTIVE	;}

	public void stepInteractionState() {
		switch (Interaction_State) {
			case INTERACTION_STATE.UNTARGETING          : detarget();         Interaction_State = isExhausted()? INTERACTION_STATE.EXHAUSTED:INTERACTION_STATE.UNTARGETED	; break;
			case INTERACTION_STATE.TARGETING            : retarget();         Interaction_State = isExhausted()? INTERACTION_STATE.EXHAUSTED:INTERACTION_STATE.TARGETED		; break;
			case INTERACTION_STATE.ACTIVATING           : activate();         Interaction_State = isExhausted()? INTERACTION_STATE.EXHAUSTED:INTERACTION_STATE.ACTIVE		; break;
			case INTERACTION_STATE.DEACTIVATING         : deactivate();       Interaction_State = isExhausted()? INTERACTION_STATE.EXHAUSTED:INTERACTION_STATE.TARGETED		; break;
			case INTERACTION_STATE.FORCE_CANCEL_ACTIVE  : abruptDeactivate(); Interaction_State = isExhausted()? INTERACTION_STATE.EXHAUSTED:INTERACTION_STATE.UNTARGETED	; break;
			default: break;
		}
	}
	
	public bool isExhausted() { return Interaction_State==INTERACTION_STATE.EXHAUSTED;}

	public abstract void detarget  ();
	public abstract void retarget  ();
	public abstract void activate  ();
	public abstract void deactivate();
	public void abruptDeactivate() { deactivate(); detarget(); }


	protected bool deactivateParticleCoreMesh = true;
	public void deactivateAllParticles()
	{
		if(deactivateParticleCoreMesh)
		{
			MeshRenderer render = GetComponentInChildren<MeshRenderer>();
			if(render != null)
				render.enabled = false;
		}

		ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
		foreach(ParticleSystem p in particles)
		{
			p.Stop();
		}
	}


}
