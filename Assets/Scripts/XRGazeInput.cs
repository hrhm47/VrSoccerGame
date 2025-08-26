// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.EventSystems;
// using System.Collections.Generic;

// public class XRGazeInput : MonoBehaviour
// {
//     [Header("XR Setup")]
//     public Camera xrCamera;
//     public Canvas worldCanvas;
    
//     [Header("Gaze Settings")]
//     public float gazeTime = 2f;
//     public float rayDistance = 50f;
    
//     [Header("Audio")]
//     public AudioClip hoverSound;
//     public AudioClip clickSound;
//     private AudioSource audioSource;
    
//     // UI Components
//     private GraphicRaycaster graphicRaycaster;
//     private EventSystem eventSystem;
    
//     // Gaze tracking
//     private Button currentButton;
//     private float gazeTimer = 0f;
//     private bool isGazing = false;
    
//     // Visual feedback
//     private GameObject gazeReticle;
//     private MeshRenderer reticleRenderer;
    
//     void Start()
//     {
//         SetupGazeSystem();
//     }
    
//     void SetupGazeSystem()
//     {
//         // Find XR camera - look specifically in XRRig structure
//         if (xrCamera == null)
//         {
//             // Try to find camera in XRRig structure
//             GameObject xrRig = GameObject.Find("XRRig");
//             if (xrRig != null)
//             {
//                 Transform cameraOffset = xrRig.transform.Find("CameraOffset");
//                 if (cameraOffset != null)
//                 {
//                     Transform mainCamera = cameraOffset.Find("MainCamera");
//                     if (mainCamera != null)
//                     {
//                         xrCamera = mainCamera.GetComponent<Camera>();
//                     }
//                 }
//             }
            
//             // Fallback to Camera.main
//             if (xrCamera == null)
//                 xrCamera = Camera.main;
//         }
        
//         if (xrCamera == null)
//         {
//             Debug.LogError("XR Camera not found! Please assign it manually.");
//             return;
//         }
        
//         // Find Canvas - look for MainMenuCanvas specifically
//         if (worldCanvas == null)
//         {
//             GameObject canvasObj = GameObject.Find("MainMenuCanvas");
//             if (canvasObj != null)
//             {
//                 worldCanvas = canvasObj.GetComponent<Canvas>();
//             }
//             else
//             {
//                 worldCanvas = FindObjectOfType<Canvas>();
//             }
//         }
        
//         if (worldCanvas == null)
//         {
//             Debug.LogError("World Canvas not found! Please assign it manually.");
//             return;
//         }
        
//         // CRITICAL: Ensure canvas is set to World Space
//         if (worldCanvas.renderMode != RenderMode.WorldSpace)
//         {
//             Debug.LogWarning("Canvas is not in World Space! Converting to World Space for VR.");
//             worldCanvas.renderMode = RenderMode.WorldSpace;
            
//             // Position canvas in front of camera
//             worldCanvas.transform.position = xrCamera.transform.position + xrCamera.transform.forward * 3f;
//             worldCanvas.transform.rotation = xrCamera.transform.rotation;
//             worldCanvas.transform.localScale = Vector3.one * 0.01f; // Scale down for world space
//         }
        
//         // Setup EventSystem
//         eventSystem = FindObjectOfType<EventSystem>();
//         if (eventSystem == null)
//         {
//             Debug.LogError("EventSystem not found! Please add one to the scene.");
//             return;
//         }
        
//         // Setup GraphicRaycaster
//         graphicRaycaster = worldCanvas.GetComponent<GraphicRaycaster>();
//         if (graphicRaycaster == null)
//         {
//             graphicRaycaster = worldCanvas.gameObject.AddComponent<GraphicRaycaster>();
//         }
        
//         // Add colliders to all buttons for World Space interaction
//         AddCollidersToButtons();
        
//         // Setup audio
//         audioSource = GetComponent<AudioSource>();
//         if (audioSource == null)
//             audioSource = gameObject.AddComponent<AudioSource>();
        
//         // Create gaze reticle
//         CreateGazeReticle();
        
//         Debug.Log($"XR Gaze System Setup Complete:");
//         Debug.Log($"- Camera: {xrCamera.name}");
//         Debug.Log($"- Canvas: {worldCanvas.name} (Mode: {worldCanvas.renderMode})");
//         Debug.Log($"- EventSystem: {eventSystem.name}");
//     }
    
