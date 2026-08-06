using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour {

    public static bool isPaused;
    public KeyCode pauseKey= KeyCode.Escape;

    //These component when PAUSED need to be ACTIVE.
            //Add them to ToggleActiveComponents(b)
    public GameObject PauseCanvas; //.setActive(true)
    public GameObject[] PauseMenus;

    public enum PAUSE_MENU_STATE { MISSING_MENU, LANDING, OPTIONS }
    PAUSE_MENU_STATE Menu_State = PAUSE_MENU_STATE.LANDING;

    //These components when PAUSE need to be DISABLED. 
            //Add them to ToggleInactiveComponents(b)
    PlayerController PlayerController; //.enabled=false

    
    void Start() {
        getComponentFields();
		initializeNonComponentFields();
    }
        void getComponentFields() {
            PlayerController= GetComponent<PlayerController>();
        }

        void initializeNonComponentFields() {
            ResumeGame();
        }

    //If the pause key is pressed toggle the game's paused state.
    void Update() {
        if (Input.GetKeyDown(pauseKey)) onPauseKeyPressed(); 
    }

    void onPauseKeyPressed() { if (isPaused) DecrementMenuState(); else PauseGame(); }

    public void DecrementMenuState() {
        switch (Menu_State) {
            case PAUSE_MENU_STATE.MISSING_MENU  : HeadToMenuIndex(PAUSE_MENU_STATE.LANDING)  ; break;
            case PAUSE_MENU_STATE.LANDING       : ResumeGame()                               ; break;
            case PAUSE_MENU_STATE.OPTIONS       : HeadToMenuIndex(PAUSE_MENU_STATE.LANDING)  ; break;

            default: Debug.LogFormat("Unimplemented Pause_Menu_State: {0}. HEADING TO LANDING", Menu_State); ;break;
        }
    }

    public void ResumeGame() {togglePauseActiveComponents(false);    togglePauseInactiveComponents(true ); Time.timeScale=1;                                            isPaused= false  ; }
           void PauseGame () {togglePauseActiveComponents(true );    togglePauseInactiveComponents(false); Time.timeScale=0; HeadToMenuIndex(PAUSE_MENU_STATE.LANDING); isPaused= true   ; }

    void togglePauseActiveComponents  (bool activate) {
        PauseCanvas.SetActive(activate);
    }

    void togglePauseInactiveComponents(bool activate) {
        PlayerController.enabled=activate;

    }

    void HeadToMenuIndex(PAUSE_MENU_STATE target) {
        foreach (GameObject menu in PauseMenus) { menu.SetActive(false); }
        try { 
            PauseMenus[(int)target].SetActive(true);
            Menu_State=target;
        }
        catch (NullReferenceException   ex){ HeadTo404( target, ex); }
        catch (IndexOutOfRangeException ex){ HeadTo404( target, ex); }

    }

    public void HeadToOptions() { HeadToMenuIndex(PAUSE_MENU_STATE.OPTIONS);}
    
    void HeadTo404( PAUSE_MENU_STATE target, Exception ex) {
        Debug.LogWarning( string.Format("Could Not find the {0}({1}) menu because of a {2}", target, (int)target, ex.GetType().ToString() ) );
        PauseMenus[0].SetActive(true);
        Menu_State=PAUSE_MENU_STATE.MISSING_MENU;

    }





}
