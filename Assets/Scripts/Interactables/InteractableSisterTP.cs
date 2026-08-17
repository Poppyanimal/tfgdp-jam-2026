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
	public bool teleportingToOutside = true;

	const float fadeTime=0.4f;
	float fadeTimeElapsed;

	Coroutine teleporting;
	public bool canTeleport=true; 
	float teleportCooldown=1;
	WaitForSeconds cooldownWait;
	const float teleportOffset = .1f;
	public sfxPlayer teleportSFX;

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
		Vector3 newLocation = SisterTP.transform.position + Vector3.up * teleportOffset;

		GetComponent<Collider>().enabled = false;
		GlobalEvents.get().doFade.Invoke();
		teleportSFX.play();
		GlobalEvents.get().teleportLock.Invoke();
		yield return new WaitForSeconds(fadeTime);


		PlayerController player = FindFirstObjectByType<PlayerController>();
		player.TeleportPlayer(newLocation);
		player.removeCollider(GetComponent<Collider>());
		
		if(camAfterTP != null)
		{
			((CinemachineCamera)FindFirstObjectByType<CinemachineBrain>().ActiveVirtualCamera).gameObject.SetActive(false);
			camAfterTP.gameObject.SetActive(true);
			player.camSwitch(camAfterTP.transform.parent.GetComponentInChildren<CameraSwitchTrigger>());
		}

		if(teleportingToOutside)
			GlobalEvents.get().goingOutdoors.Invoke();
		else
			GlobalEvents.get().goingIndoors.Invoke();


		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForFixedUpdate();
		player.TeleportPlayer(newLocation);
		player.resetCameraInput();

		yield return new WaitForSeconds(fadeTime);
		GlobalEvents.get().teleportUnlock.Invoke();
		GlobalEvents.get().endFade.Invoke();
		GetComponent<Collider>().enabled = true;
	}

}
