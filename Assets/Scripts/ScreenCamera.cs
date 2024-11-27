using UnityEngine;

public class ScreenCamera : MonoBehaviour
{
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private LayerMask targetLayers;
    private Camera securityCamera;

    void Start()
    {
        // Get or add camera component
        securityCamera = GetComponent<Camera>();
        if (securityCamera == null)
            securityCamera = gameObject.AddComponent<Camera>();

        // Setup camera properties
        securityCamera.clearFlags = CameraClearFlags.SolidColor;
        securityCamera.backgroundColor = new Color(0, 0, 0, 0);
        securityCamera.cullingMask = targetLayers;
        securityCamera.targetTexture = renderTexture;
        
        // Disable audio listener if present
        AudioListener audioListener = GetComponent<AudioListener>();
        if (audioListener != null)
            audioListener.enabled = false;
    }
}