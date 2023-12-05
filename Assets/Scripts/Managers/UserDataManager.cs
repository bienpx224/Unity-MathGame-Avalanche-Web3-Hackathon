
using System;
using System.Collections.Generic;
using QRCoder;
using Thirdweb;
using UnityEngine;

public class UserDataManager : Singleton<UserDataManager>
{
    public ThirdwebSDK sdk;
    public Contract contract;
    public CurrencyValue currencyValue;
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
        

    }

    public async void GetWalletBalance()
    {
        currencyValue = await sdk.wallet.GetBalance();
        Debug.Log("GetWalletBalance");
        Debug.Log(currencyValue);
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
            var data = await contract.Read<int>("getMyScore");
            Debug.Log(data.ToString());
            onComplete?.Invoke(data.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError("TestGetValue Error 1: " + e);
        }
    }
}