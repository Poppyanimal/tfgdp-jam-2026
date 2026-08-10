using UnityEngine;

public class InteractableMemory: EmptyInteractionInteractable
{

	InteractableController Control;
	string[][] SpeakerDialogue;

	override public void Start(){
		Control= FindFirstObjectByType<InteractableController>();
	}


	public override void activate() {
		Control.memory_progress+=1;

		Debug.Log(Control.memory_progress);
		parseCurrentMemory(Control.memory_progress);
		playCurrentMemory();

	}
	void parseCurrentMemory(int curr) {
		if ( ! validateCurr(curr) ) return;

		string[] currMemory = Control.memory_texts[curr];
	
		int itt=0;
		SpeakerDialogue= new string[currMemory.Length][];
		foreach ( string quip in currMemory) {
			SpeakerDialogue[itt]= quip.Split("&");
		}
	//Replace Spearker IDs with Names by switch	
	}

	bool validateCurr(int curr) { 
		bool currIsValid = 0 <=curr && curr< Control.memory_texts.Length;	
	
		if (! currIsValid) Debug.LogFormat("{0} tried to access invalid memory index {1}.", this.ToString(), curr);
		
		return currIsValid;
	}


	void playCurrentMemory() {
		Control.playMemory(SpeakerDialogue);
	}



}
