using System.Collections;
using System.Collections.Generic;
using Thirdweb;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : Singleton<MainMenu> {

	public GameObject mainMenuObj;
	public GameObject optionsMenuObj;
	public GameObject instructionsMenuObj;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI AddressWalletText;
    public PopupRanking popupRankingGO;
    public PopupToast popupToast;

    private void Start() {
        if (!PlayerPrefs.HasKey("score1")) {
            PlayerPrefs.SetInt("score1", 0);
            PlayerPrefs.SetInt("score2", 0);
            PlayerPrefs.SetInt("score3", 0);
        }

        ShowPopupRanking(false);
        staminaText.text = $"{PlayerPrefs.GetInt(Constants.LIFE_PREFS, UserDataManagers.MAX_LIFE)}/{UserDataManagers.MAX_LIFE}";
        popupToast.Show("Hi! Welcome you to Math Game !");
    }
    
    public void PlayGame()
    {
	    if (string.IsNullOrEmpty(UserDataManagers.Instance.currentAddressWallet))
	    {
		    popupToast.Show("You need connect wallet first!");
		    return;
	    }
	    int life = PlayerPrefs.GetInt(Constants.LIFE_PREFS, UserDataManagers.MAX_LIFE);
	    if (life > 0)
	    {
		    PlayerPrefs.SetInt(Constants.LIFE_PREFS, life - 1);
		    SceneManager.LoadScene("Level 1");
	    }
	    else
	    {
		    popupToast.Show("You don't have enough stamina!");
	    }
    }

    public void EnterName() {
        instructionsMenuObj.SetActive(false);
        mainMenuObj.SetActive(false);
    }

    public void QuitGame() {
		Application.Quit();
		Debug.Log("Quit");
	}

	public void OptionsMenu() {
		mainMenuObj.SetActive(false);
		optionsMenuObj.SetActive(true);
	}

	public void InstructionsMenu() {
		mainMenuObj.SetActive(false);
		instructionsMenuObj.SetActive(true);
	}

	public void Back() {
		optionsMenuObj.SetActive(false);
		instructionsMenuObj.SetActive(false);
		mainMenuObj.SetActive(true);
	}

	public void ShowPopupRanking(bool isShow = true)
	{
		popupRankingGO.gameObject.SetActive(isShow);
	}
}
