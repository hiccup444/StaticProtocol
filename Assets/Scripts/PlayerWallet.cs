using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public int currentMoney = 0;

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        Debug.Log($"Wallet: {currentMoney} credits");
    }

    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            return true;
        }
        return false;
    }
}

