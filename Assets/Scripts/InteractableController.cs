using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractableController : MonoBehaviour
{
    public string[][] mr_418_exception_speech= new string[3][]{ new string[3]{ "Mr. 418", "I don't know how ya managed it kiddo, but cha tried to remember something that happened before yous were born.", "418"}, 
		                                                        new string[2]{ "Mr. 418", "Sorry, only way I know to fix the timeline is to start cha over. Whelp, should be fixed now; give it another shot."   },
														        new string[2]{ "Mr. 418", "Be warned though, I can't gaurantee there are enough whips about to unlock the Boss door. You might be better off with a full reset."} };
	public string[][] ms_429_exception_speech= new string[2][]{ new string[3]{ "Ms. 429", "Oh bless your heart darlin'. Your love the game so much, you found an extra wisp of memory, but we ain't got no more story left for ya.", "429"}, 
		                                                        new string[2]{ "Ms. 429", "Why don't you head on over to the final boss arena and see how this all ends?" } };
	public string mx_404_exception_name= "Mx 404, Myst. Stranger";

    public bool logDebugBehavior= false;

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

    a_Interactable targetInteractable;
    a_Interactable prevInteractable;

    bool isDialogueError; string interaction_error_code;
    public int overallMemoryProgress { set; get; } = -1;
    readonly public string[][] memory_texts = { 
        new string[]{ 
            "2&What's your earliest childhood memory?", 
            "1&Don't have any.",
            "2&What do you mean?", 
            "1&My first memory is from two weeks after my fifthteenth birthday.", 
            "2&Nothing before that?", 
            "1&Nope. Why?",
            "2&Bestie,.. that's not normal.", 
            "1&What do you mean?"
            }, //Memoryless
        new string[]{ 
            "1&Well, here I am.", 
            "1&Home sweet childhood home."
            }, //Childhood Home
        new string[]{
            "R&Oh what a handsome young man. You've gotten so big. Here, I brought presents.", 
            "1&Oh, uh... Thanks Aunt Rosemary.",
            "0& ",
            "M&You could of at least pretended to be excited for Aunt Rosemary's gift. She think really hard about what clothes you'd like.",
            "M&Even if she got it wrong this year there's no reason to sound so ungrateful.", 
            "1&She gets it wrong every year." 
            }, //Aunt Rosemary's Gift
        new string[]{ 
            }, //Christmas reprisal
        new string[]{ 
            "D&Stop your baby-crying boy. It's just a scratch. Real men don't cry about small shit like this.", 
            "1&*sniff*"
            }, //Stop crying
        new string[]{ 
            "1&I want the pink one.", 
            "M&Now Son, you know Jessica wants the pink one. Why not let her have it.", 
            "1&She always gets to have the pink one.", 
            "M&Of course she does; she's a girl, Son."
            }, //The Pink One
        new string[]{ 
            "D&No Son of mine is going to play with Dolls.", 
            "1&Dad stop it, please stop.", 
            "D&Quit crying boy, before I give you something real to cry about."
            }, //Dolls
        new string[]{ 
            "1&Hey guys.",
            "3&Woah dude! You scared the shit out of me. How did you learn to move so silently.", 
            "1&My mom likes her quiet time and our floorboards creak."
            }, //Sneaky
        new string[]{ 
            "J&Bro, wake up! Naptime's over.",
            "1&Wish I could just nap forever.",
            "J&What?",
            "1&You know, lay down and never wake up.", 
            "J&Bro, you alright?", 
            "1&Yeah? ... Forget about it, alright."
            }, //Naptime
        new string[]{ 
            "1&Hey Jess, I was thinking; if you wanted to, you could call me Lily.", 
            "J&What? But isn't that a girl's name: wouldn't it be weird.", 
            "1&... I said if you wanted to.",
            "1b&I guess Lily was a stupid nickname anyway."
            }, //Lily
        new string[]{ 
            "D&<b>That's it!</b> This is ridiculous. I'm taking you to the barber and you're getting a haircut.", 
            "1&But Dad, I like my hair long.", 
            "D&And if you give me anymore lip, I'll have George shave you." 
            }, //Haircuts
        new string[]{ 
            "D&Why don't you talk to me anymore?", 
            "1&..."
            }, //Talk to me
        new string[]{ 
            "D&I lost my son, and you're saying I'm not even allowed to grieve.", 
            "1&You didn't lose anything. I'm still here, same as I ever was.", 
            "1&I'm just not pretending to be the man you think I was supposed to be."
            }, //Grieving
        new string[]{ 
            "D&We talked with Aunt Rosemary about your situation.",
            "M&She recommended we enroll you in a summer camp of sorts.", 
            "1&I thought I told old you not to tell her."
            }, // 'Summer Camp'
        new string[]{ 
            "D&Young man, you are the child and we're the adults. When we listen to you, it is as a courtesy.", 
            "D&It's not something you can demand, especially against your best interest.", 
            "D&Now got pack your fucking bags."
            }, //Courtesy
        new string[]{ 
            "Doll&Oh deary you, what a wretched life.", 
            "1&Yeah it kinda sucked.", 
            "Doll&Would you like to forget about it?", 
            "Doll&I can help with that."
            }, //Wretch Lift
        new string[]{ 
            "1&Why?", 
            "Doll&Because I'm hungry, and your look frayed enough to agree to it.", 
            "1&...Yeah okay then. Do it.", 
            "Doll&Don't worry sweetie, I'll be thorough."
            }, //Forget about all that 
    };

    int currentMemoryProgress=-1;
    string[][] currentMemory;

    public enum SPECIAL_MEMORY { UMBRELLA, STAIRS, LILIES }
    readonly public string[][] special_memory_texts = {
        new string[]{ 
            "Don't forget your umbrella. It's supposed to rain tomorrow", 
            "My umbrella?", 
            "... umbrella...", 
            "MY UMBRELLA!"
        },
        new string[]{ 
            "You never get used to the sensation of falling.", 
            "It feels so freeing, like you've escaped gravity's cruel prison.", 
            "But then, the Ground is a harsh Warden, and she always catches her runaways."
        },
        new string[]{ "Lily lily lily, like the flower."}
        };
    public bool[] special_memories_seen;


	private void Start() {
        getComponentFields();
        initializeFields();
        emptyAndHidePrompt();
	}

    void getComponentFields() {}

    void initializeFields() {
        special_memories_seen= new bool[special_memory_texts.Length];
    }

	// Update is called once per frame
	void Update()
    {   
        if (targetInteractable!=null) handleInteraction();
    }

	#region Interact with Target

    void handleInteraction() {
        bool interact       = Input.GetKeyDown(targetInteractable.Interaction_Key);
        bool interactEnd    = Input.GetKeyUp  (targetInteractable.Interaction_Key);
       
        if (0<=currentMemoryProgress) {
            if (interact) {
                TypewriterEffect typer;
                bool typerExists= DialogueTextContainer.TryGetComponent<TypewriterEffect>(out typer);
                if (typerExists) { 
                    if (typer.isTyping())   {   typer.SkipTyping();     }
                    else                    {   advanceMemory();        }
                }
                else { advanceMemory();}

            }
        }
        else if (interact || interactEnd){ 
            switch (targetInteractable.Interaction_State) {
                case a_Interactable.INTERACTION_STATE.TARGETED   :    if (interact   ) targetInteractable.interact()   ; else fillAndShowPrompt (); break; 
                case a_Interactable.INTERACTION_STATE.ACTIVATING :
                case a_Interactable.INTERACTION_STATE.ACTIVE     :    if (interactEnd) targetInteractable.endInteract(); else emptyAndHidePrompt(); break;
                case a_Interactable.INTERACTION_STATE.EXHAUSTED  :    break;
                default:                                              Debug.LogFormat("UNEXPECTED INTERACTION STATE {0}", targetInteractable.Interaction_State); break;
            }
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

        if ( ! freshInteractable.isExhausted()) {
            targetInteractable=freshInteractable;
            targetInteractable.target();
            fillAndShowPrompt();
        }
        else {
            targetInteractable=null;
        }

    }

    void cleanupPrevTarget() {
        if (targetInteractable==null) return;
        prevInteractable = targetInteractable;
        targetInteractable=null;

        switch (prevInteractable.Interaction_State){
            case a_Interactable.INTERACTION_STATE.ACTIVATING: case a_Interactable.INTERACTION_STATE.ACTIVE                       : prevInteractable.forceCancelInteract(); break;
            case a_Interactable.INTERACTION_STATE.TARGETING : case a_Interactable.INTERACTION_STATE.TARGETED                     : prevInteractable.endTarget()          ; break; 
            case a_Interactable.INTERACTION_STATE.EXHAUSTED :                                                                                                              break;
            default: Debug.LogFormat("Unexpected Case in cleanupPrevTarget() for: {0}. ForceCanceling as default", ToString() );   prevInteractable.forceCancelInteract(); break;
        }
        emptyAndHidePrompt();

        debugCleanup(logDebugBehavior);

    }
    void debugCleanup(bool debugThis) { 
        string str0= prevInteractable  == null? "null":string.Format("{0} @{1}", prevInteractable  , prevInteractable.Interaction_State  ) ;
        string str1= targetInteractable== null? "null":string.Format("{0} @{1}", targetInteractable, targetInteractable.Interaction_State) ;
        //if(debugThis) Debug.LogFormat( "\nPrev is {0}\n Target is {0}", str0, str1);   
    }
    
    void fillAndShowPrompt() { 
        promptContainer.gameObject.SetActive(true);
        promptTextContainer1.text = targetInteractable.Interaction_Prompt;
        promptTextContainer2.text = targetInteractable.Interaction_Prompt;
    }

    void emptyAndHidePrompt() {
        promptContainer.gameObject.SetActive(false);
    }

    public void playMemory(string[][]memoryToPlay) {
        detectDialogueError(memoryToPlay[0]);
        psuedoPause(true);
        currentMemory=memoryToPlay;
        showAndBeginDialogue();
        //int itt=0;
        //foreach (string[] strA in memoryToPlay) {
        //    Debug.LogFormat("{0} {1}: {2}  .", itt, strA[0], strA[1]);
        //    itt+=1;
        //}
    }

    void detectDialogueError(string[] memoryToCheck) {
        if (memoryToCheck.Length > 2) { 
            isDialogueError=true;
            interaction_error_code = memoryToCheck[2];
            Debug.LogWarning( string.Format("Obj:{0} produced error code {1} during interaction.", targetInteractable.ToString(), interaction_error_code) );
        }
    }

    void psuedoPause(bool startPsuedoPause) {
        Time.timeScale= startPsuedoPause? 0.0f:1.0f;
        PlayerController pc;
        Player.TryGetComponent<PlayerController>(out pc);
        pc.enabled=!startPsuedoPause;
    }

    void showAndBeginDialogue() {
        currentMemoryProgress=-1;
        dialogueContainer.gameObject.SetActive(true);
        advanceMemory();
    }

    void advanceMemory() {
        currentMemoryProgress+=1;
        if (currentMemoryProgress == currentMemory.Length) {
            endMemory();
            return;
        }
        setTextAndTypewriter();
    }

    void setTextAndTypewriter() {
        SpeakerTextContainer .SetText(currentMemory[currentMemoryProgress][0]);
        DialogueTextContainer.GetComponent<TypewriterEffect>().TypeText(currentMemory[currentMemoryProgress][1]);
    }

    void endMemory() {
        if (isDialogueError){
            switch (interaction_error_code) {
                case "419": 
                    overallMemoryProgress= -1;
                    currentMemoryProgress=-1;
                    currentMemory= new string[0][];
                    dialogueContainer.gameObject.SetActive(false);   
                    psuedoPause(false);
                    targetInteractable.Interaction_State=a_Interactable.INTERACTION_STATE.PREVENT_EXHAUSTION;
                    break;
            }
        }
        currentMemoryProgress=-1;
        currentMemory= new string[0][];
        dialogueContainer.gameObject.SetActive(false);   
        psuedoPause(false);
    }

    public string[][] replaceSpeakerNames(string[][] rawSplitDialogue) {
		foreach (string[] dialoguePage in rawSplitDialogue) {
			switch (dialoguePage[0]){
				case "1": dialoguePage[0]="Protag"; break;
				case "2": dialoguePage[0]="Friend"; break;
				case "M": dialoguePage[0]="Mom"   ; break;
				case "D": dialoguePage[0]="Dad"   ; break;
				case "J": dialoguePage[0]="Jessica"; break;
				case "R": dialoguePage[0]="Aunt Rosemary"; break;
				case "3": dialoguePage[0]="Childhood Friend"; break;
				case "1b":dialoguePage[0]="Protag(later)"; break;
			}
		}

		return rawSplitDialogue;

	}

}