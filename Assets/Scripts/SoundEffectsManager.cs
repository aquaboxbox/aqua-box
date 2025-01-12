using System.Collections.Generic;
using UnityEngine;

public class SoundEffectsManager : MonoBehaviour
{
    // Singleton instance for easy global access
    public static SoundEffectsManager Instance;

    [System.Serializable]
    public class SoundEffect
    {
        public string name; // Name for identifying the sound effect
        public AudioClip clip; // The audio clip to play
    }

    public List<SoundEffect> soundEffects; // List of sound effects
    private Dictionary<string, AudioClip> soundEffectsDictionary;
    private AudioSource audioSource;
    private HashSet<string> currentlyPlayingSounds; // Tracks sounds that are currently playing

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize the dictionary and audio source
        soundEffectsDictionary = new Dictionary<string, AudioClip>();
        audioSource = gameObject.AddComponent<AudioSource>();
        currentlyPlayingSounds = new HashSet<string>();

        foreach (var soundEffect in soundEffects)
        {
            if (!soundEffectsDictionary.ContainsKey(soundEffect.name))
            {
                soundEffectsDictionary.Add(soundEffect.name, soundEffect.clip);
            }
        }
    }

    /// <summary>
    /// Plays a sound effect by name if it is not currently playing.
    /// </summary>
    /// <param name="name">Name of the sound effect to play</param>
    public void PlaySound(string name)
    {
        if (!currentlyPlayingSounds.Contains(name))
        {
            if (soundEffectsDictionary.TryGetValue(name, out AudioClip clip))
            {
                currentlyPlayingSounds.Add(name);
                audioSource.PlayOneShot(clip);
                StartCoroutine(RemoveFromCurrentlyPlaying(name, clip.length));
            }
            else
            {
                Debug.LogWarning($"SoundEffect '{name}' not found!");
            }
        }
    }

    /// <summary>
    /// Plays a sound effect with custom volume if it is not currently playing.
    /// </summary>
    /// <param name="name">Name of the sound effect to play</param>
    /// <param name="volume">Volume of the sound effect</param>
    public void PlaySound(string name, float volume)
    {
        if (!currentlyPlayingSounds.Contains(name))
        {
            if (soundEffectsDictionary.TryGetValue(name, out AudioClip clip))
            {
                currentlyPlayingSounds.Add(name);
                audioSource.PlayOneShot(clip, volume);
                StartCoroutine(RemoveFromCurrentlyPlaying(name, clip.length));
            }
            else
            {
                Debug.LogWarning($"SoundEffect '{name}' not found!");
            }
        }
    }

    /// <summary>
    /// Removes a sound from the currently playing set after its duration.
    /// </summary>
    /// <param name="name">Name of the sound effect</param>
    /// <param name="duration">Duration of the sound effect</param>
    private System.Collections.IEnumerator RemoveFromCurrentlyPlaying(string name, float duration)
    {
        yield return new WaitForSeconds(duration);
        currentlyPlayingSounds.Remove(name);
    }

    /// <summary>
    /// Stops all currently playing sounds on this AudioSource.
    /// </summary>
    public void StopAllSounds()
    {
        audioSource.Stop();
        currentlyPlayingSounds.Clear(); // Clear the set of currently playing sounds
    }
}
