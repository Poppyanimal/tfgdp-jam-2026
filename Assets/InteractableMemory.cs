using UnityEngine;

public class InteractableMemory: EmptyInteractionInteractable
{

	InteractableController Control;
	string[][] SpeakerDialogue;

	override public void Start(){
		Control= FindFirstObjectByType<InteractableController>();
	}


	public override void activate() {
		Control.overallMemoryProgress+=1;

		Debug.Log(Control.overallMemoryProgress);
		parseCurrentMemory(Control.overallMemoryProgress);
		playCurrentMemory();

	}

	//To Generalize this for special memory classes
	void parseCurrentMemory(int curr) {
		if ( ! validateCurr(curr) ) return;

		string[] currMemory = Control.memory_texts[curr];
	
		int itt=0;
		SpeakerDialogue= new string[currMemory.Length][];
		foreach ( string quip in currMemory) {
			if (quip.Contains("&") )
				SpeakerDialogue[itt]= quip.Split("&");
			else
				SpeakerDialogue[itt]= new string[2] { "??? ? ???", quip };
			itt+=1;
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