//     void AddCollidersToButtons()
//     {
//         // Find all buttons in the canvas and add colliders
//         Button[] buttons = worldCanvas.GetComponentsInChildren<Button>(true);
        
//         foreach (Button button in buttons)
//         {
//             // Add BoxCollider if it doesn't exist
//             BoxCollider collider = button.GetComponent<BoxCollider>();
//             if (collider == null)
//             {
//                 collider = button.gameObject.AddComponent<BoxCollider>();
                
//                 // Set collider size based on RectTransform
//                 RectTransform rectTransform = button.GetComponent<RectTransform>();
//                 if (rectTransform != null)
//                 {
//                     collider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 1f);
//                 }
//             }
            
//             Debug.Log($"Added collider to button: {button.name}");
//         }
//     }
    
//     void CreateGazeReticle()
//     {
//         // Create reticle as child of camera
//         gazeReticle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//         gazeReticle.name = "GazeReticle";
//         gazeReticle.transform.SetParent(xrCamera.transform);
//         gazeReticle.transform.localPosition = new Vector3(0, 0, 2f);
//         gazeReticle.transform.localScale = Vector3.one * 0.02f;
        
//         // Remove collider to prevent interference
//         Destroy(gazeReticle.GetComponent<Collider>());
        
//         // Setup material
//         reticleRenderer = gazeReticle.GetComponent<MeshRenderer>();
//         Material reticleMaterial = new Material(Shader.Find("Unlit/Color"));
//         reticleMaterial.color = Color.white;
//         reticleRenderer.material = reticleMaterial;
        
//         Debug.Log("Gaze reticle created");
//     }
    
//     void Update()
//     {
//         if (xrCamera == null || worldCanvas == null || graphicRaycaster == null)
//         {
//             return;
//         }
        
//         PerformGazeRaycast();
//         UpdateGazeTimer();
//         UpdateReticleVisual();
//     }
    
//     void PerformGazeRaycast()
//     {
//         // Cast ray from camera center forward
//         Ray gazeRay = new Ray(xrCamera.transform.position, xrCamera.transform.forward);
        
//         // Debug ray (red line in scene view)
//         Debug.DrawRay(gazeRay.origin, gazeRay.direction * rayDistance, Color.red, 0.1f);
        
//         Button detectedButton = null;
        
//         // Method 1: Physics Raycast for World Space UI (Primary method)
//         RaycastHit hit;
//         if (Physics.Raycast(gazeRay, out hit, rayDistance))
//         {
//             Debug.Log($"Hit object: {hit.collider.name}");
            
//             // Try to get button from hit object
//             detectedButton = hit.collider.GetComponent<Button>();
            
//             // If not found, try parent objects
//             if (detectedButton == null)
//             {
//                 detectedButton = hit.collider.GetComponentInParent<Button>();
//             }
            
//             if (detectedButton != null)
//             {
//                 Debug.Log($"Found button via Physics: {detectedButton.name}");
//             }
//         }
        
//         // Method 2: UI Raycast (Fallback for complex UI)
//         if (detectedButton == null)
//         {
//             detectedButton = RaycastUI();
//         }
        
//         // Handle button change
//         if (detectedButton != currentButton)
//         {
//             OnGazeExit();
//             if (detectedButton != null && detectedButton.interactable)
//             {
//                 OnGazeEnter(detectedButton);
//             }
//         }
//     }
    
//     Button RaycastUI()
//     {
//         if (eventSystem == null || graphicRaycaster == null) return null;
        
//         // Create pointer data for screen center
//         PointerEventData pointerEventData = new PointerEventData(eventSystem);
//         pointerEventData.position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        
//         // Perform UI raycast
//         List<RaycastResult> raycastResults = new List<RaycastResult>();
//         graphicRaycaster.Raycast(pointerEventData, raycastResults);
        
//         // Find first interactable button
//         foreach (var result in raycastResults)
//         {
//             Button button = result.gameObject.GetComponent<Button>();
//             if (button != null && button.interactable)
//             {
//                 Debug.Log($"Found button via UI Raycast: {button.name}");
//                 return button;
//             }
//         }
        
//         return null;
//     }
    
//     void OnGazeEnter(Button button)
//     {
//         currentButton = button;
//         isGazing = true;
//         gazeTimer = 0f;
        
//         // Audio feedback
//         if (hoverSound != null && audioSource != null)
//             audioSource.PlayOneShot(hoverSound);
        
//         Debug.Log($"Started gazing at: {button.name}");
//     }
    
