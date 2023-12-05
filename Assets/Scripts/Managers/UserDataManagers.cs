
using System;
using System.Collections.Generic;
using QRCoder;
using Thirdweb;
using UnityEngine;

public class UserDataManagers : PersistentSingleton<UserDataManagers>
{
    public ThirdwebSDK sdk;
    public Contract contract;
    public CurrencyValue currencyValue;
    public string currentAddressWallet = "";
    public int currentScore = 0;
    public double timeStartLevel = 0;
    public int life = 0;
    public static int MAX_LIFE = 4;
    
    async void Start()
    {
        try
        {
            sdk = ThirdwebManager.Instance.SDK;
            contract = sdk.GetContract(Constants.SmartContractAddress, Constants.SmartContractAddressABI);
        }
        catch (Exception e)
        {
            Debug.LogError("UserDataManager Start Error : " + e.ToString());
        }
        Debug.Log("UserDataManager Started!");
        life = PlayerPrefs.GetInt(Constants.LIFE_PREFS, MAX_LIFE);
    }

    public static double GetCurrentTimeSeconds()
    {
        double t = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
        return t;
    }
    public static string ProcessString(string input)
    {
        if (input.Length >= 10)
        {
            string firstFive = input.Substring(0, 5);
            string lastFive = input.Substring(input.Length - 5, 5);

            return $"{firstFive}...{lastFive}";
        }
        else
        {
            // Xử lý trường hợp chuỗi ngắn hơn 10 ký tự (nếu cần)
            return input;
        }
    }
    public async void GetWalletBalance()
    {
        currencyValue = await sdk.wallet.GetBalance();
        currentAddressWallet = await sdk.wallet.GetAddress();
        Debug.Log("GetWalletBalance");
        Debug.Log(currencyValue);
        Debug.Log("Current Address wallet");
        Debug.Log(currentAddressWallet);

        MainMenu.Instance.AddressWalletText.text = $"{ProcessString(currentAddressWallet)}";
        GetMyScore();
    }

    public async void GetTopPlayers()
    {
        Debug.Log("Start GetTopPlayers");
        
        try
        {
            Debug.Log("Call Contract GetBalance()");
            var data = await contract.Read<string>("getTopPlayers");
            Debug.Log(data.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError("GetTopPlayer Error 2: " + e);
        }
    }

    public async void TestGetValue()
    {
        Debug.Log("Start TestGetValue");
        try
        {
            Debug.Log("Call Write testGetValue");
            var data = await contract.Read<int>("testGetValue", 22);
            Debug.Log(data.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError("TestGetValue Error 1: " + e);
        }
    }
    public async void AddScore(int addScore = 0)
    {
        Debug.Log("Start AddScore");
        try
        {
            Debug.Log("Call Write Add Score");
            TransactionResult data = await contract.Write("updateScore", addScore);
            Debug.Log(data.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError("AddScore Error 1: " + e);
        }
    }

    public async void GetMyScore(Action<string> onComplete = null)
    {
        Debug.Log("Start getMyScore");
        try
        {
            Debug.Log("Call Read getMyScore");
            string data = await contract.Read<string>("getMyScore");
            Debug.Log(data.ToString());
            onComplete?.Invoke(data.ToString());
            currentScore = int.Parse(data.ToString());
            MainMenu.Instance.ScoreText.text = currentScore.ToString();
        }
        catch (Exception e)
        {
            Debug.LogError("TestGetValue Error 1: " + e);
        }
    }
}