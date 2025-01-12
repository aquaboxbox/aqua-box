using UnityEngine;

public class CollisionSound : MonoBehaviour
{
    public AudioClip collisionSound; // Assign in the inspector
    private AudioSource audioSource;

    void Start()
    {
        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();
        
        // If no AudioSource is found, log an error
        if (audioSource == null)
        {
            Debug.LogError("No AudioSource found on " + gameObject.name);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the AudioSource and sound clip are assigned
        if (audioSource != null && collisionSound != null)
        {
            // Play the sound when a collision happens

            audioSource.pitch = Random.Range(0.8f, 1.2f); // Random pitch between 0.8 and 1.2
            audioSource.PlayOneShot(collisionSound);

        }
    }
}
