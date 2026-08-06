using UnityEngine;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour {

    public static bool isPaused;
    public KeyCode pauseKey= KeyCode.Escape;

    //These component when PAUSED need to be ACTIVE.
            //Add them to ToggleActiveComponents(b)
    public GameObject PauseMenuCanvas; //.setActive(true)

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
            isPaused=ResumeGame();
        }

    //If the pause key is pressed toggle the game's paused state.
    void Update() {
        if (Input.GetKeyDown(pauseKey)) isPaused= isPaused? ResumeGame():PauseGame(); 
    }
        bool PauseGame () {togglePauseActiveComponents(true );    togglePauseInactiveComponents(false); Time.timeScale=0; return true   ; }
        bool ResumeGame() {togglePauseActiveComponents(false);    togglePauseInactiveComponents(true ); Time.timeScale=1; return false  ; }

    void togglePauseActiveComponents  (bool activate) {
        PauseMenuCanvas.SetActive(activate);
    }

    void togglePauseInactiveComponents(bool activate) {
        PlayerController.enabled=activate;
    }

    public void ForceResumeGame(int Delay) { }
    public void ForceResumeGame() { isPaused=ResumeGame(); }

}
