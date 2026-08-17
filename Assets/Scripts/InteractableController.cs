using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class InteractableController : MonoBehaviour
{

    public Canvas promptContainer;
    public TextMeshProUGUI promptTextContainer1, promptTextContainer2;
    public Canvas dialogueContainer;
    public TextMeshProUGUI SpeakerTextContainer, DialogueTextContainer;
    GameObject Player;


    a_Interactable currently_selected_interactable;



    int memorysSeen = 0;
    public int memoriesTillEvent = 4;

    public void incrementMemorysSeen()
    {
        memorysSeen++;
        if(memorysSeen >= memoriesTillEvent)
            GlobalEvents.get().memory_threshold_reached.Invoke();
    }


    //Memories
    
    ScriptableMemoryScript curMemory;
    int currentMemoryProgress=-1;


    //other

	private void Start() {
        getComponentFields();
        hidePrompt();
        GlobalEvents.get().hidePrompt.AddListener(hidePrompt);
        GlobalEvents.get().interaction_input.AddListener(interacted);
	}

    void getComponentFields() { Player = FindFirstObjectByType<PlayerController>().gameObject; }

    public void Update()
    {
        if (currentMemoryProgress >= 0 && Input.GetButtonDown("Interact"))
        {
            TypewriterEffect typer = FindFirstObjectByType<TypewriterEffect>();
            if (typer.isTyping())   {   typer.SkipTyping();     }
            else                    {   advanceMemory();        }
        }
    }

    public void interacted()
    {
        if(currently_selected_interactable == null)
        {
            Debug.LogError(currently_selected_interactable + " is null! expected interactable");
            return;
        }
        currently_selected_interactable.activate();
    }
    
    void showPrompt(string text)
    { 
        promptTextContainer1.text = text;
        promptTextContainer2.text = text;
        promptContainer.gameObject.SetActive(true);
    }

    public void hidePrompt()
    {
        promptContainer.gameObject.SetActive(false);
    }

    public void playMemory(ScriptableMemoryScript memory) {
        psuedoPause(true);
        curMemory=memory;
        showAndBeginDialogue();
    }

    void psuedoPause(bool startPsuedoPause) {
        Time.timeScale= startPsuedoPause? 0.0f:1.0f;
        FindFirstObjectByType<PlayerController>().enabled=!startPsuedoPause;
    }

    void showAndBeginDialogue() {
        currentMemoryProgress=-1;
        dialogueContainer.gameObject.SetActive(true);
        advanceMemory();
    }

    void advanceMemory() {
        currentMemoryProgress+=1;
        if (currentMemoryProgress >= curMemory.lines.Count)
        {
            endMemory();
            return;
        }
        setTextAndTypewriter();
    }

    void setTextAndTypewriter() {
        updateSpeakerName(curMemory, currentMemoryProgress);
        DialogueTextContainer.GetComponent<TypewriterEffect>().TypeText(curMemory.lines[currentMemoryProgress].line);
    }

    void endMemory() 
    {
        currentMemoryProgress=-1;
        curMemory = null;
        dialogueContainer.gameObject.SetActive(false);   
        psuedoPause(false);
    }

    public void updateSpeakerName(ScriptableMemoryScript script, int progress)
    {
        memorySpeaker speaker = script.speakers[script.lines[progress].speaker];
        SpeakerTextContainer.text = speaker.name;
        SpeakerTextContainer.color = speaker.color;
	}

    public a_Interactable getCurrentInteractable() { return currently_selected_interactable; }

    public void updateCurrentInteractable(a_Interactable i)
    {
        currently_selected_interactable = i;
        if(i == null)
            hidePrompt();
        else
            showPrompt(i.Interaction_Prompt);
    }

}