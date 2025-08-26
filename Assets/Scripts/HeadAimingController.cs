using UnityEngine;
using UnityEngine.UI;

public class HeadAimingController : MonoBehaviour
{
    [Header("Settings")]
    public Camera vrCamera;
    public float sensitivity = 1f;
    public Vector2 aimRange = new Vector2(3f, 2f);
    
    private Vector3 baseDirection;
    private Vector3 currentAimDirection = Vector3.zero;
    
    void Start()
    {
        if (vrCamera == null) vrCamera = Camera.main;
        if (vrCamera != null) baseDirection = vrCamera.transform.forward;
        enabled = false; // Disabled until aiming phase
    }
    
    void Update()
    {
        if (vrCamera == null) return;
        
        Vector3 headDirection = vrCamera.transform.forward;
        Vector3 deltaRotation = headDirection - baseDirection;
        
        currentAimDirection.x = Mathf.Clamp(deltaRotation.x * sensitivity, -aimRange.x, aimRange.x);
        currentAimDirection.y = Mathf.Clamp(deltaRotation.y * sensitivity, -aimRange.y, aimRange.y);
        
        UpdateCrosshair();
    }
    
    void UpdateCrosshair()
    {
        Image crosshair = GameObject.Find("CrosshairImage")?.GetComponent<Image>();
        if (crosshair != null)
        {
            RectTransform rect = crosshair.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localPosition = new Vector3(currentAimDirection.x * 50f, currentAimDirection.y * 50f, 0);
            }
            
            float accuracy = Vector3.Distance(currentAimDirection, Vector3.zero) / aimRange.magnitude;
            crosshair.color = Color.Lerp(Color.red, Color.green, accuracy);
        }
    }
    
    public Vector3 GetAimDirection()
    {
        return currentAimDirection;
    }
}