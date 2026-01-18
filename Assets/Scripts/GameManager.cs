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
    [SerializeField]
    private UnityEvent gameOver;

    [Header("Timer")]
    [SerializeField]
    private bool hasTimer = false;
    [SerializeField]
    private float countdownTime = 60f;
    private float currentTime;
    private bool timerRunning = false;
    [SerializeField]
    private TextMeshProUGUI timeDisplay;


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
        currentTime = countdownTime;
    }

    private void Update()
    {
        if (!timerRunning || !hasTimer) return;

        currentTime -= Time.deltaTime;

        if (timeDisplay)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timeDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else
            Debug.LogWarning("No Timer assigned!");

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            GameOver();
        }



    }

    public void StopTimer()
    {
        timerRunning = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isGameRunning = false;

    }

    public void ResumeTimer()
    {
        timerRunning = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isGameRunning = true;

    }

    public float GetTimeRemaining()
    {
        return currentTime;
    }

    public void GameOver()
    {
        timerRunning = false;
        Debug.Log("Timer finished!");
        timeDisplay.text = string.Format("{0:00}:{1:00}", 0, 0);
        isGameRunning = false;
        Cursor.lockState = CursorLockMode.None;
        gameOver.Invoke();
        //meter aqui as merdas de quando o jogo acaba
    }
    public void PauseGame()
    {
        if (canPause)
        {
            FlipisPaused();
            if (isPaused == true)
            {
                StopTimer();
                pauseGame.Invoke();
            }
            else
            {
                ResumeTimer();
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

}
