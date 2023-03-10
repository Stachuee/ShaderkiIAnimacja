using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;

    public bool playerAlive;

    [SerializeField]
    GameObject panel;
    [SerializeField]
    GameObject timer;

    [SerializeField]
    TextMeshProUGUI endTimer;
    [SerializeField]
    GameObject endPanel;

    public bool pause;
    [SerializeField]
    GameObject pauseMenu;

    private void Awake()
    {
        if (gameManager == null) gameManager = this;
        else Destroy(gameObject);

        playerAlive = true;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
        Debug.Log(Cursor.lockState);
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void KillPlayer()
    {
        timer.SetActive(false);
        panel.SetActive(true);
        playerAlive = false;
    }

    public void PauseGame()
    {
        pause = !pause;
        pauseMenu.SetActive(pause);
        if (!pause)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void Win()
    {
        Cursor.lockState = CursorLockMode.Confined;
        TimeSpan timeSpan = TimeSpan.FromSeconds(TargetCounter.time);
        string timeText = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        endTimer.text = timeText;
        endPanel.SetActive(true);
        playerAlive = false;
    }
}
