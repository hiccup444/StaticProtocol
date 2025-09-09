using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false; // ✅ add this

    // Event for when the player dies
    public event Action OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return; // don’t take damage after death

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"{gameObject.name} took {amount} damage. Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return; // safety check
        isDead = true;

        Debug.Log($"{gameObject.name} died!");
        OnDeath?.Invoke(); // notify others
    }

    void Update()
    {
        // Quick test: press K to suicide
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(9999);
        }
    }
}


