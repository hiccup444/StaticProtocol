using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorCamera : MonoBehaviour
{
    public Transform[] spectatePoints; // optional fixed points or player transforms
    private int currentIndex = 0;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.enabled = false; // start disabled
    }

    public void Activate()
    {
        cam.enabled = true;
        Debug.Log("Spectator mode activated.");
    }

    void Update()
    {
        if (!cam.enabled) return;

        // Cycle through points with arrow keys
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex = (currentIndex + 1) % spectatePoints.Length;
            MoveToPoint();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex = (currentIndex - 1 + spectatePoints.Length) % spectatePoints.Length;
            MoveToPoint();
        }
    }

    private void MoveToPoint()
    {
        if (spectatePoints.Length == 0) return;
        transform.position = spectatePoints[currentIndex].position;
        transform.rotation = spectatePoints[currentIndex].rotation;
    }
}
