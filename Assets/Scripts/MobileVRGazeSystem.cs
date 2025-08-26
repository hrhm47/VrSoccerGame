using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;

public class MobileVRGazeSystem : MonoBehaviour
{
    [Header("VR Camera Setup")]
    public Camera vrCamera;
    
    [Header("Crosshair Settings")]
    public float crosshairDistance = 2f;
    public float crosshairSize = 0.03f; // Bigger for mobile
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color gazeColor = new Color(1f, 0.5f, 0f);
    public Color clickColor = Color.green;
    
    [Header("Gaze Settings")]
    public float gazeTime = 1.0f; // Faster for kids
    public float raycastDistance = 50f;
    public LayerMask uiLayerMask = -1;
    
    [Header("Mobile VR Settings")]
    public bool enableXRHeadTracking = true;
    public bool enableGyroscopeTracking = true;
    public float gyroSensitivity = 1f;
    
    [Header("Audio")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    private AudioSource audioSource;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool showCrosshairInEditor = true;
    
    // Crosshair components
    private GameObject crosshairObject;
    private LineRenderer[] crosshairLines;
    private Canvas crosshairCanvas; // UI-based crosshair for better mobile support
    private Image crosshairImage;
    
    // Head tracking
    private bool isXRActive = false;
    private bool isGyroActive = false;
    private Quaternion gyroInitialRotation;
    private Quaternion cameraInitialRotation;
    
    // Gaze detection
    private Button currentButton;
    private float gazeTimer = 0f;
    private bool isGazing = false;
    
    void Start()
    {
        SetupMobileVRGaze();
    }
    
    void SetupMobileVRGaze()
    {
        // Find camera
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
            Debug.LogError("❌ No camera found!");
            return;
        }
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Initialize head tracking
        InitializeHeadTracking();
        
        // Create crosshair for mobile VR
        CreateMobileCrosshair();
        
        Debug.Log("✅ Mobile VR Gaze System initialized!");
    }
    
    void InitializeHeadTracking()
    {
        // Try to initialize XR first
        if (enableXRHeadTracking)
        {
            InitializeXRTracking();
        }
        
        // Initialize gyroscope as fallback
        if (enableGyroscopeTracking && SystemInfo.supportsGyroscope)
        {
            InitializeGyroscopeTracking();
        }
        
        Debug.Log($"📱 Head tracking: XR={isXRActive}, Gyro={isGyroActive}");
    }
    
