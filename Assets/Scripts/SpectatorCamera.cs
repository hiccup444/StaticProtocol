using UnityEngine;

public class SpectatorCamera : MonoBehaviour
{
    public Transform[] spectatePoints; // optional fixed points or player transforms
    private int currentIndex = 0;

    private Camera cam;
    private AudioListener listener;
    private Camera playerCamera;
    private AudioListener playerListener;

    void Awake()
    {
        // Get the spectator camera and listener
        cam = GetComponent<Camera>();
        listener = GetComponent<AudioListener>();

        if (cam == null)
            Debug.LogWarning("SpectatorCamera: No Camera found on this object.");

        if (listener == null)
        {
            listener = gameObject.AddComponent<AudioListener>();
        }

        cam.enabled = false;
        listener.enabled = false;

        // Find the main player camera
        playerCamera = Camera.main;
        if (playerCamera != null)
            playerListener = playerCamera.GetComponent<AudioListener>();
    }

    public void Activate()
    {
        cam.enabled = true;
        listener.enabled = true;

        // Disable player camera listener
        if (playerListener != null)
            playerListener.enabled = false;

        Debug.Log("Spectator mode activated.");
    }

    public void Deactivate()
    {
        cam.enabled = false;
        listener.enabled = false;

        // Re-enable player camera listener
        if (playerListener != null)
            playerListener.enabled = true;

        Debug.Log("Spectator mode deactivated.");
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
    