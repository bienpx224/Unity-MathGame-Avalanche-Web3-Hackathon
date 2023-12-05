using System;
using System.Collections.Generic;
using Thirdweb.EWS;
using TMPro;
using UnityEngine;

public class PopupRanking : MonoBehaviour
{
    [Header("My Info")] [SerializeField] private TextMeshProUGUI myAddressText;
    [SerializeField] private TextMeshProUGUI myScoreText;

    [Header("Top 10 Players")] [SerializeField]
    private List<PlayerRanking> topPlayers;

    private void Start()
    {
    }

    private void OnEnable()
    {
        InitUI();
    }

    public void InitUI()
    {
        myAddressText.text = UserDataManagers.ProcessString(UserDataManagers.Instance.currentAddressWallet);
        myScoreText.text = UserDataManagers.Instance.currentScore.ToString();
    }

    public void HidePopupRanking()
    {
        this.gameObject.SetActive(false);
    }
}