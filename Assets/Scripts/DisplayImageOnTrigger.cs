using UnityEngine;

public class TriggerCylinderTransform : MonoBehaviour
{
    public Transform innerCylinder;   // The InnerCylinder to transform
    public float targetHeight = 1.0f; // Desired height when this trigger is activated
    public float expandSpeed = 1.0f;  // Speed of the height transformation

    private bool isActivated = false; // Ensure the trigger only activates once

    void Update()
    {
        if (isActivated && innerCylinder != null)
        {
            // Gradually adjust the cylinder's height
            Vector3 scale = innerCylinder.localScale;
            scale.y = Mathf.MoveTowards(scale.y, targetHeight, expandSpeed * Time.deltaTime);
            innerCylinder.localScale = scale;

            // Adjust the position to keep the bottom anchored
            Vector3 position = innerCylinder.localPosition;
            position.y = scale.y / 2.0f; // Keep the base anchored
            innerCylinder.localPosition = position;

            // Stop updates once the target height is reached
            if (Mathf.Approximately(scale.y, targetHeight))
            {
                isActivated = false; // Stop further updates
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Activate only when the player enters and hasn't already activated this trigger
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;

            // Change the cylinder's color to blue
            if (innerCylinder != null)
            {
                Renderer renderer = innerCylinder.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.blue; // Set the color to blue
                }
            }
        }
    }
}
