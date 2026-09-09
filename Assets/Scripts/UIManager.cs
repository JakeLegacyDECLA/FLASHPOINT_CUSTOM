/*
* Title: UIManager
* Author: R. Hurtado
* 
* Description: 
* Script desigend to handle the movement across canvas to load all
* content inside the only created scene Game.
* The movement between canvas changes which canvas is loaded, or stopped
* and the time scale of the game, to pause it or initialize it.
*
* AI Usage:
* The distribution of the code and creation of individual functions that handle 
* the direct call to the function "Go" was suggested by AI.
* The implementation originally suggested an list of states; however that idea
* was discarted. 
*/
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class UIManager : MonoBehaviour
{
    private static bool startInGame = false;
    /*
    * Establishes the only public and available instance of the UIManager to
    * be accessible from any other class.
    */
    public static UIManager Instance;

    /*
    * Canvas' with the different game screens
    */
    public GameObject startScreen;
    public GameObject learnScreen;
    public GameObject gameScreen;
    public GameObject winScreen;
    public GameObject loseScreen;
    public GameObject pauseScreen;

    //------------------------------ FUNTIONS ----------------------------
    /*
    * Awake()
    * Build function that runs as soon as the object is created before Start()
    * In this case used to refer the instantiation to itself.
    */
    void Awake()
    {
        Instance = this;
    }

    /*
    * Start()
    * Build function that runs as soon as the object is created after Awake()
    *
    * Depending on the bool startInGame, it goes directly to the Play() screen or
    * the Menu() screen. This is used to restart the game once the player clicks any
    * button Restart or Retry, only instead of sending it to the Menu(), send directly
    * to the game.
    */
    void Start()
    {
        if (startInGame)
            Go("Play");
        else
            Go("Start");
    }

    /*
    * Go()
    * Function that handles the deactivation of all canvas and activation of the given
    * name one in the parameter estado, along with the time management of each, stopping
    * or playing the actual game.
    * 
    */
    void Go(string estado)
    {
        startScreen.SetActive(false);
        learnScreen.SetActive(false);
        gameScreen.SetActive(false);
        winScreen.SetActive(false);
        loseScreen.SetActive(false);
        pauseScreen.SetActive(false);

        switch (estado)
        {
            case "Start":
                startScreen.SetActive(true);
                Time.timeScale = 0;
                break;
            case "Learn":
                learnScreen.SetActive(true);
                Time.timeScale = 0;
                break;
            case "Play":
                gameScreen.SetActive(true);
                Time.timeScale = 1;
                break;
            case "Win":
                winScreen.SetActive(true);
                Time.timeScale = 0;
                break;
            case "Lose":
                loseScreen.SetActive(true);
                Time.timeScale = 0;
                break;
            case "Pause":
                pauseScreen.SetActive(true);
                Time.timeScale = 0;
                break;
        }
    }

    /*
    * ChangeWithDelay()
    * Coroutine that allows to pass certain seconds before activating
    * the indicated screen. This is used to delay Win and Lose.
    */
    private IEnumerator ChangeWithDelay(string destination, float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds); // ver nota abajo sobre Realtime
        Go(destination);
    }

    /*
    * WinWithDelay()
    * Application of the coroutine by calling the Win screen after given seconds.
    */
    public void WinWithDelay(float seconds)
    {
        StartCoroutine(ChangeWithDelay("Win", seconds));
    }

    /*
    * WinWithDelay()
    * Application of the coroutine by calling the Win screen after given seconds.
    */
    public void LoseWithDelay(float seconds)
    {
        StartCoroutine(ChangeWithDelay("Lose", seconds));
    }

    /*
    * Retry()
    * Function that changes the start bool condition startInGame to allow next start 
    * in Play(), sets timeScale = 1 to start the game inmediately and restarts the 
    * scene.
    */
    public void Retry()
    {
        startInGame = true;
        Time.timeScale = 1; // importante: si estabas en pausa/derrota, timeScale estaba en 0
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /*
    * Play(), Learn(), Menu(), Win(), Lose(), Pause()
    * Individual functions that handle the call to the Go function 
    * with a concrete canva's name.
    */
    public void Play() => Go("Play");
    public void Learn() => Go("Learn");
    public void Menu() => Go("Start");
    public void Win() => Go("Win");
    public void Lose() => Go("Lose");
    public void Pause() => Go("Pause");
}