using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(TMP_Text))]

public class TypewriterEffect : MonoBehaviour
{
	TMP_Text textContainer;

	int currVisibleCharacterIndex;

	Coroutine _typewriterCoroutine;
	WaitForSecondsRealtime simpleDelay;
	WaitForSecondsRealtime punctDelay;
	
	[Header ("Basic Settings")]
	public float charPerSecond = 20   ;
	
	[SerializeField] float addPunctDelay =  0.05f;

	[Header ("Skipping")]
	public bool isSkipping{ get; private set; }
	[SerializeField] float skipSpeedFactor=5f;
	WaitForSecondsRealtime skipDelay;
	[SerializeField] public bool quickSkipping{ get; set;}

	void Awake() {
		textContainer = GetComponent<TMP_Text>();
		simpleDelay   = new WaitForSecondsRealtime(1/charPerSecond);
		punctDelay    = new WaitForSecondsRealtime(addPunctDelay);
		skipDelay     = new WaitForSecondsRealtime(1/ (charPerSecond*Mathf.Max(skipSpeedFactor,1.0f)) );
	}

	void Start() {
		//SetText(testString);
	}

	public void TypeText(string text) {
		if (_typewriterCoroutine!=null)
			StopCoroutine(_typewriterCoroutine);

		textContainer.text= text;
		textContainer.maxVisibleCharacters=0;
		currVisibleCharacterIndex=0;

		_typewriterCoroutine= StartCoroutine(Typewriter());
	}

	public void SkipTyping() {
		if (isSkipping) { 
			quickSkipping=true; 
		}
		else if (!quickSkipping) {
			isSkipping=true;
			StartCoroutine(SkipResetter());
			return;
		}

		StopCoroutine(_typewriterCoroutine);
		textContainer.maxVisibleCharacters = textContainer.textInfo.characterCount;
		isSkipping=false;
		quickSkipping=false;
	}


	public bool isTyping() {
		TMP_TextInfo textInfo = textContainer.textInfo;
		return textContainer.maxVisibleCharacters < textInfo.characterCount;
	}



	IEnumerator Typewriter() {
		TMP_TextInfo textInfo = textContainer.textInfo;

		while (currVisibleCharacterIndex < textInfo.characterCount + 1) {

			char character = ' ';
			try { character = textInfo.characterInfo[currVisibleCharacterIndex].character; }
			catch (IndexOutOfRangeException) { }

			textContainer.maxVisibleCharacters+=1;
			if (isSkipping)																									yield return skipDelay  ;
			else { switch (character) {
					case '.': case ',':case '!':case ';':case ':':case '?':case ')' :case '-':  yield return punctDelay;	yield return simpleDelay; break;
					default:																								yield return simpleDelay; break;
				}
			}
			currVisibleCharacterIndex+=1;

		}

	}
	
	IEnumerator SkipResetter() {
		yield return new WaitUntil(() => textContainer.maxVisibleCharacters == textContainer.textInfo.characterCount - 1) ;
		isSkipping= false;
		quickSkipping= false;
	}

	public void setCharPerSec(int setTo) {
		charPerSecond= setTo;
		simpleDelay   = new WaitForSecondsRealtime(1/charPerSecond);
		skipDelay     = new WaitForSecondsRealtime(1/ (charPerSecond*Mathf.Max(skipSpeedFactor,1.0f)) );
	}





}

