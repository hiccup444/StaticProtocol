using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public int damage = 10;
    public float knockbackForce = 5f;
    public int hitsBeforeVisorDamage = 3;
    public float bulletLifetime = 5f;

    private static int globalHitCount = 0; // Static to persist across bullet instances
    private bool hasHit = false; // Prevent multiple hits from same bullet

    void Start()
    {
        // Auto-destroy bullet after lifetime to prevent accumulation
        Destroy(gameObject, bulletLifetime);

        // Validate components
        if (GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("TurretBullet: Missing Rigidbody component!");
        }
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError("TurretBullet: Missing Collider component!");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Prevent multiple hits from the same bullet
        if (hasHit) return;

        Debug.Log("Bullet hit: " + collision.gameObject.name + " with tag: " + collision.gameObject.tag);

        if (collision.gameObject.CompareTag("Player"))
        {
            hasHit = true; // Mark this bullet as having hit
            HandlePlayerHit(collision.gameObject);
        }

        // Destroy bullet after impact
        Destroy(gameObject);
    }

    void HandlePlayerHit(GameObject playerObj)
    {
        Debug.Log("Bullet hit player!");

        // Apply health damage
        PlayerHealth health = playerObj.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Debug.Log("Applied " + damage + " damage to player");
        }
        else
        {
            Debug.LogWarning("Player hit but no PlayerHealth component found!");
        }

        // Increment global hit count
        globalHitCount++;
        Debug.Log("Global hit count: " + globalHitCount);

        // Apply visor damage after X hits
        HelmetHandler visor = playerObj.GetComponentInChildren<HelmetHandler>();
        if (visor != null && globalHitCount >= hitsBeforeVisorDamage)
        {
            visor.TakeDamage();
            Debug.Log("Applied visor damage!");
            globalHitCount = 0; // Reset counter
        }
        else if (visor == null)
        {
            Debug.LogWarning("Player hit but no HelmetHandler component found!");
        }

        // Apply knockback
        Vector3 dir = (playerObj.transform.position - transform.position).normalized;
        PlayerController playerController = playerObj.GetComponentInChildren<PlayerController>();
        if (playerController != null)
        {
            playerController.AddForce(dir * knockbackForce);
            Debug.Log("Applied knockback force");
        }
    }
}