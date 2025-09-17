using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Turret Settings")]
    public float range = 20f;            // detection radius
    public float fireRate = 5f;
    public float bulletForce = 25f;
    public float fieldOfView = 180f;      // cone angle in degrees
    public float rotationSpeed = 5f;

    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform firePointTop;
    public Transform firePointBottom;

    [Header("Debug")]
    public bool showDebugGizmos = true;

    private Transform player;
    private float fireCooldown = 0f;
    private bool useLeftBarrel = true; // toggle between barrels

    void Start()
    {
        // More robust player finding
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("Turret found player: " + player.name);
        }
        else
        {
            Debug.LogError("Turret could not find player with 'Player' tag!");
        }

        // Validate components
        if (bulletPrefab == null)
            Debug.LogError("Turret: bulletPrefab not assigned!");
        if (firePointTop == null)
            Debug.LogError("Turret: firePointTop not assigned!");
        if (firePointBottom == null)
            Debug.LogError("Turret: firePointBottom not assigned!");
    }

    void Update()
    {
        if (player == null) return;

        if (PlayerInSight())
        {
            // Calculate direction to player but only on horizontal plane (Y-axis rotation only)
            Vector3 dirToPlayer = player.position - transform.position;
            dirToPlayer.y = 0; // Remove vertical component
            dirToPlayer = dirToPlayer.normalized;

            // Calculate target Y rotation
            float targetYRotation = Mathf.Atan2(dirToPlayer.x, dirToPlayer.z) * Mathf.Rad2Deg;

            // Smoothly rotate only on Y-axis
            Vector3 currentRotation = transform.eulerAngles;
            currentRotation.y = Mathf.LerpAngle(currentRotation.y, targetYRotation, Time.deltaTime * rotationSpeed);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, currentRotation.y, transform.eulerAngles.z);

            // Fire cooldown
            fireCooldown -= Time.deltaTime;
            if (fireCooldown <= 0f)
            {
                Shoot();
                fireCooldown = 1f / fireRate;
            }
        }
    }

    bool PlayerInSight()
    {
        // check distance
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > range)
        {
            return false;
        }

        // check cone (field of view) - only consider horizontal angle
        Vector3 dirToPlayer = (player.position - transform.position);
        dirToPlayer.y = 0; // Remove vertical component for FOV check
        dirToPlayer = dirToPlayer.normalized;

        Vector3 turretForward = transform.forward;
        turretForward.y = 0; // Remove vertical component
        turretForward = turretForward.normalized;

        float angle = Vector3.Angle(turretForward, dirToPlayer);
        if (angle > fieldOfView * 0.5f)
        {
            return false;
        }

        // raycast to check walls/obstacles
        Vector3 rayDirection = (player.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, rayDirection, out RaycastHit hit, range))
        {
            Debug.DrawRay(transform.position, rayDirection * range, Color.red, 0.1f);

            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    void Shoot()
{
    if (bulletPrefab == null) return;

    Transform currentFirePoint = useLeftBarrel ? firePointTop : firePointBottom;
    if (currentFirePoint == null) return;

    // Spawn bullet
    GameObject bullet = Instantiate(bulletPrefab, currentFirePoint.position, currentFirePoint.rotation);


    Vector3 targetPos = player.position + Vector3.up * 1.5f;

    Vector3 targetDir = (targetPos - currentFirePoint.position).normalized;

    // Apply velocity toward target
    Rigidbody rb = bullet.GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.velocity = targetDir * bulletForce;
    }

    useLeftBarrel = !useLeftBarrel;
}


    // Debug visualization in Scene view
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);

        // Draw field of view cone (horizontal only)
        Gizmos.color = Color.red;
        Vector3 forward = transform.forward;
        forward.y = 0; // Keep only horizontal direction
        forward = forward.normalized;

        Vector3 leftBoundary = Quaternion.AngleAxis(-fieldOfView * 0.5f, Vector3.up) * forward;
        Vector3 rightBoundary = Quaternion.AngleAxis(fieldOfView * 0.5f, Vector3.up) * forward;

        Gizmos.DrawRay(transform.position, leftBoundary * range);
        Gizmos.DrawRay(transform.position, rightBoundary * range);
    }
}