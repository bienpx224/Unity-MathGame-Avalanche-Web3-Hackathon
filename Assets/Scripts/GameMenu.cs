using System;
using System.Collections;
using System.Collections.Generic;
using Thirdweb.EWS;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : Singleton<GameMenu>
{
    public GameObject playerUI;
    public GameObject pauseUI;
    public GameObject popupEndGame;
    [SerializeField] public TextMeshProUGUI finalScoreText;
    private int MAX_SCORE_BONUS = 400;
    private float timeBonusOffset = 1f;

    // Use this for initialization
    void Start()
    {
        UserDataManagers.Instance.timeStartLevel = UserDataManagers.GetCurrentTimeSeconds();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ShowPopupEndGame(bool isWin = true)
    {
        popupEndGame.SetActive(true);
        int finalScore = PlayerScore.playerScore;
        finalScore += CalcBonusScore(isWin);

        UserDataManagers.Instance.currentScore += finalScore;
        finalScoreText.text = $"SCORE : {finalScore}";
        
        UserDataManagers.Instance.AddScore(finalScore);
    }

    private int CalcBonusScore(bool isWin)
    {
        int time = (int)(UserDataManagers.GetCurrentTimeSeconds() - UserDataManagers.Instance.timeStartLevel);
        Debug.LogWarning("Time Bonus : " + time);
        int maxScoreBonus = (isWin ? MAX_SCORE_BONUS : MAX_SCORE_BONUS / 2);
        int bonus =   Math.Clamp(maxScoreBonus - (int)(time * timeBonusOffset), 0, MAX_SCORE_BONUS);
        Debug.LogWarning("Score Bonus : " + bonus);
        return bonus;
    }

    public void PauseGame()
    {
        PlayerLife.mainAudio.Pause();
        Time.timeScale = 0;
        playerUI.SetActive(false);
        pauseUI.SetActive(true);
    }

    public void ResumeGame()
    {
        PlayerLife.mainAudio.Play();
        Time.timeScale = 1.0f;
        pauseUI.SetActive(false);
        playerUI.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1.0f;
        PlayerScore.playerScore = 0;
        PlayerLife.lives = 0;
        PlayerLife.countLives = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1.0f;
        PlayerScore.playerScore = 0;
        PlayerLife.lives = 0;
        PlayerLife.countLives = 0;
        SceneManager.LoadScene("MainMenu");
    }
}