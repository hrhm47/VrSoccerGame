using UnityEngine;
using UnityEngine.UI;

public class VRUIManager : MonoBehaviour
{
    [Header("UI Positioning")]
    public Canvas mainCanvas;
    public Camera vrCamera;
    
    [Header("Distance Settings")]
    public float uiDistance = 5f; // Distance from camera
    public float uiScale = 0.003f; // Scale for readability
    public float heightOffset = 0f; // Vertical offset
    
    [Header("Auto-Adjust Settings")]
    public bool autoAdjustOnStart = true;
    public bool followCameraRotation = false;
    
    void Start()
    {
        if (autoAdjustOnStart)
        {
            SetupOptimalUIPosition();
        }
    }
    
    public void SetupOptimalUIPosition()
    {
        if (mainCanvas == null)
        {
            mainCanvas = FindObjectOfType<Canvas>();
        }
        
        if (vrCamera == null)
        {
            // Find camera in XRRig structure
            GameObject xrRig = GameObject.Find("XRRig");
            if (xrRig != null)
            {
                Transform cameraOffset = xrRig.transform.Find("CameraOffset");
                if (cameraOffset != null)
                {
                    Transform mainCam = cameraOffset.Find("MainCamera");
                    if (mainCam != null)
                    {
                        vrCamera = mainCam.GetComponent<Camera>();
                    }
                }
            }
        }
        
        if (mainCanvas != null && vrCamera != null)
        {
            // Set canvas to World Space
            mainCanvas.renderMode = RenderMode.WorldSpace;
            
            // Position canvas in front of camera
            Vector3 cameraForward = vrCamera.transform.forward;
            Vector3 cameraPosition = vrCamera.transform.position;
            
            // Calculate optimal position
            Vector3 targetPosition = cameraPosition + (cameraForward * uiDistance);
            targetPosition.y += heightOffset;
            
            mainCanvas.transform.position = targetPosition;
            
            // Set rotation to face camera
            if (followCameraRotation)
            {
                mainCanvas.transform.rotation = vrCamera.transform.rotation;
            }
            else
            {
                mainCanvas.transform.LookAt(vrCamera.transform);
                mainCanvas.transform.Rotate(0, 180, 0); // Face camera correctly
            }
            
            // Set optimal scale
            mainCanvas.transform.localScale = Vector3.one * uiScale;
            
            Debug.Log($"UI positioned at distance: {uiDistance}m, scale: {uiScale}");
        }
        
        // Update colliders after repositioning
        UpdateButtonColliders();
    }
    
    void UpdateButtonColliders()
    {
        Button[] buttons = mainCanvas.GetComponentsInChildren<Button>(true);
        
        foreach (Button button in buttons)
        {
            BoxCollider collider = button.GetComponent<BoxCollider>();
            if (collider != null)
            {
                RectTransform rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Scale collider size appropriately
                    Vector2 size = rectTransform.rect.size;
                    collider.size = new Vector3(size.x, size.y, 50f);
                }
            }
        }
    }
    
    // Call this method to test different distances
    public void TestUIDistance(float newDistance)
    {
        uiDistance = newDistance;
        SetupOptimalUIPosition();
    }
    
    // Call this method to test different scales
    public void TestUIScale(float newScale)
    {
        uiScale = newScale;
        SetupOptimalUIPosition();
    }
    
    void Update()
    {
        // Optional: Make UI always face camera
        if (followCameraRotation && mainCanvas != null && vrCamera != null)
        {
            mainCanvas.transform.LookAt(vrCamera.transform);
            mainCanvas.transform.Rotate(0, 180, 0);
        }
    }
}