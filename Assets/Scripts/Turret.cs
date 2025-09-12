using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Turret Settings")]
    public float range = 20f;            // detection radius
    public float fireRate = 1f;
    public float bulletForce = 25f;
    public float fieldOfView = 90f;      // cone angle in degrees

    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform firePointTop;
    public Transform firePointBottom;

    private Transform player;
    private float fireCooldown = 0f;
    private bool useLeftBarrel = true; // toggle between barrels

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        if (PlayerInSight())
        {
            // Rotate turret toward player
            Vector3 dir = (player.position - transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * 5f);

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
        if (distance > range) return false;

        // check cone (field of view)
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > fieldOfView * 0.5f) return false;

        // optional: raycast to check walls
        if (Physics.Raycast(transform.position, dirToPlayer, out RaycastHit hit, range))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;

        Transform currentFirePoint = useLeftBarrel ? firePointTop : firePointBottom;
        if (currentFirePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, currentFirePoint.position, currentFirePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.AddForce(currentFirePoint.forward * bulletForce, ForceMode.Impulse);

        useLeftBarrel = !useLeftBarrel;
    }
}
