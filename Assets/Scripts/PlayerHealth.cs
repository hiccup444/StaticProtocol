using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("References")]
    public Oxygen oxygenSystem; // Assign your Oxygen script here

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (oxygenSystem == null)
        {
            oxygenSystem = GetComponent<Oxygen>();
        }
    }

    void Update()
    {
        if (isDead) return;

        // Instant death if oxygen runs out
        if (oxygenSystem != null && oxygenSystem.currentOxygen <= 0)
        {
            Die("Ran out of oxygen!");
        }

        // Optional: could handle gradual damage over time here
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Die("Killed by damage!");
        }
    }

    private void Die(string reason)
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"Player died: {reason}");

        // Disable player movement
        var controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

    }

    private void Respawn()
    {
        // Implement respawn logic here (e.g., reset position, health, etc.)
        currentHealth = maxHealth;
        isDead = false;
        var controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = true;
        Debug.Log("Player respawned.");
    }

    public bool IsDead()
    {
        return isDead;
    }
}

