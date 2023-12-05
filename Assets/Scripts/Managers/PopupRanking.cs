

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupRanking : MonoBehaviour
{
    [Header("My Info")] 
    [SerializeField] private TextMeshProUGUI myAddressText;
    [SerializeField] private TextMeshProUGUI myScoreText;
    
    [Header("Top 10 Players")]
    [SerializeField] private List<PlayerRanking> topPlayers;

    private void Start()
    {
        
    }

    private void OnEnable()
    {
        InitUI();
    }

    public void InitUI()
    {
        myAddressText.text = UserDataManager.Instance.currencyValue.value;
        UserDataManager.Instance.GetMyScore((value) =>
        {
            myScoreText.text = value;
        });
    }
}