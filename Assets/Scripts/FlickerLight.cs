using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickerLights : MonoBehaviour
{
    [Header("Flicker Settings")]
    public float minIntensity = 100f; // minimum brightness
    public float maxIntensity = 40000f;   // maximum brightness
    public float flickerSpeed = 2f;

    private Light[] lights;
    private float[] offsets;

    void Start()
    {
        // Get all child lights dynamically
        lights = GetComponentsInChildren<Light>();
        offsets = new float[lights.Length];

        for (int i = 0; i < lights.Length; i++)
        {
            // Random offset per light to make them flicker independently
            offsets[i] = Random.Range(0f, 100f);
        }
    }

    void Update()
    {
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                // Perlin noise for smooth flicker
                float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, offsets[i]);
                lights[i].intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
            }
        }
    }
}


