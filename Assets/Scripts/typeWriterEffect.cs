using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[RequireComponent(typeof(TMP_Text))]

public class TypeWriterEffect : MonoBehaviour
{
	TMP_Text textContainer;

	[Header("Test String")]
	[SerializeField] string testString;

	int currVisibleCharacterIndex;

	Coroutine _typewriterCoroutine;
	WaitForSecondsRealtime simpleDelay;
	WaitForSecondsRealtime punctDelay;
	
	[Header ("Typewriter Settings")]
	[SerializeField] float charPerSecond = 20   ;
	[SerializeField] float addPunctDelay =  0.5f;

	void Awake() {
		textContainer = GetComponent<TMP_Text>();
		simpleDelay   = new WaitForSecondsRealtime(1/charPerSecond);
		punctDelay    = new WaitForSecondsRealtime(addPunctDelay);
	}

	void Start() {
		//SetText(testString);
	}

	public void SetText(string text) {
		if (_typewriterCoroutine!=null)
			StopCoroutine(_typewriterCoroutine);

		textContainer.text= text;
		textContainer.maxVisibleCharacters=0;
		currVisibleCharacterIndex=0;

		Debug.LogFormat("\n\n{0} \n{1}", text, textContainer.text );
		_typewriterCoroutine= StartCoroutine(Typewriter());
	}

	IEnumerator Typewriter() {
		TMP_TextInfo textInfo = textContainer.textInfo;

		while (currVisibleCharacterIndex < textInfo.characterCount + 1) {

			char character = textInfo.characterInfo[currVisibleCharacterIndex].character;
			textContainer.maxVisibleCharacters+=1;
			Debug.Log(character);

			switch (character) {
				case '.': case ',':case '!':case ';':case ':':case '?':case ')' :case '-':  yield return punctDelay;	yield return simpleDelay; break;
				default:																								yield return simpleDelay; break;
			}
			
			currVisibleCharacterIndex+=1;

		}

	}








}