//     void OnGazeExit()
//     {
//         if (currentButton != null)
//         {
//             Debug.Log($"Stopped gazing at: {currentButton.name}");
//         }
        
//         currentButton = null;
//         isGazing = false;
//         gazeTimer = 0f;
//     }
    
//     void UpdateGazeTimer()
//     {
//         if (!isGazing || currentButton == null) return;
        
//         gazeTimer += Time.deltaTime;
        
//         // Show progress in console
//         float progress = (gazeTimer / gazeTime) * 100f;
//         Debug.Log($"Gaze progress: {progress:F0}% ({gazeTimer:F1}s/{gazeTime:F1}s)");
        
//         if (gazeTimer >= gazeTime)
//         {
//             ExecuteGazeClick();
//         }
//     }
    
//     void ExecuteGazeClick()
//     {
//         if (currentButton == null || !currentButton.interactable) return;
        
//         Debug.Log($"GAZE CLICK EXECUTED: {currentButton.name}");
        
//         // Audio feedback
//         if (clickSound != null && audioSource != null)
//             audioSource.PlayOneShot(clickSound);
        
//         // Execute button click
//         currentButton.onClick.Invoke();
        
//         // Reset gaze
//         OnGazeExit();
//     }
    
//     void UpdateReticleVisual()
//     {
//         if (reticleRenderer == null) return;
        
//         if (isGazing && currentButton != null)
//         {
//             // Show progress with color change
//             float progress = gazeTimer / gazeTime;
//             reticleRenderer.material.color = Color.Lerp(Color.yellow, Color.green, progress);
            
//             // Scale animation
//             float scale = 0.02f + (progress * 0.01f);
//             gazeReticle.transform.localScale = Vector3.one * scale;
//         }
//         else if (currentButton != null)
//         {
//             // Hovering over button
//             reticleRenderer.material.color = Color.yellow;
//             gazeReticle.transform.localScale = Vector3.one * 0.025f;
//         }
//         else
//         {
//             // Normal state
//             reticleRenderer.material.color = Color.white;
//             gazeReticle.transform.localScale = Vector3.one * 0.02f;
//         }
//     }
    
//     // Public method to test gaze system
//     public void TestGazeSystem()
//     {
//         Debug.Log("=== GAZE SYSTEM TEST ===");
//         Debug.Log($"XR Camera: {(xrCamera != null ? xrCamera.name : "NULL")}");
//         Debug.Log($"World Canvas: {(worldCanvas != null ? worldCanvas.name : "NULL")}");
//         Debug.Log($"Canvas Render Mode: {(worldCanvas != null ? worldCanvas.renderMode.ToString() : "NULL")}");
//         Debug.Log($"Graphic Raycaster: {(graphicRaycaster != null ? "Present" : "NULL")}");
//         Debug.Log($"Event System: {(eventSystem != null ? eventSystem.name : "NULL")}");
        
//         // Count buttons with colliders
//         if (worldCanvas != null)
//         {
//             Button[] buttons = worldCanvas.GetComponentsInChildren<Button>(true);
//             int buttonsWithColliders = 0;
//             foreach (Button btn in buttons)
//             {
//                 if (btn.GetComponent<Collider>() != null)
//                     buttonsWithColliders++;
//             }
//             Debug.Log($"Buttons found: {buttons.Length}, With colliders: {buttonsWithColliders}");
//         }
//     }
// }





