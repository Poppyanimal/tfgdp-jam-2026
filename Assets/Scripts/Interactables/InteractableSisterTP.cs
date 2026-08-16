using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class InteractableTP :  EmptyInteractionInteractable
{

	public GameObject Sister;
	protected InteractableTP SisterTP;
	protected Vector3 position;
	public CinemachineCamera camAfterTP;

	const float fadeTime=0.4f;
	float fadeTimeElapsed;

	Coroutine teleporting;
	public bool canTeleport=true; 
	float teleportCooldown=1;
	WaitForSeconds cooldownWait;

	public override void Start() {
		base.Start();
		SisterTP=Sister.GetComponent<InteractableTP>();
		position= transform.position+Vector3.up*0.5f;
		cooldownWait = new WaitForSeconds(teleportCooldown+3*fadeTime);
		Interaction_Prompt = "[E] Continue Forward";
	}

	public override void activate()   {	
		tempDisableSister();
		if (canTeleport) {
			if (teleporting != null) StopCoroutine(teleporting); 
			teleporting=StartCoroutine(TeleportPlayer());	
		}
		GlobalEvents.get().hidePrompt.Invoke();
	}

	void tempDisableSister() {
		SisterTP.canTeleport=false;
		StartCoroutine(SisterCooldownResetter());
	}

	IEnumerator SisterCooldownResetter() {
		yield return cooldownWait;
		SisterTP.canTeleport=true;
	}

	IEnumerator TeleportPlayer() {
		GlobalEvents.get().doFade.Invoke();
		yield return new WaitForSeconds(fadeTime);

		yield return new WaitForEndOfFrame();

		if(camAfterTP != null)
		{
			((CinemachineCamera)FindFirstObjectByType<CinemachineBrain>().ActiveVirtualCamera).gameObject.SetActive(false);
			camAfterTP.gameObject.SetActive(true);
		}

		SisterTP=Sister.GetComponent<InteractableTP>();
		//TODO: INTERACTABLES SHOULD NOT RELY ON IS KEY DOWN, BUT INSTEAD ON THE INITIAL ONE FRAME PULSE!!!!!!!!!!
		//DO NOT ALLOW MULTIPLE INTERACTABLE INTERACTIONS IN ONE FRAME!!!!
		//TODO
		//TODO
		//TODO
		FindFirstObjectByType<PlayerController>().TeleportPlayer(SisterTP.transform.position);


		yield return new WaitForSeconds(fadeTime);
		GlobalEvents.get().endFade.Invoke();
	}

}
