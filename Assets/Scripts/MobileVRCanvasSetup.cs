using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class MobileVRCanvasSetup : MonoBehaviour
{
    [Header("Mobile VR UI Settings")]
    public Camera vrCamera;
    public float uiDistance = 3f;
    public float uiHeight = 1.5f;
    public float mobileScale = 0.01f;
    
    [Header("Auto-Setup")]
    public bool autoSetupOnStart = true;
    public bool makeUIStaticInWorld = true;
    public bool optimizeForMobile = true;
    
    private Canvas canvas;
    private bool isSetup = false;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupMobileVRCanvas();
        }
    }
    
    public void SetupMobileVRCanvas()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ No Canvas component found!");
            return;
        }
        
        // Find camera if not assigned
        if (vrCamera == null)
        {
            vrCamera = Camera.main;
            if (vrCamera == null)
            {
                vrCamera = FindObjectOfType<Camera>();
            }
        }
        
        if (vrCamera == null)
        {
            Debug.LogError("❌ No VR Camera found!");
            return;
        }
        
        // Setup canvas for mobile VR
        ConfigureCanvasForMobileVR();
        
        // Position UI in world space
        PositionUIForMobileVR();
        
        // Optimize for mobile performance
        if (optimizeForMobile)
        {
            OptimizeForMobile();
        }
        
        isSetup = true;
        Debug.Log("✅ Mobile VR Canvas setup complete!");
    }
    
    void ConfigureCanvasForMobileVR()
    {
        // Remove Canvas Scaler if it exists (causes issues in World Space)
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            DestroyImmediate(scaler);
            Debug.Log("🗑️ Removed Canvas Scaler for mobile VR");
        }
        
        // Configure canvas
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = vrCamera;
        
        // Set appropriate scale for mobile
        transform.localScale = Vector3.one * mobileScale;
        
        Debug.Log("📱 Canvas configured for mobile VR");
    }
    
    void PositionUIForMobileVR()
    {
        if (makeUIStaticInWorld)
        {
            // Position UI at fixed world location
            Vector3 cameraForward = vrCamera.transform.forward;
            cameraForward.y = 0f; // Keep UI level
            cameraForward.Normalize();
            
            Vector3 uiPosition = vrCamera.transform.position + 
                               cameraForward * uiDistance;
            uiPosition.y = uiHeight;
            
            transform.position = uiPosition;
            
            // Face the camera initially
            Vector3 lookDirection = vrCamera.transform.position - transform.position;
            lookDirection.y = 0f;
            transform.rotation = Quaternion.LookRotation(-lookDirection);
            
            Debug.Log($"📍 UI positioned at: {transform.position}");
        }
    }
    
    void OptimizeForMobile()
    {
        // Set canvas group settings for better performance
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Add GraphicRaycaster if missing
        GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = gameObject.AddComponent<GraphicRaycaster>();
        }
        
        // Optimize raycaster for mobile
        raycaster.ignoreReversedGraphics = true;
        raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.ThreeD;
        
        Debug.Log("🚀 Mobile optimizations applied");
    }
    
    void Update()
    {
        // Optional: Very slow following of player position (not rotation)
        if (isSetup && makeUIStaticInWorld && vrCamera != null)
        {
            // Uncomment below for subtle position following
            // FollowPlayerPositionSlowly();
        }
    }
    
    void FollowPlayerPositionSlowly()
    {
        Vector3 targetPosition = vrCamera.transform.position + 
                               vrCamera.transform.forward * uiDistance;
        targetPosition.y = uiHeight;
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 0.5f);
    }
    
    // Public methods for runtime configuration
    public void SetUIDistance(float newDistance)
    {
        uiDistance = newDistance;
        if (isSetup)
        {
            PositionUIForMobileVR();
        }
    }
    
    public void SetUIHeight(float newHeight)
    {
        uiHeight = newHeight;
        if (isSetup)
        {
            PositionUIForMobileVR();
        }
    }
    
    public void RecalibrateUI()
    {
        if (isSetup)
        {
            PositionUIForMobileVR();
            Debug.Log("🔄 UI recalibrated");
        }
    }
    
    [ContextMenu("Setup Mobile VR Canvas")]
    public void ManualSetup()
    {
        SetupMobileVRCanvas();
    }
}