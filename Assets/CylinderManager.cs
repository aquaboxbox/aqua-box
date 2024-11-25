using UnityEngine;

public class CylinderManager : MonoBehaviour
{
    public Renderer cylinderRenderer;     // Renderer for the cylinder's material
    public float fillSpeed = 1.0f;        // Speed at which the water level rises
    public float cycleInterval = 2.0f;   // Time interval between trigger activations

    private int currentTriggerIndex = 0;  // Index of the active trigger
    private float[] targetFillLevels;     // Fill levels for each trigger
    private float currentFillLevel = 0f;  // Current water fill level (0 to 1)
    private bool isFilling = false;       // Is the water level increasing?
    private float timer = 0f;             // Timer for cycling triggers

    void Start()
    {
        // Initialize fill levels for triggers (normalized between 0 and 1)
        targetFillLevels = new float[] { 0.25f, 0.5f, 0.75f, 1.0f };

        // Ensure the cylinder has a material and starts empty
        if (cylinderRenderer != null)
        {
            // Check if the material is properly assigned
            if (cylinderRenderer.material == null)
            {
                Debug.LogError("Cylinder Material is missing! Assign it in the Inspector.");
                return;
            }

            // Ensure the custom shader is applied
            Shader customShader = Shader.Find("Custom/BarrelFillShader");
            if (customShader == null)
            {
                Debug.LogError("Custom shader 'BarrelFillShader' not found!");
                return;
            }
            cylinderRenderer.material.shader = customShader;

            // Initialize the fill level
            cylinderRenderer.material.SetFloat("_FillLevel", 0f);
        }

        // Trigger the first fill level change
        ActivateNextTrigger();
    }

    void Update()
    {
        if (cylinderRenderer == null)
        {
            Debug.LogError("Cylinder Renderer is not assigned! Please assign it in the Inspector.");
            return;
        }

        if (isFilling)
        {
            currentFillLevel = Mathf.MoveTowards(currentFillLevel, targetFillLevels[currentTriggerIndex], fillSpeed * Time.deltaTime);

            cylinderRenderer.material.SetFloat("_FillLevel", currentFillLevel);

            if (Mathf.Approximately(currentFillLevel, targetFillLevels[currentTriggerIndex]))
            {
                isFilling = false;
            }
        }

        // Cycle triggers after the interval
        timer += Time.deltaTime;
        if (timer >= cycleInterval)
        {
            timer = 0f;
            ActivateNextTrigger();
        }
    }

    void ActivateNextTrigger()
    {
        // Move to the next target fill level
        currentTriggerIndex = (currentTriggerIndex + 1) % targetFillLevels.Length;

        isFilling = true;

        Debug.Log($"Trigger {currentTriggerIndex + 1} activated: Target fill level = {targetFillLevels[currentTriggerIndex]}");
    }
}
