using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField]
    private float countdownTime = 60f;
    private float currentTime;
    private bool timerRunning = true;
    [SerializeField]
    private TextMeshProUGUI timeDisplay;
    [SerializeField]
    public static bool isGameRunning = true;

    private PlayerInputActions inputActions;
    [SerializeField]
    private UnityEvent pauseGame;
    [SerializeField]
    private UnityEvent unPauseGame;
    private bool isPaused = false;
    public bool canPause = true;
    [SerializeField]
    private UnityEvent gameOver;

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
    }
    private void Start()
    {
        isGameRunning = true;
        currentTime = countdownTime;
    }

    private void Update()
    {
        if (!timerRunning) return;

        currentTime -= Time.deltaTime;

        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timeDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);


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
