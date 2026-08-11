using UnityEngine;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class InteractableMemory: EmptyInteractionInteractable
{

	InteractableController Control;
	string[][] SpeakerDialogue;
	string[][] mr_418_exception_speech= new string[3][]{ new string[3]{ "Mr. 418", "I don't know how ya managed it kiddo, but cha tried to remember something that happened before yous were born.", "418"}, 
		                                                 new string[2]{ "Mr. 418", "Sorry, only way I know to fix the timeline is to start cha over. Whelp, should be fixed now; give it another shot."   },
														 new string[2]{ "Mr. 418", "Be warned though, I can't gaurantee there are enough whips about to unlock the Boss door. You might be better off with a full reset."} };
	string[][] ms_429_exception_speech= new string[2][]{ new string[3]{ "Ms. 429", "Oh bless your heart darlin'. Your love the game so much, you found an extra wisp of memory, but we ain't got no more story left for ya.", "429"}, 
		                                                 new string[2]{ "Ms. 429", "Why don't you head on over to the final boss arena and see how this all ends?" } };
	string mx_404_exception_name= "Mx 404, Myst. Stranger";

	override public void Start(){
		Control= FindFirstObjectByType<InteractableController>();
	}

	public override void activate() {
		Control.overallMemoryProgress+=1;
		
		SpeakerDialogue= parseCurrentMemory(Control.overallMemoryProgress);
		playCurrentMemory();

	}

	//To Generalize this for special memory classes
	string[][] parseCurrentMemory(int curr) {
		if ( ! validateCurr(curr) ) { return curr<0? mr_418_exception_speech:ms_429_exception_speech; }

		string[] currMemory = Control.memory_texts[curr];
	
		int itt=0;
		string[][] toReturn = new string[currMemory.Length][];
		foreach ( string quip in currMemory) {
			if (quip.Contains("&") )		
				toReturn[itt]= quip.Split("&");
			else {		
				toReturn[itt]= new string[2] { mx_404_exception_name, quip };
				toReturn[0]= new string[3] { toReturn[0][0], toReturn[0][1], "404" };
			}
			itt+=1;
		}
		return toReturn;
	//Replace Spearker IDs with Names by switch	
	}

	bool validateCurr(int curr) { 
		bool currIsValid = 0 <=curr && curr< Control.memory_texts.Length;	
		//if (! currIsValid) Debug.LogFormat("{0} tried to access invalid memory index {1}.", this.ToString(), curr);
		
		return currIsValid;
	}


	void playCurrentMemory() {
		Control.playMemory(SpeakerDialogue);
	}

	public override void deactivate() {
		if (Interaction_State!=INTERACTION_STATE.PREVENT_EXHAUSTION) {
			Interaction_State= INTERACTION_STATE.EXHAUSTED;
		}
		else {
			Interaction_State= INTERACTION_STATE.TARGETING;
		}
	}


}
