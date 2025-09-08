using UnityEngine;

public class HelmetDamageRelay : MonoBehaviour
{
    [Header("References")]
    public HelmetHandler helmetHandler; // assign your HelmetHandler (UI overlay)

    [Header("Settings")]
    public float minImpactVelocity = 5f;
    public float damageCooldown = 0.5f;

    private CharacterController controller;
    private Vector3 lastVelocity;
    private float lastDamageTime = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        // Store velocity for collision impact calculation
        lastVelocity = controller.velocity;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (helmetHandler == null) return;

        // Calculate approximate impact speed along collision normal
        float impactSpeed = Vector3.Dot(lastVelocity, -hit.normal);

        if (impactSpeed >= minImpactVelocity && Time.time - lastDamageTime >= damageCooldown)
        {
            helmetHandler.TakeDamage();
            lastDamageTime = Time.time;
            Debug.Log($"Helmet took damage! Impact speed: {impactSpeed:F2} on {hit.collider.name}");
        }
    }
}
