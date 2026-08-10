using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractableController : MonoBehaviour
{
    public bool logDebugBehavior= true;

    [Header ("Interaction Prompt")]
    public Canvas promptContainer;
    public TextMeshProUGUI promptTextContainer1;
    public TextMeshProUGUI promptTextContainer2;

    [Header ("Dialogue")]
    public Canvas dialogueContainer;
    public TextMeshProUGUI SpeakerTextContainer;
    public TextMeshProUGUI DialogueTextContainer;

    [Header ("Player")]
    public GameObject Player;

    bool lockTarget;
    a_Interactable targetInteractable;
    a_Interactable prevInteractable;

    public int overallMemoryProgress { set; get; } = -1;
    readonly public string[][] memory_texts = { 
        new string[]{ "1&I hardly have any memories from chidhood", "2&What do you mean?", "3&I can barely remember anything before my fifthteenth birthday.", "4&Nothing?", "5&Sometimes, I get flashes,","6&Bestie, that's not normal.", "7&What do you mean?"},
        new string[]{ "Well, here we are.", "Home sweet childhood home."},
        new string[]{ "You could at least pretend to be excited about Aunt Rosemary's christmas gift. She thought really hard about what clothes you'll like, and even if she got it wrong this year there's no reason to sound so ungrateful.", "She gets it wrong every year." },
    
        new string[]{ "Come now boy, stop your baby-crying. It's just a scratch. Boys don't cry about little things like this.", "*sniff*"},
        new string[]{ "I want the pink one.", "Now #@&^!&, you know Jessica wants the pink one. Why not let her have it.", "She always gets to have the pink one.", "Of course she does; she's a girl, Son."},
        new string[]{ "No son of mine is going to play with Dolls.", "Dad stop it, please stop.", "Quit crying boy, before I give you something to actually cry about."},
        new string[]{ "Hey guys.","Woah dude! You scared the shit out of me. How did you learn to move so silently.", "My mom like her quiet time and the floorboards creak."},
        new string[]{ "Hey #@&^!&, what's up.","You ever feel like it'd be nice to just, nap forever.", "What?", "You know, fall asleep and not wake up?", "Bro, you doing alright?", "*sigh* Just forget about it, alright."},
        
        new string[]{ "You can call me Lily, if you want to.", "What? But that's a girl's name? It'd be weird.", "... I said only if you wanted to."},
        new string[]{ "I guess Lily was a stupid nickname anyway."},
        new string[]{ "That's IT! This is ridiculous. I'm taking you to the barber, and you're getting a haircut.", "But, I like my hair long.", "And if you give me anymore lip, I'll have George shave you." },
        new string[]{ "Why don't you talk to me anymore?", "..."},
        
        new string[]{ "I lost my son, and you're saying I'm not even allowed to grieve.", "You didn't lose anything. I'm still here, same as I've always been. I'm just not who you thought I was."},
        new string[]{ "We talked with Aunt Rosemary about your situation, and she recommended we enroll you in a summer camp of sorts.", "I thought I told old you not to tell her."},
        new string[]{ "Young man, you are the child and we're the adults. Us listening to you is a courtesy we offer, not something you can demand. Pack your fucking bags."},
        new string[]{ ""},        
        new string[]{ ""}
    };

    int currentMemoryProgress=-1;
    string[][] currentMemory;

    readonly public string[][] special_memories = {
        new string[]{ "Don't forget your umbrella. It's supposed to rain tomorrow", "My umbrella?", "... umbrella...", "MY UMBRELLA!"},
        new string[]{ "You never get used to the sensation of falling.", "It feels so freeing, like you've escaped gravity's cruel prison.", "But the Ground is a harsh Warden, and she always catches her runaways."}
    };


	private void Start() {
        getComponentFields();
        emptyAndHidePrompt();
	}

    void getComponentFields() {}

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

    public void playMemory(string[][]memoryToPlay) {
        psuedoPause();
        currentMemory=memoryToPlay;
        showAndPopulateDialogue();
        //int itt=0;
        //foreach (string[] strA in memoryToPlay) {
        //    Debug.LogFormat("{0} {1}: {2}  .", itt, strA[0], strA[1]);
        //    itt+=1;
        //}
    }

    void psuedoPause() {
        Time.timeScale=0.1f;
        PlayerController pc;
        Player.TryGetComponent<PlayerController>(out pc);
        pc.enabled=false;
    }

    void showAndPopulateDialogue() {
        currentMemoryProgress=0;

        dialogueContainer.enabled=true;
        SpeakerTextContainer.text= currentMemory[currentMemoryProgress][0];
        DialogueTextContainer.text= currentMemory[currentMemoryProgress][1];
    }

}