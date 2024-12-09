using UnityEngine;

public class CameraToggle : MonoBehaviour
{
    // Assign the cameras in the Inspector
    public Camera camera1;
    public Camera camera2;

    // This will track which camera is currently active
    private bool isCamera1Active = true;

    void Start()
    {
        // Ensure that one of the cameras is active at the start
        if (camera1 != null && camera2 != null)
        {
            camera1.gameObject.SetActive(true);
            camera2.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Cameras not assigned in the Inspector.");
        }
    }

    void Update()
    {
        // Toggle the cameras when the user presses the "T" key (you can change this to another key if needed)
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleCameras();
        }
    }

    void ToggleCameras()
    {
        // Switch the active camera based on the current state
        if (isCamera1Active)
        {
            camera1.gameObject.SetActive(false);
            camera2.gameObject.SetActive(true);
        }
        else
        {
            camera1.gameObject.SetActive(true);
            camera2.gameObject.SetActive(false);
        }

        // Toggle the state flag to keep track of which camera is active
        isCamera1Active = !isCamera1Active;
    }
}
