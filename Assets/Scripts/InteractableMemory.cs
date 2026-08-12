using System;
using UnityEngine;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class InteractableMemory: EmptyInteractionInteractable
{

	InteractableController Control;
	string[][] SpeakerDialogue;

	override public void Start(){
		Control= FindFirstObjectByType<InteractableController>();
	}

	public override void activate() {
		Control.overallMemoryProgress+=1;
		
		SpeakerDialogue= parseCurrentMemory(Control.overallMemoryProgress);
		try { playCurrentMemory(); }
		catch (NullReferenceException) { }

	}

	//To Generalize this for special memory classes
	string[][] parseCurrentMemory(int curr) { 

		if ( ! validateCurr(curr) ) { return curr<0? Control.mr_418_exception_speech:Control.ms_429_exception_speech; }

		string[] currMemory = Control.memory_texts[curr];
	
		int itt=0;
		string[][] toReturn = new string[currMemory.Length][];
		foreach ( string quip in currMemory) {
			if (quip.Contains("&") )		
				toReturn[itt]= quip.Split("&");
			else {		
				toReturn[itt]= new string[2] { Control.mx_404_exception_name, quip };
				toReturn[0]= new string[3] { toReturn[0][0], toReturn[0][1], "404" };
			}
			itt+=1;
		}
		toReturn= Control.replaceSpeakerNames(toReturn);


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
		//if (Interaction_State!=INTERACTION_STATE.PREVENT_EXHAUSTION) {
		////	Interaction_State= INTERACTION_STATE.EXHAUSTED;
		//}
		//else {
			Interaction_State= INTERACTION_STATE.TARGETING;
		//}
	}


}
