using System.Collections;
using UnityEngine;
using TMPro; // For TextMeshPro support

public class CubeTriggerScript : MonoBehaviour
{
    public Canvas countdownCanvas; // Assign the Canvas in the Inspector
    public TextMeshProUGUI countdownText; // Assign the TextMeshPro text element in the Inspector
    public float countdownDuration = 3.0f; // Duration of the countdown in seconds

    private bool isCountingDown = false; // To prevent multiple triggers

    private void Start()
    {
        // Ensure the Canvas is hidden initially
        if (countdownCanvas != null)
        {
            countdownCanvas.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug message to confirm what entered the trigger
        Debug.Log($"Triggered by: {other.name} with tag: {other.tag}");

        // Check if the Main Camera enters the trigger zone
        if (other.CompareTag("MainCamera") && !isCountingDown)
        {
            Debug.Log("Main Camera detected. Starting countdown...");
            StartCoroutine(StartCountdown());
        }
    }

    private IEnumerator StartCountdown()
    {
        isCountingDown = true;

        // Show the Canvas
        if (countdownCanvas != null)
        {
            countdownCanvas.gameObject.SetActive(true);
        }

        float remainingTime = countdownDuration;

        // Countdown logic
        while (remainingTime > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.Ceil(remainingTime).ToString(); // Update the countdown text
            }
            Debug.Log($"Countdown: {remainingTime}");
            yield return new WaitForSeconds(1.0f); // Wait for 1 second
            remainingTime--;
        }

        // Final action when the countdown reaches 0
        if (countdownText != null)
        {
            countdownText.text = "0"; // Display "0"
        }

        Debug.Log("Countdown finished. Triggering event...");
        TriggerEvent(); // Call the custom event logic

        // Hide the Canvas
        if (countdownCanvas != null)
        {
            countdownCanvas.gameObject.SetActive(false);
        }

        isCountingDown = false; // Reset trigger state
    }

    private void TriggerEvent()
    {
        // Debug message to confirm the event was triggered
        Debug.Log("Event triggered!");
        // Add your custom logic here (e.g., opening a door, playing a sound)
    }
}
