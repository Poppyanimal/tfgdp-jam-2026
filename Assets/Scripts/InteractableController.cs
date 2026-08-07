using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractableController : MonoBehaviour
{
    public bool logDebugBehavior= true;

    [SerializeField]
    Canvas promptContainer;
    TextMeshProUGUI promptTextContainer;

    [SerializeField]
    public GameObject PlayerRotation;
    CapsuleCollider Hitbox;

    const float look_fov  = 15.0f ,
                look_dist =  1.5f ;
          float lookAtAngle =  0.0f ;
    RaycastHit[] scanSweepInteract;

    bool lockTarget;
    a_Interactable targetInteractable;
    a_Interactable prevInteractable;
   
	private void Start() {
        getComponentFields();
        emptyAndHidePrompt();
	}

    void getComponentFields() {
        Hitbox=GetComponent<CapsuleCollider>();
    }

	// Update is called once per frame
	void Update()
    {   
        if (targetInteractable!=null) handleInteraction();
    }

	#region Interact with Target

    void handleInteraction() {
        bool interact       =   Input.GetKeyDown(targetInteractable.Interaction_Key);
        bool interactEnd    = ! Input.GetKey    (targetInteractable.Interaction_Key);
        bool debugIEnd      =   Input.GetKeyUp  (targetInteractable.Interaction_Key);

        switch (targetInteractable.Interaction_State) {
            case a_Interactable.INTERACTION_STATE.ACTIVATING :
            case a_Interactable.INTERACTION_STATE.ACTIVE     :    if (interactEnd) targetInteractable.endInteract() ; break;
            default:                                              if (interact   ) targetInteractable.interact()    ; break;
        }

        if (logDebugBehavior) {
            if (interact ) Debug.LogFormat("{0}, was interacted with."         , targetInteractable.ToString() );
            if (debugIEnd) Debug.LogFormat("{0}, stoped being interacted with.", targetInteractable.ToString() );
        }
    }
    #endregion


    void OnTriggerEnter(Collider other)	{
        a_Interactable freshInteractable;
        if (other.TryGetComponent<a_Interactable>(out freshInteractable)) {
                updateTargetInteractable(freshInteractable);
        }
	}

	void OnTriggerExit(Collider other) {
        cleanupPrevTarget();
	}

    void updateTargetInteractable(a_Interactable freshInteractable) {
        cleanupPrevTarget();

        targetInteractable=freshInteractable;
        targetInteractable.target();
        fillAndShowPrompt();

    }

    void cleanupPrevTarget() {
        if (targetInteractable==null) return;
        prevInteractable = targetInteractable;
        targetInteractable=null;

        switch (prevInteractable.Interaction_State){
            case a_Interactable.INTERACTION_STATE.ACTIVATING: case a_Interactable.INTERACTION_STATE.ACTIVE                       : prevInteractable.forceCancelInteract(); break;
            case a_Interactable.INTERACTION_STATE.TARGETING : case a_Interactable.INTERACTION_STATE.TARGETED                     : prevInteractable.endTarget()          ; break; 
            default: Debug.LogFormat("Unexpected Case in cleanupPrevTarget() for: {0}. ForceCanceling as default", ToString() );   prevInteractable.forceCancelInteract(); break;
        }
        emptyAndHidePrompt();

        debugCleanup(logDebugBehavior);

    }
    void debugCleanup(bool debugThis) { 
        string str0= prevInteractable  == null? "null":string.Format("{0} @{1}", prevInteractable  , prevInteractable.Interaction_State  ) ;
        string str1= targetInteractable== null? "null":string.Format("{0} @{1}", targetInteractable, targetInteractable.Interaction_State) ;
        Debug.LogFormat( "\nPrev is {0}\n Target is {0}", str0, str1);   
    }
    
    void fillAndShowPrompt() { 
        promptContainer.enabled=true;
        //        promptTextContainer.text = targetedInteractable.Interaction_Prompt;
    }

    void emptyAndHidePrompt() {
        promptContainer.enabled=false;
    }
}