using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class XRGazeInput : MonoBehaviour
{
    [Header("XR Setup")]
    public Camera xrCamera;
    public Canvas worldCanvas;
    
    [Header("Gaze Settings")]
    public float gazeTime = 2f;
    public float rayDistance = 50f;
    
    [Header("Audio")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    private AudioSource audioSource;
    
    // UI Components
    private GraphicRaycaster graphicRaycaster;
    private EventSystem eventSystem;
    
    // Gaze tracking
    private Button currentButton;
    private TMP_InputField currentInputField;
    private VRInputFieldDetector currentInputDetector;
    private float gazeTimer = 0f;
    private bool isGazing = false;
    private System.Type currentTargetType;
    
    // Visual feedback
    private GameObject gazeReticle;
    private MeshRenderer reticleRenderer;
    
    void Start()
    {
        SetupGazeSystem();
    }
    
    void SetupGazeSystem()
    {
        // Find XR camera
        if (xrCamera == null)
        {
            GameObject xrRig = GameObject.Find("XRRig");
            if (xrRig != null)
            {
                Transform cameraOffset = xrRig.transform.Find("CameraOffset");
                if (cameraOffset != null)
                {
                    Transform mainCamera = cameraOffset.Find("MainCamera");
                    if (mainCamera != null)
                    {
                        xrCamera = mainCamera.GetComponent<Camera>();
                    }
                }
            }
            
            if (xrCamera == null)
                xrCamera = Camera.main;
        }
        
        if (xrCamera == null)
        {
            Debug.LogError("XR Camera not found!");
            return;
        }
        
        // Find Canvas
        if (worldCanvas == null)
        {
            GameObject canvasObj = GameObject.Find("MainMenuCanvas");
            if (canvasObj != null)
            {
                worldCanvas = canvasObj.GetComponent<Canvas>();
            }
            else
            {
                worldCanvas = FindObjectOfType<Canvas>();
            }
        }
        
        if (worldCanvas == null)
        {
            Debug.LogError("World Canvas not found!");
            return;
        }
        
        // Setup EventSystem
        eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("EventSystem not found!");
            return;
        }
        
        // Setup GraphicRaycaster
        graphicRaycaster = worldCanvas.GetComponent<GraphicRaycaster>();
        if (graphicRaycaster == null)
        {
            graphicRaycaster = worldCanvas.gameObject.AddComponent<GraphicRaycaster>();
        }
        
        // Add colliders to all interactive elements
        AddCollidersToInteractiveElements();
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Create gaze reticle
        CreateGazeReticle();
        
        Debug.Log("XR Gaze System Setup Complete with Input Field Support");
    }
    
    void AddCollidersToInteractiveElements()
    {
        // Add colliders to buttons
        Button[] buttons = worldCanvas.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            AddColliderToElement(button.gameObject, button.GetComponent<RectTransform>());
        }
        
        // Add colliders to input fields
        TMP_InputField[] inputFields = worldCanvas.GetComponentsInChildren<TMP_InputField>(true);
        foreach (TMP_InputField inputField in inputFields)
        {
            AddColliderToElement(inputField.gameObject, inputField.GetComponent<RectTransform>());
        }
        
        Debug.Log($"Added colliders to {buttons.Length} buttons and {inputFields.Length} input fields");
    }
    
    void AddColliderToElement(GameObject element, RectTransform rectTransform)
    {
        BoxCollider collider = element.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = element.AddComponent<BoxCollider>();
        }
        
        if (rectTransform != null)
        {
            collider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 50f);
        }
    }
    
    void CreateGazeReticle()
    {
        gazeReticle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gazeReticle.name = "GazeReticle";
        gazeReticle.transform.SetParent(xrCamera.transform);
        gazeReticle.transform.localPosition = new Vector3(0, 0, 2f);
        gazeReticle.transform.localScale = Vector3.one * 0.02f;
        
        Destroy(gazeReticle.GetComponent<Collider>());
        
        reticleRenderer = gazeReticle.GetComponent<MeshRenderer>();
        Material reticleMaterial = new Material(Shader.Find("Unlit/Color"));
        reticleMaterial.color = Color.white;
        reticleRenderer.material = reticleMaterial;
    }
    
    void Update()
    {
        if (xrCamera == null || worldCanvas == null || graphicRaycaster == null)
            return;
        
        PerformGazeRaycast();
        UpdateGazeTimer();
        UpdateReticleVisual();
    }
    
    void PerformGazeRaycast()
    {
        Ray gazeRay = new Ray(xrCamera.transform.position, xrCamera.transform.forward);
        Debug.DrawRay(gazeRay.origin, gazeRay.direction * rayDistance, Color.red, 0.1f);
        
        Button detectedButton = null;
        TMP_InputField detectedInputField = null;
        VRInputFieldDetector detectedInputDetector = null;
        
        // Physics Raycast
        RaycastHit hit;
        if (Physics.Raycast(gazeRay, out hit, rayDistance))
        {
            // Check for button
            detectedButton = hit.collider.GetComponent<Button>();
            if (detectedButton == null)
                detectedButton = hit.collider.GetComponentInParent<Button>();
            
            // Check for input field
            if (detectedButton == null)
            {
                detectedInputField = hit.collider.GetComponent<TMP_InputField>();
                if (detectedInputField == null)
                    detectedInputField = hit.collider.GetComponentInParent<TMP_InputField>();
                
                // Check for input field detector
                if (detectedInputField != null)
                {
                    detectedInputDetector = hit.collider.GetComponent<VRInputFieldDetector>();
                    if (detectedInputDetector == null)
                        detectedInputDetector = hit.collider.GetComponentInParent<VRInputFieldDetector>();
                }
            }
        }
        
        // Handle target change
        bool targetChanged = false;
        
        if (detectedButton != currentButton)
        {
            targetChanged = true;
            OnGazeExit();
            if (detectedButton != null && detectedButton.interactable)
            {
                OnGazeEnterButton(detectedButton);
            }
        }
        else if (detectedInputField != currentInputField)
        {
            targetChanged = true;
            OnGazeExit();
            if (detectedInputField != null && detectedInputField.interactable)
            {
                OnGazeEnterInputField(detectedInputField, detectedInputDetector);
            }
        }
    }
    
    void OnGazeEnterButton(Button button)
    {
        currentButton = button;
        currentInputField = null;
        currentInputDetector = null;
        currentTargetType = typeof(Button);
        isGazing = true;
        gazeTimer = 0f;
        
        if (hoverSound != null && audioSource != null)
            audioSource.PlayOneShot(hoverSound);
        
        Debug.Log($"Started gazing at button: {button.name}");
    }
    
    void OnGazeEnterInputField(TMP_InputField inputField, VRInputFieldDetector detector)
    {
        currentButton = null;
        currentInputField = inputField;
        currentInputDetector = detector;
        currentTargetType = typeof(TMP_InputField);
        isGazing = true;
        gazeTimer = 0f;
        
        if (hoverSound != null && audioSource != null)
            audioSource.PlayOneShot(hoverSound);
        
        Debug.Log($"Started gazing at input field: {inputField.name}");
    }
    
    void OnGazeExit()
    {
        if (currentButton != null)
        {
            Debug.Log($"Stopped gazing at button: {currentButton.name}");
        }
        else if (currentInputField != null)
        {
            Debug.Log($"Stopped gazing at input field: {currentInputField.name}");
        }
        
        currentButton = null;
        currentInputField = null;
        currentInputDetector = null;
        currentTargetType = null;
        isGazing = false;
        gazeTimer = 0f;
    }
    
    void UpdateGazeTimer()
    {
        if (!isGazing) return;
        
        gazeTimer += Time.deltaTime;
        
        float progress = (gazeTimer / gazeTime) * 100f;
        Debug.Log($"Gaze progress: {progress:F0}%");
        
        if (gazeTimer >= gazeTime)
        {
            ExecuteGazeClick();
        }
    }
    
    void ExecuteGazeClick()
    {
        if (currentTargetType == typeof(Button) && currentButton != null && currentButton.interactable)
        {
            Debug.Log($"GAZE CLICK - Button: {currentButton.name}");
            
            if (clickSound != null && audioSource != null)
                audioSource.PlayOneShot(clickSound);
            
            currentButton.onClick.Invoke();
        }
        else if (currentTargetType == typeof(TMP_InputField) && currentInputField != null && currentInputField.interactable)
        {
            Debug.Log($"GAZE CLICK - Input Field: {currentInputField.name}");
            
            if (clickSound != null && audioSource != null)
                audioSource.PlayOneShot(clickSound);
            
            // Trigger keyboard opening
            if (currentInputDetector != null)
            {
                currentInputDetector.OnGazeClick();
            }
            // else
            // {
            //     // Fallback: try to find keyboard manager
            //     VRKeyboardManager keyboardManager = FindObjectOfType<VRKeyboardManager>();
            //     if (keyboardManager != null)
            //     {
            //         keyboardManager.ShowKeyboard(currentInputField);
            //     }
            // }
        }
        
        OnGazeExit();
    }
    
    void UpdateReticleVisual()
    {
        if (reticleRenderer == null) return;
        
        if (isGazing && (currentButton != null || currentInputField != null))
        {
            float progress = gazeTimer / gazeTime;
            reticleRenderer.material.color = Color.Lerp(Color.yellow, Color.green, progress);
            
            float scale = 0.02f + (progress * 0.01f);
            gazeReticle.transform.localScale = Vector3.one * scale;
        }
        else if (currentButton != null || currentInputField != null)
        {
            reticleRenderer.material.color = Color.yellow;
            gazeReticle.transform.localScale = Vector3.one * 0.025f;
        }
        else
        {
            reticleRenderer.material.color = Color.white;
            gazeReticle.transform.localScale = Vector3.one * 0.02f;
        }
    }
}