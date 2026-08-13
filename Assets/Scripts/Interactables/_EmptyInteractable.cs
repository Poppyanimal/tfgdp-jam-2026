using UnityEngine;

public class EmptyInteractionInteractable :  a_Interactable
{
	[SerializeField] bool logDebugMessages= false;

	public override void detarget()   {		
		//Implement what happens when the interactable stops being targeted.
	}


	public override void retarget()   {	
		//Implement what happens when the interactable starts beling targeted.
	}


	public override void activate()   { 	
		//Implement what happens when the interactable, while targeted has its interact key pressed.	
	}


	public override void deactivate() {     
		//Implement what happens when the interactble, while active and targeted, has its interact key released.	
	}

}
