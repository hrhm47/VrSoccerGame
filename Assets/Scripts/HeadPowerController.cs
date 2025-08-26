using UnityEngine;
using UnityEngine.UI;

public class HeadPowerController : MonoBehaviour
{
    [Header("Settings")]
    public Camera vrCamera;
    public float minTiltAngle = -30f;
    public float maxTiltAngle = 60f;
    
    private float currentPower = 50f;
    
    void Start()
    {
        if (vrCamera == null) vrCamera = Camera.main;
        enabled = false;
    }
    
    void Update()
    {
        if (vrCamera == null) return;
        
        float tilt = vrCamera.transform.eulerAngles.x;
        if (tilt > 180f) tilt -= 360f;
        
        float normalizedTilt = Mathf.InverseLerp(minTiltAngle, maxTiltAngle, tilt);
        currentPower = Mathf.Lerp(20f, 100f, normalizedTilt);
        
        UpdatePowerMeter();
    }
    
    void UpdatePowerMeter()
    {
        Slider powerMeter = GameObject.Find("PowerMeter")?.GetComponent<Slider>();
        if (powerMeter != null)
        {
            powerMeter.value = currentPower;
        }
    }
    
    public float GetCurrentPower()
    {
        return currentPower;
    }
}