using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class GeneralFillManager : MonoBehaviour
{
    public float fillSpeed = 1.0f;         // Speed at which the fill level transitions
    public Transform box;                 // The "Box" object being moved
    public Transform[] triggers;          // Array of triggers (Trigger 1, 2, 3, 4)
    public GameObject[] lightCylinders;   // LightCylinder objects under each trigger

    private Renderer objectRenderer;      // Renderer for the object this script is attached to
    private float[] targetFillLevels;     // Target fill levels for each trigger
    private int activeTriggerIndex = -1;  // Index of the currently active trigger
    private float currentFillLevel = 0f;  // Current fill level (0 to 1)
    private bool isFilling = false;       // Is the object being filled?
    private float minY;                   // Minimum Y-coordinate of the object
    private float maxY;                   // Maximum Y-coordinate of the object

    void Start()
    {
        // Initialize target fill levels
        targetFillLevels = new float[] { 0.1f, 0.3f, 0.5f, 1.0f };

        // Get the renderer of the current object
        objectRenderer = GetComponent<Renderer>();

        // Ensure the object uses the correct shader
        if (!objectRenderer.material.HasProperty("_FillLevel") ||
            !objectRenderer.material.HasProperty("_MinY") ||
            !objectRenderer.material.HasProperty("_MaxY"))
        {
            Debug.LogError("Shader does not have required properties (_FillLevel, _MinY, _MaxY)!");
            return;
        }

        // Calculate _MinY and _MaxY for this object
        SetMinMaxY();

        // Initialize the material's properties
        objectRenderer.material.SetFloat("_FillLevel", 0f);

        // Deactivate all light cylinders initially
        foreach (var light in lightCylinders)
        {
            light.SetActive(false);
        }
    }

    void Update()
    {
        // Check which trigger the box is overlapping
        int triggerIndex = GetTriggerIndexUnderBox();

        if (triggerIndex != activeTriggerIndex)
        {
            // Update the active trigger and corresponding light cylinder
            SetActiveTrigger(triggerIndex);
        }

        // Handle fill level transitions
        if (isFilling && activeTriggerIndex >= 0)
        {
            currentFillLevel = Mathf.MoveTowards(
                currentFillLevel,
                targetFillLevels[activeTriggerIndex],
                fillSpeed * Time.deltaTime
            );

            // Update the shader's fill level
            objectRenderer.material.SetFloat("_FillLevel", currentFillLevel);

            if (Mathf.Approximately(currentFillLevel, targetFillLevels[activeTriggerIndex]))
            {
                isFilling = false;
            }
        }
    }

    void SetMinMaxY()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Bounds meshBounds = meshFilter.sharedMesh.bounds;

        // Calculate _MinY and _MaxY based on the object's local scale
        minY = meshBounds.min.y * transform.localScale.y;
        maxY = meshBounds.max.y * transform.localScale.y;

        // Set these values in the material
        objectRenderer.material.SetFloat("_MinY", minY);
        objectRenderer.material.SetFloat("_MaxY", maxY);

        Debug.Log($"{gameObject.name} -> MinY: {minY}, MaxY: {maxY}");
    }

    int GetTriggerIndexUnderBox()
    {
        // Check each trigger to see if the box is overlapping it
        for (int i = 0; i < triggers.Length; i++)
        {
            if (IsBoxOverlappingTrigger(box, triggers[i]))
            {
                return i;
            }
        }
        return -1; // No trigger is overlapped
    }

    bool IsBoxOverlappingTrigger(Transform box, Transform trigger)
    {
        // Get the 2D bounds of the box
        Vector2 boxMin = new Vector2(
            box.position.x - box.localScale.x / 2f,
            box.position.z - box.localScale.z / 2f
        );
        Vector2 boxMax = new Vector2(
            box.position.x + box.localScale.x / 2f,
            box.position.z + box.localScale.z / 2f
        );

        // Get the 2D bounds of the trigger
        Vector2 triggerMin = new Vector2(
            trigger.position.x - trigger.localScale.x / 2f,
            trigger.position.z - trigger.localScale.z / 2f
        );
        Vector2 triggerMax = new Vector2(
            trigger.position.x + trigger.localScale.x / 2f,
            trigger.position.z + trigger.localScale.z / 2f
        );

        // Check for overlap in 2D (ignoring height)
        return boxMin.x < triggerMax.x && boxMax.x > triggerMin.x &&
               boxMin.y < triggerMax.y && boxMax.y > triggerMin.y;
    }

    void SetActiveTrigger(int triggerIndex)
    {
        // Deactivate the previous light cylinder
        if (activeTriggerIndex >= 0)
        {
            lightCylinders[activeTriggerIndex].SetActive(false);
        }

        // Activate the new light cylinder
        if (triggerIndex >= 0)
        {
            lightCylinders[triggerIndex].SetActive(true);

            // Start filling the object for the new trigger
            activeTriggerIndex = triggerIndex;
            isFilling = true;

            Debug.Log($"Trigger {triggerIndex + 1} activated: Target fill level = {targetFillLevels[triggerIndex]}");
        }
        else
        {
            activeTriggerIndex = -1; // No trigger is active
        }
    }
}
