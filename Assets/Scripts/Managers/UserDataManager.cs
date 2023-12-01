
using System;
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
            contract = sdk.GetContract(Constants.SmartContractAddress);
        }
        catch (Exception e)
        {
            Debug.LogError("UserDataManager Start Error : " + e.ToString());
        }
        Debug.Log("UserDataManager Started!");

        GetWalletBalance();
        
        var data = await contract.Read<int>("getTopPlayers");
        Debug.Log("data");
    }

    public async void GetWalletBalance()
    {
        currencyValue = await sdk.wallet.GetBalance();
        Debug.Log("GetWalletBalance");
        Debug.Log(currencyValue);
    }
}