    void InitializeXRTracking()
    {
        try
        {
            if (XRSettings.enabled && XRSettings.loadedDeviceName.Length > 0)
            {
                isXRActive = true;
                Debug.Log($"🥽 XR Active: {XRSettings.loadedDeviceName}");
            }
            else
            {
                Debug.Log("⚠️ XR not active, will use gyroscope");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ XR initialization failed: {e.Message}");
        }
    }
    
    void InitializeGyroscopeTracking()
    {
        try
        {
            if (SystemInfo.supportsGyroscope)
            {
                Input.gyro.enabled = true;
                gyroInitialRotation = Input.gyro.attitude;
                cameraInitialRotation = vrCamera.transform.rotation;
                isGyroActive = true;
                Debug.Log("📱 Gyroscope tracking enabled");
            }
            else
            {
                Debug.LogWarning("⚠️ Device doesn't support gyroscope");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Gyroscope initialization failed: {e.Message}");
        }
    }
    
    void CreateMobileCrosshair()
    {
        // Create UI-based crosshair for better mobile compatibility
        GameObject crosshairCanvasGO = new GameObject("CrosshairCanvas");
        crosshairCanvas = crosshairCanvasGO.AddComponent<Canvas>();
        crosshairCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        crosshairCanvas.sortingOrder = 1000; // Always on top
        
        // Add CanvasScaler for mobile
        CanvasScaler scaler = crosshairCanvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster
        crosshairCanvasGO.AddComponent<GraphicRaycaster>();
        
        // Create crosshair image
        GameObject crosshairImageGO = new GameObject("CrosshairImage");
        crosshairImageGO.transform.SetParent(crosshairCanvasGO.transform);
        
        crosshairImage = crosshairImageGO.AddComponent<Image>();
        crosshairImage.color = normalColor;
        
        // Create simple crosshair texture
        Texture2D crosshairTexture = CreateCrosshairTexture();
        crosshairImage.sprite = Sprite.Create(crosshairTexture, 
            new Rect(0, 0, crosshairTexture.width, crosshairTexture.height), 
            new Vector2(0.5f, 0.5f));
        
        // Position in center of screen
        RectTransform crosshairRect = crosshairImageGO.GetComponent<RectTransform>();
        crosshairRect.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairRect.anchoredPosition = Vector2.zero;
        crosshairRect.sizeDelta = new Vector2(40, 40); // Size in pixels
        
        Debug.Log("🎯 Mobile crosshair created (UI-based)");
    }
    
    Texture2D CreateCrosshairTexture()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        
        // Create crosshair pattern
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isHorizontalLine = (y == size / 2) && (x >= size / 4 && x <= 3 * size / 4);
                bool isVerticalLine = (x == size / 2) && (y >= size / 4 && y <= 3 * size / 4);
                bool isCenter = (Mathf.Abs(x - size / 2) <= 2 && Mathf.Abs(y - size / 2) <= 2);
                
                if (isHorizontalLine || isVerticalLine)
                {
                    pixels[y * size + x] = Color.white;
                }
                else if (isCenter)
                {
                    pixels[y * size + x] = Color.clear;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    void Update()
    {
        if (vrCamera == null) return;
        
        // Update head tracking
        UpdateHeadTracking();
        
        // Detect gaze target
        DetectGazeTarget();
        
        // Update crosshair visuals
        UpdateCrosshairVisuals();
        
        // Debug info
        if (showDebugInfo && Time.frameCount % 60 == 0)
        {
            DebugHeadTracking();
        }
    }
    
    void UpdateHeadTracking()
    {
        if (isXRActive)
        {
            // XR handles head tracking automatically
            return;
        }
        else if (isGyroActive)
        {
            // Update camera rotation based on gyroscope
            UpdateGyroscopeTracking();
        }
    }
    
    void UpdateGyroscopeTracking()
    {
        try
        {
            // Get gyroscope rotation
            Quaternion gyroRotation = Input.gyro.attitude;
            
            // Convert gyroscope coordinate system to Unity's
            Quaternion correctedGyro = new Quaternion(gyroRotation.x, gyroRotation.y, -gyroRotation.z, -gyroRotation.w);
            
            // Apply rotation to camera
            Quaternion targetRotation = cameraInitialRotation * Quaternion.Inverse(gyroInitialRotation) * correctedGyro;
            vrCamera.transform.rotation = Quaternion.Lerp(vrCamera.transform.rotation, targetRotation, Time.deltaTime * gyroSensitivity);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Gyroscope update failed: {e.Message}");
        }
    }
    
    void DetectGazeTarget()
    {
        // Cast ray from camera center
        Ray gazeRay = new Ray(vrCamera.transform.position, vrCamera.transform.forward);
        RaycastHit hit;
        
        // Debug ray
        Debug.DrawRay(gazeRay.origin, gazeRay.direction * raycastDistance, 
                     currentButton != null ? Color.green : Color.red);
        
        Button detectedButton = null;
        
        // Physics raycast for UI buttons
        if (Physics.Raycast(gazeRay, out hit, raycastDistance, uiLayerMask))
        {
            detectedButton = hit.collider.GetComponent<Button>();
            
            if (showDebugInfo && detectedButton != null)
            {
                Debug.Log($"👁️ Gaze hit: {detectedButton.name} at {hit.distance:F2}m");
            }
        }
        
        // Process gaze
        if (detectedButton != null && detectedButton.interactable)
        {
            if (currentButton != detectedButton)
            {
                StartGazing(detectedButton);
            }
            ContinueGazing(detectedButton);
        }
        else
        {
            StopGazing();
        }
    }
    
    void StartGazing(Button button)
    {
        StopGazing();
        
        currentButton = button;
        isGazing = true;
        gazeTimer = 0f;
        
        // Audio feedback
        if (hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
        
        // Haptic feedback
        if (SystemInfo.supportsVibration)
        {
            Handheld.Vibrate();
        }
        
        Debug.Log($"🎯 Started gazing at: {button.name}");
    }
    
    void ContinueGazing(Button button)
    {
        if (!isGazing) return;
        
        gazeTimer += Time.deltaTime;
        
        if (gazeTimer >= gazeTime)
        {
            ExecuteGazeClick(button);
        }
    }
    
    void StopGazing()
    {
        if (currentButton != null)
        {
            Debug.Log($"👋 Stopped gazing at: {currentButton.name}");
            currentButton = null;
        }
        
        isGazing = false;
        gazeTimer = 0f;
    }
    
    void ExecuteGazeClick(Button button)
    {
        if (button == null || !button.interactable) return;
        
        // Audio feedback
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        
        // Strong haptic
        if (SystemInfo.supportsVibration)
        {
            Handheld.Vibrate();
        }
        
        // Execute click
        button.onClick.Invoke();
        
        Debug.Log($"🎉 Gaze click: {button.name}");
        
        StopGazing();
    }
    
    void UpdateCrosshairVisuals()
    {
        if (crosshairImage == null) return;
        
        Color targetColor = normalColor;
        float scale = 1f;
        
        if (isGazing && currentButton != null)
        {
            float progress = Mathf.Clamp01(gazeTimer / gazeTime);
            targetColor = Color.Lerp(hoverColor, gazeColor, progress);
            scale = 1f + Mathf.Sin(Time.time * 8f) * 0.2f * progress;
        }
        else if (currentButton != null)
        {
            targetColor = hoverColor;
            scale = 1f + Mathf.Sin(Time.time * 4f) * 0.1f;
        }
        
        crosshairImage.color = targetColor;
        crosshairImage.transform.localScale = Vector3.one * scale;
    }
    
    void DebugHeadTracking()
    {
        Debug.Log($"📱 Camera rotation: {vrCamera.transform.rotation.eulerAngles}");
        Debug.Log($"📱 Camera position: {vrCamera.transform.position}");
        Debug.Log($"📱 Head tracking: XR={isXRActive}, Gyro={isGyroActive}");
        
        if (isGyroActive)
        {
            Debug.Log($"📱 Gyro attitude: {Input.gyro.attitude.eulerAngles}");
        }
    }
    
    // Public methods
    public void CalibrateGyroscope()
    {
        if (isGyroActive)
        {
            gyroInitialRotation = Input.gyro.attitude;
            cameraInitialRotation = vrCamera.transform.rotation;
            Debug.Log("📱 Gyroscope calibrated");
        }
    }
    
    public void SetGazeTime(float newTime)
    {
        gazeTime = Mathf.Clamp(newTime, 0.5f, 3f);
        Debug.Log($"⏱️ Gaze time: {gazeTime}s");
    }
    
    [ContextMenu("Test Gaze")]
    public void TestGaze()
    {
        if (currentButton != null)
        {
            Debug.Log($"🧪 Testing gaze on: {currentButton.name}");
            ExecuteGazeClick(currentButton);
        }
    }
    
    void OnDestroy()
    {
        if (crosshairCanvas != null)
        {
            DestroyImmediate(crosshairCanvas.gameObject);
        }
    }
}