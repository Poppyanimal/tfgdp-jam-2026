using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractableController : MonoBehaviour
{
    public bool logDebugBehavior= true;

    public Canvas promptContainer;
    public TextMeshProUGUI promptTextContainer1;
    public TextMeshProUGUI promptTextContainer2;

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

    readonly public string[][] memories_texts = { 
        new string[]{ "I hardly have any memories of my chidhood", "What do you mean?", "I can barely remember anything before my fifthteenth birthday.", "Nothing?", "Sometimes, I get flashes,","Bestie, that's not normal.", "What do you mean?"},
        new string[]{ "Well, here we are.", "Home sweet childhood home."},
        new string[]{ "You could at least pretend to be excited about Aunt Rosemary's christmas gift. She thought really hard about what clothes you'll like, and even if she got it wrong this year there's no reason to sound so ungrateful.", "She gets it wrong every year." },
        new string[]{ "Come now boy, stop your baby-crying. It's just a scratch. Boys don't cry about little things like this.", "*sniff*"},
        new string[]{ "I want the pink one.", "Now #@&^!&, you know Jessica wants the pink one. Why not let her have it.", "She always gets to have the pink one.", "Of course she does; she's a girl,[diminutive]."},
        new string[]{ "No son of mine is going to play with Dolls.", "Dad stop it, please stop.", "Quit crying boy, before I give you something to actually cry about."},
        new string[]{ "Don't forget your umbrella. It's supposed to rain tomorrow", "My umbrella?", "... umbrella...", "MY UMBRELLA!"},
    
        new string[]{ "Who would want to be a girl anyway?"},
        new string[]{ "Hey guys.","Woah dude! You scared the shit out of me. How did you learn to move so silently.", "My mom like her quiet time and the floorboards creak."},
        new string[]{ "Hey #@&^!&, what's up.","You ever feel like it'd be nice to just, nap forever.", "What?", "You know, fall asleep and not wake up?", "Bro, you doing alright?", "*sigh* Just forget about it, alright."},
        new string[]{ ""},
        
        new string[]{ "You can call me Lily, if you want to.", "What? But that's a girl's name? It'd be weird.", "...Only if you want to."},
        new string[]{ "Why don't you talk to me anymore?", "..."},
        new string[]{ "I'm not gonna kiss you. What are you gay?"},
        new string[]{ "That's IT! This is ridiculous. I'm taking you to the barber, and you're getting a haircut.", "But, I like my hair long.", "And if you give me anymore lip, I'll have George shave you." },
        new string[]{ ""},

        new string[]{ "I lost my son, and you're saying I'm not even allowed to grieve.", "You didn't lose anything. I'm still here, same as I've always been. I'm just not who you thought I was."},
        new string[]{ "We talked with Aunt Rosemary about your situation, and she recommended we enroll you in a summer camp of sorts.", "I thought I told old you not to tell her."},
        new string[]{ "Young man, you are the child and we're the adults. Us listening to you is a courtesy we offer, not something you can demand. Pack your fucking bags."},
        new string[]{ ""},        
        new string[]{ ""}
    };

    readonly public string[][] special_memories = {
        new string[]{ "You never get used to the sensation of falling.", "It feels so freeing, like you've escaped gravity's cruel prison.", "But the Ground is a harsh Warden, who'll always catch you with a closed fist."}
    };





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
            case a_Interactable.INTERACTION_STATE.TARGETED   :    if (interact   ) targetInteractable.interact()   ; else fillAndShowPrompt (); break; 
            case a_Interactable.INTERACTION_STATE.ACTIVATING :
            case a_Interactable.INTERACTION_STATE.ACTIVE     :    if (interactEnd) targetInteractable.endInteract(); else emptyAndHidePrompt(); break;
            default:                                              Debug.Log("UNEXPECTED INTERACTION STATE")                                   ; break;
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
        promptTextContainer1.text = targetInteractable.Interaction_Prompt;
        promptTextContainer2.text = targetInteractable.Interaction_Prompt;
    }

    void emptyAndHidePrompt() {
        promptContainer.enabled=false;
    }
}