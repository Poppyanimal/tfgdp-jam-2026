using System;
using TMPro;
using UnityEngine;

public class InteractableController : MonoBehaviour
{
    public bool logDebugBehavior= true;

    [SerializeField]
    TextMeshProUGUI promptContainer;

    [SerializeField]
    PlayerController playerController;
    GameObject rotationBody;
    const float look_fov  = 15.0f ,
                look_dist =  1.5f ;
          float lookAngle =  0.0f ;
    RaycastHit[] lookAt;

    a_Interactable targetedInteractable;
    a_Interactable prevTargetedInteractable;
   

	private void Start() {
		if (playerController !=null) rotationBody=playerController.rotationBody;
        emptyAndHidePrompt();
	}

	// Update is called once per frame
	void Update()
    {
        updateLookAt();
        storePrevInteractable();
        
        if (findNewInteractable()) {
            if(prevTargetedInteractable!=null) cleanupPrevTarget();
            if(targetedInteractable    !=null) updateTargetInteractable();   
        }

        if (targetedInteractable!=null) handleInteraction();

        if (logDebugBehavior) {
            if ( !(prevTargetedInteractable==null && targetedInteractable==null) ) {
                if (prevTargetedInteractable!=null) { 
                    if(prevTargetedInteractable.Equals(targetedInteractable) ) ;
                        //Debug.LogFormat("The current target, {0} is: {1}.", targetedInteractable?.ToString(), targetedInteractable?.Interaction_State);
                    else
                        Debug.LogFormat("The prev target, {0} is: {1}. \n The current target, {2} is: {3}.", 
                            prevTargetedInteractable?.ToString(), prevTargetedInteractable?.Interaction_State,
                                targetedInteractable?.ToString(),     targetedInteractable?.Interaction_State);
                }
                else
                    Debug.LogFormat("The current target, {0} is: {1}.", targetedInteractable?.ToString(), targetedInteractable?.Interaction_State);
            }
        } 
    
    }

	#region Start of Update Housekeeping
     void updateLookAt() {
        lookAngle=rotationBody.transform.eulerAngles.y;
        lookAt= SharedLib.castWFC(rotationBody.transform.position, lookAngle,look_fov, look_dist, "Interact", true);
    }

     void storePrevInteractable() { prevTargetedInteractable=targetedInteractable; }
	#endregion

	#region Find the Targeted Interactable 
   
    //TODO: find Interactables the Player is 'inside of'
    //TODO: make sure players can't interact through walls that should block their view.
    bool findNewInteractable() {
        Collider collider=null;

        if      (lookAt[1].collider!=null) collider= lookAt[1].collider;
        else if (lookAt[0].collider!=null) collider= lookAt[0].collider;
        else if (lookAt[2].collider!=null) collider= lookAt[2].collider;

        a_Interactable interactComponent;
        try {
            interactComponent= collider.GetComponent<a_Interactable>();
            if (interactComponent==null) { Debug.LogWarning("Missing i_Interactable component: " + collider.ToString() ); }
        }
        catch (NullReferenceException) {
            interactComponent=null;
        }
        targetedInteractable=interactComponent;

        if (prevTargetedInteractable == null) { return targetedInteractable!=null;}
        return ! prevTargetedInteractable.Equals(targetedInteractable);

    }

	#endregion

	#region Cleanup and Update Targets

	#region Cleanup
	void cleanupPrevTarget() {
        switch (prevTargetedInteractable.Interaction_State){
            case a_Interactable.INTERACTION_STATE.ACTIVATING: case a_Interactable.INTERACTION_STATE.ACTIVE                       : prevTargetedInteractable.forceCancelInteract(); break;
            case a_Interactable.INTERACTION_STATE.TARGETING : case a_Interactable.INTERACTION_STATE.TARGETED                     : prevTargetedInteractable.endTarget()          ; break; 
            default: Debug.LogFormat("Unexpected Case in cleanupPrevTarget() for: {0}. ForceCanceling as default", ToString() );   prevTargetedInteractable.forceCancelInteract(); break;
        }
        emptyAndHidePrompt();
    }

    void emptyAndHidePrompt() {
        promptContainer.text = string.Empty;
    }
    
	#endregion 

	#region Update Target
	void updateTargetInteractable() {
        targetedInteractable.target();
        fillAndShowPrompt();

    }


    void fillAndShowPrompt() { 
        promptContainer.text = targetedInteractable.Interaction_Prompt;
    }
	#endregion 

    #endregion


	#region Interact with Target

    void handleInteraction() {
        switch (targetedInteractable.Interaction_State) {
            case a_Interactable.INTERACTION_STATE.ACTIVATING :
            case a_Interactable.INTERACTION_STATE.ACTIVE     :    if (checkInteractionInputEnds()) targetedInteractable.endInteract() ; break;
            default:                                              if (checkInteractionInput()    ) targetedInteractable.interact()    ; break;
        }

        if (logDebugBehavior) {
            Debug.LogFormat("The currently targeted interactable : {0}, was interacted with.", targetedInteractable.ToString() );
        }
    }

	bool checkInteractionInput()     { return  Input.GetKeyDown(targetedInteractable.Interaction_Key); } //Start Interaction
    bool checkInteractionInputEnds() { return !Input.GetKey    (targetedInteractable.Interaction_Key); } //Continue or Stop Interaction
    #endregion

}