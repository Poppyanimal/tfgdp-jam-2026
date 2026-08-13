using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class InteractableTP :  EmptyInteractionInteractable
{

	public GameObject Sister;
	public InteractableTP SisterTP;
	public Vector3 position;
	public Camera camAfterTP;

	[SerializeField] GameObject FadePanel;
	Color currentFade;
	float fadeTime=0.75f;
	float fadeTimeElapsed;

	Coroutine teleporting;
	Coroutine fading;
	public bool canTeleport=true; 
	float teleportCooldown=1;
	WaitForSeconds cooldownWait;

	public override void Start() {
		base.Start();
		SisterTP=Sister.GetComponent<InteractableTP>();
		position= transform.position+Vector3.up*0.5f;
		cooldownWait = new WaitForSeconds(teleportCooldown+3*fadeTime);
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
		if (fading!=null) StopCoroutine(fading);
		fading= StartCoroutine( FadeOutBeforeTP() );


		yield return new WaitUntil(() => fadeTimeElapsed >= fadeTime);
		Debug.Log("Faded");
		yield return new WaitForEndOfFrame();
		Debug.Log(SisterTP.position.ToString());
		Control.Player.GetComponent<PlayerController>().TeleportPlayer(SisterTP.position);
		yield return new WaitForSeconds(fadeTime);
		fading=StartCoroutine( FadeInAfterTP() );
	}
	IEnumerator FadeOutBeforeTP() {
		while (fadeTimeElapsed<fadeTime) {		
			Debug.LogFormat("Fading:{0}/{1}",fadeTimeElapsed,fadeTime);
			fadeTimeElapsed+= Time.deltaTime;
			Image tempImg = FadePanel.GetComponent<Image>();
			Color tempColor = tempImg.color;
			tempColor.a= Mathf.Lerp(0,1,fadeTimeElapsed/fadeTime);

			tempImg.color=tempColor;
			yield return new WaitForSeconds(0.001f);
		}
	}
	IEnumerator FadeInAfterTP() {
		while (fadeTimeElapsed>0) {		
			Debug.LogFormat("Defading:{0}/{1}",fadeTimeElapsed,fadeTime);
			fadeTimeElapsed-= Time.deltaTime;
			Image tempImg = FadePanel.GetComponent<Image>();
			Color tempColor = tempImg.color;
			tempColor.a= Mathf.Lerp(0,1,fadeTimeElapsed/fadeTime);

			tempImg.color=tempColor;
			yield return new WaitForSeconds(0.001f);
		}
		Image tempImg2 = FadePanel.GetComponent<Image>();
		Color tempColor2 = tempImg2.color;
		tempColor2.a= 0;
		tempImg2.color=tempColor2;
	}

}
