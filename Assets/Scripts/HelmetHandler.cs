using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelmetHandler : MonoBehaviour
{
    [Header("Helmet Settings")]
    public int maxDurability = 3;
    public int currentDurability;

    [Header("Helmet Overlay UI")]
    public UnityEngine.UI.Image crackImage; // cracks sprite
    public Sprite[] crackStages; // assign sprites for different damage levels

    [Header("Collision Damage Settings")]
    public float minImpactVelocity = 5f; // minimum speed to take damage
    public float damageCooldown = 1f;    // prevent multiple hits in quick succession

    private bool isBroken = false;
    private float lastDamageTime = -999f;

    void Start()
    {
        currentDurability = maxDurability;
        UpdateOverlay();
    }

    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ignore if helmet is already broken
        if (isBroken) return;

        // Get impact velocity
        float impactSpeed = collision.relativeVelocity.magnitude;

        if (impactSpeed >= minImpactVelocity && Time.time - lastDamageTime >= damageCooldown)
        {
            Debug.Log($"Helmet took damage from collision with {collision.gameObject.name}, speed: {impactSpeed}");
            TakeDamage();
            lastDamageTime = Time.time;
        }
    }

    public void TakeDamage()
    {
        if (isBroken) return;

        currentDurability -= 1;
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

    public void RepairHelmet(int amount = 1)
    {
        if (isBroken) return; // maybe prevent repair after shatter

        currentDurability = Mathf.Min(currentDurability + amount, maxDurability);
        UpdateOverlay();
        Debug.Log($"Helmet repaired by {amount}, durability: {currentDurability}/{maxDurability}");
    }


    void BreakHelmet()
    {
        isBroken = true;
        Debug.Log("Helmet shattered! Oxygen leak begins.");
    }
}
