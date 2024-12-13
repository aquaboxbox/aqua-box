using System.Collections;
using UnityEngine;
using TMPro; // For TextMeshPro support

public class CubeTriggerScript : MonoBehaviour
{
    public TextMeshProUGUI countdownText; // Assign the TextMeshPro text element in the Inspector
    public float countdownDuration = 3.0f; // Countdown duration in seconds
    public Transform xrOrigin; // Assign the XR Origin object in the Inspector
    public Vector3 teleportLocation; // Set the world coordinates of the teleport location

    private bool isCountingDown = false; // To prevent multiple triggers

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Object entered: {other.name}, Tag: {other.tag}");

        // Check if the entering object has the tag "TriggerBox"
        if (other.CompareTag("TriggerBox") && !isCountingDown)
        {
            Debug.Log("TriggerBox detected! Starting countdown...");
            countdownText.gameObject.SetActive(true); // Make text visible
            StartCoroutine(StartCountdown());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Object exited: {other.name}, Tag: {other.tag}");

        // If leaving the platform, stop the countdown and hide the text
        if (other.CompareTag("TriggerBox") && isCountingDown)
        {
            Debug.Log("Left the platform before countdown finished.");
            StopAllCoroutines();
            countdownText.gameObject.SetActive(false); // Hide text
            isCountingDown = false; // Reset state
        }
    }

    private IEnumerator StartCountdown()
    {
        isCountingDown = true;

        float remainingTime = countdownDuration;

        // Countdown logic
        while (remainingTime > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.Ceil(remainingTime).ToString(); // Update countdown text
            }
            Debug.Log($"Countdown: {remainingTime}");
            yield return new WaitForSeconds(1.0f); // Wait for 1 second
            remainingTime--;
        }

        // Show "Go!" at the end of the countdown
        if (countdownText != null)
        {
            countdownText.text = "Go!";
        }
        Debug.Log("Countdown finished. Teleporting player...");

        yield return new WaitForSeconds(1.0f); // Keep "Go!" visible for 1 second

        // Hide the text and teleport the XR Origin
        countdownText.gameObject.SetActive(false); // Hide text
        TeleportXROrigin();

        isCountingDown = false; // Reset trigger state
    }

    private void TeleportXROrigin()
    {
        if (xrOrigin != null)
        {
            xrOrigin.position = teleportLocation; // Set XR Origin's position directly in world space
            Debug.Log($"XR Origin teleported to {teleportLocation} (world coordinates).");
        }
        else
        {
            Debug.LogError("XR Origin object not assigned!");
        }
    }
}
