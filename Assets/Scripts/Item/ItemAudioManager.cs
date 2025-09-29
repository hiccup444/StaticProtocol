using UnityEngine;

public class ItemAudioManager : MonoBehaviour
{
    private static ItemAudioManager instance;
    public static ItemAudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ItemAudioManager");
                instance = go.AddComponent<ItemAudioManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("Settings")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    public float ambientVolume = 0.5f;
    [Range(0f, 1f)]
    public float spatialBlend = 1f; // 1 = full 3D audio

    [Header("3D Audio Settings")]
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
    public float minDistance = 1f;
    public float maxDistance = 20f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Play sound at a specific world position (for pickup/drop)
    public void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip != null)
        {
            GameObject tempAudio = new GameObject("TempAudio_" + clip.name);
            tempAudio.transform.position = position;
            AudioSource source = tempAudio.AddComponent<AudioSource>();
            
            source.clip = clip;
            source.volume = volume * sfxVolume;
            source.spatialBlend = spatialBlend;
            source.rolloffMode = rolloffMode;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.Play();
            
            Destroy(tempAudio, clip.length + 0.1f);
        }
    }

    public void PlayPickupSound(AudioClip clip, Vector3 position)
    {
        PlaySoundAtPosition(clip, position, sfxVolume);
    }

    public void PlayDropSound(AudioClip clip, Vector3 position)
    {
        PlaySoundAtPosition(clip, position, sfxVolume);
    }

    // Setup ambient audio source on a GameObject
    public AudioSource SetupAmbientSource(GameObject obj, AudioClip clip)
    {
        if (clip == null || obj == null) return null;

        AudioSource source = obj.GetComponent<AudioSource>();
        if (source == null)
        {
            source = obj.AddComponent<AudioSource>();
        }

        source.clip = clip;
        source.volume = ambientVolume;
        source.loop = true;
        source.spatialBlend = spatialBlend;
        source.rolloffMode = rolloffMode;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.playOnAwake = false;

        return source;
    }

    public void PlayAmbient(AudioSource source)
    {
        if (source != null && !source.isPlaying)
        {
            source.Play();
        }
    }

    public void StopAmbient(AudioSource source)
    {
        if (source != null && source.isPlaying)
        {
            source.Stop();
        }
    }

    public void FadeOutAmbient(AudioSource source, float duration = 0.5f)
    {
        if (source != null)
        {
            StartCoroutine(FadeOutCoroutine(source, duration));
        }
    }

    private System.Collections.IEnumerator FadeOutCoroutine(AudioSource source, float duration)
    {
        if (source == null) yield break;

        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration && source != null)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        if (source != null)
        {
            source.Stop();
            source.volume = ambientVolume;
        }
    }
}