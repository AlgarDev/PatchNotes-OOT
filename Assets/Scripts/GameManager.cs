using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("State Control")]
    [SerializeField]
    public static bool isGameRunning = false;
    private PlayerInputActions inputActions;
    public bool canPause = true;
    [SerializeField]
    private UnityEvent pauseGame;
    [SerializeField]
    private UnityEvent unPauseGame;
    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        inputActions.Player.Pause.performed += _ => PauseGame();
        isGameRunning = true;
    }
    private void Start()
    {
        isGameRunning = true;
    }


    public void PauseGame()
    {
        if (canPause)
        {
            FlipisPaused();
            if (isPaused == true)
            {
                pauseGame.Invoke();
            }
            else
            {
                unPauseGame.Invoke();

            }
        }


    }
    public void CanPause()
    {
        canPause = true;
    }
    public void FlipisPaused()
    {
        isPaused = !isPaused;

    }
    public void GoToMainMenu()
    {
        UnpauseGame();
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void PauseGameCall()
    {
        Cursor.lockState = CursorLockMode.None; 
        isGameRunning = false;
        Time.timeScale = 0;
        var audioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioSource in audioSources)
        {
            audioSource.Pause();
        }

    }
    public void UnpauseGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        isGameRunning = true;
        Time.timeScale = 1;
        var audioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioSource in audioSources)
        {
            audioSource.UnPause();
        }
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }
}
