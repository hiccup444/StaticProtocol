using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    public Camera playerCamera;        // The normal player camera
    public SpectatorCamera spectatorCamera;

    private PlayerHealth health;

    void Start()
    {
        health = GetComponent<PlayerHealth>();
        health.OnDeath += HandleDeath;
    }

    void HandleDeath()
    {
        playerCamera.enabled = false;

        spectatorCamera.Activate();

        var controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
    }
}

