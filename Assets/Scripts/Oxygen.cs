using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oxygen : MonoBehaviour
{
    [Header("Oxygen Settings")]
    public float maxOxygen = 100f;
    public float currentOxygen;

    [Header("References")]
    public HelmetHandler helmetHandler; // assign your helmet script here

    [Header("Environment")]
    public bool inSpace = true; // true = outside vacuum, false = oxygenated room

    void Start()
    {
        currentOxygen = maxOxygen;
    }

    void Update()
    {
        if (helmetHandler == null) return;

        if (inSpace)
        {
            float drainRate = GetDrainRateByHelmetStage();
            currentOxygen -= drainRate * Time.deltaTime;
            currentOxygen = Mathf.Clamp(currentOxygen, 0, maxOxygen);

            if (currentOxygen <= 0)
            {
                // Player suffocating
                Debug.Log("Player has run out of oxygen!");

            }
        }
        else
        {
            // Optional: slowly refill oxygen when in a room
            currentOxygen += 20f * Time.deltaTime; // 20 units per second
            currentOxygen = Mathf.Clamp(currentOxygen, 0, maxOxygen);
        }
    }

    float GetDrainRateByHelmetStage()
    {
        if (helmetHandler == null) return 0;

        switch (helmetHandler.currentDurability)
        {
            case 3: return 0f;   // no cracks
            case 2: return 5f;   // light cracks
            case 1: return 10f;  // medium cracks
            case 0: return 20f;  // broken
            default: return 0f;
        }
    }

    public void RefillOxygen(float amount)
    {
        currentOxygen += amount;
        currentOxygen = Mathf.Clamp(currentOxygen, 0, maxOxygen);
    }
}
