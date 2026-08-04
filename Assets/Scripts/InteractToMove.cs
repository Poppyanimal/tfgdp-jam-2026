using UnityEngine;

public class InteractToMove :  a_Interactable
{
	[SerializeField] bool logDebugMessages= false;

	ParticleSystem particleDoohicky;


	Rigidbody body;

	public void Start() {
		body = GetComponent<Rigidbody>();
		particleDoohicky = GetComponent<ParticleSystem>();
		body.constraints = RigidbodyConstraints.FreezeAll;

		stepInteractionState();
	}
	
	public override void detarget()   {
		var main =particleDoohicky.main;
			main.loop=false;

		}
	public override void retarget()   { 
		var main =particleDoohicky.main;
			main.loop=true;
		particleDoohicky.Play();

		}
	public override void activate()   { 
		var color= particleDoohicky.colorOverLifetime.color;
		color= new Gradient();
		}
	public override void deactivate() { /*Close a Dialogue*/ }


	public void Update() { stepInteractionState();	}


}
