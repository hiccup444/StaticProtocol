using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelmetHandler : MonoBehaviour
{
    [Header("Helmet Settings")]
    public int maxDurability = 3;
    public int currentDurability;

    [Header("Helmet Overlay UI")]
    public CanvasGroup helmetOverlay; // UI canvas over camera
    public UnityEngine.UI.Image crackImage; // cracks sprite
    public Sprite[] crackStages; // assign sprites for different damage levels

    private bool isBroken = false;

    void Start()
    {
        currentDurability = maxDurability;
        UpdateOverlay();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage();
        }
    }

    public void TakeDamage()
    {
        if (isBroken) return;
        int damage = 1;
        currentDurability -= damage;
        if (currentDurability <= 0)
        {
            currentDurability = 0;
            BreakHelmet();
        }

        UpdateOverlay();
    }

    void UpdateOverlay()
    {
        if (crackImage == null || crackStages.Length == 0) return;

        float damagePercent = 1f - (float)currentDurability / maxDurability;
        int stage = Mathf.FloorToInt(damagePercent * (crackStages.Length - 1));
        crackImage.sprite = crackStages[stage];
    }

    void BreakHelmet()
    {
        isBroken = true;
        Debug.Log("Helmet shattered! Oxygen leak begins.");
        // TODO: Tell the OxygenSystem to start draining
    }
}

