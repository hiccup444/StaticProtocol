using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public int damage = 10;
    public float knockbackForce = 5f;
    public int hitsBeforeVisorDamage = 3;

    private int hitCount = 0;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            HelmetHandler visor = collision.gameObject.GetComponentInChildren<HelmetHandler>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }

            hitCount++;

            // Apply visor damage after X hits
            if (visor != null && hitCount >= hitsBeforeVisorDamage)
            {
                visor.TakeDamage();
                hitCount = 0;
            }

            // Apply knockback
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = (collision.transform.position - transform.position).normalized;
                rb.AddForce(dir * knockbackForce, ForceMode.Impulse);
            }
        }

        Destroy(gameObject); // destroy bullet after impact
    }
}

