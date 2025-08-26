// using UnityEngine;

// [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
// public class PlayerMovement : MonoBehaviour
// {
//     public Transform headCamera; // Assign your MainCamera here
//     public float walkSpeed = 4f;
//     public float sprintSpeed = 7f;
//     public float lookDownThreshold = 15.0f;

//     // newly added variables for kicking
//     public float kickStrength = 5f;
//     public float kickCooldown = 1f;

//     private float lastKickTime = -999f;

//     // ==
//     private Rigidbody rb;
//     private bool isGrounded;

//     // for head tiwist tracking
//     private bool isMoving = true;  // toggled on twist

//     public float twistSensitivity = 45f; // degrees
//     public float twistDetectionTime = 1.5f; // seconds

//     void Start()
//     {
//         rb = GetComponent<Rigidbody>();
//         rb.freezeRotation = true;

//         // Optional but helps for stability
//         rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
//     }

//     void Update()
//     {
//         if (headCamera == null) return;

//         // CheckGrounded();
//         CheckGrounded();
//         HandleMovement();
//         // DetectTwistToToggle();  // NEW

//         // if (isMoving)
//         // {
//         //     HandleMovement();   // EXISTING
//         // }
//     }
//     // void FixedUpdate()
//     // {


//     // }

//     void CheckGrounded()
//     {
//         // Raycast down to check if player is on the ground
//         Ray ray = new Ray(transform.position, Vector3.down);
//         isGrounded = Physics.Raycast(ray, 1.1f);
//     }

//     void HandleMovement()
//     {
//         if (!isGrounded) return; // Prevent mid-air movement

//         Vector3 forward = headCamera.forward;
//         forward.y = 0f;
//         forward.Normalize();

//         // Get camera pitch (x-axis rotation)
//         float pitch = headCamera.eulerAngles.x;

//         // Normalize angle to -180 to 180
//         if (pitch > 180f) pitch -= 360f;
//         // if (pitch >= lookDownThreshold && pitch < 90.0f)
//         // {
//         float speed = Mathf.Abs(pitch) > lookDownThreshold ? sprintSpeed : walkSpeed;

//         rb.MovePosition(rb.position + forward * speed * Time.fixedDeltaTime);

//         // }


//         // Decide speed based on look angle
//     }

//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Ball") && Time.time > lastKickTime + kickCooldown)
//         {
//             Rigidbody ballRb = other.GetComponent<Rigidbody>();
//             if (ballRb != null)
//             {
//                 Vector3 kickDir = headCamera.forward;
//                 kickDir.y = 0f; // Keep kick grounded

//                 ballRb.linearVelocity = Vector3.zero; // Reset old movement
//                 ballRb.AddForce(kickDir.normalized * kickStrength, ForceMode.Impulse);

//                 lastKickTime = Time.time;

//                 Debug.Log("Ball kicked toward look direction!");
//             }
//         }
//     }



//     private float twistAccumulated = 0f;
//     private float twistTimer = 0f;
//     private float previousYaw = 0f;

//     void DetectTwistToToggle()
//     {
//         // Step 1: Get head pitch (up/down)
//         float pitch = headCamera.eulerAngles.x;
//         pitch = (pitch > 180f) ? pitch - 360f : pitch;

//         // Step 2: Only allow twisting if looking down
//         if (pitch < 30f) // You can tune this
//         {
//             float currentYaw = headCamera.eulerAngles.y;
//             float yawDelta = Mathf.DeltaAngle(previousYaw, currentYaw); // This handles wrap-around at 360°

//             twistAccumulated += Mathf.Abs(yawDelta);
//             twistTimer += Time.fixedDeltaTime;

//             if (twistAccumulated > twistSensitivity)
//             {
//                 isMoving = !isMoving;
//                 Debug.Log("Twist triggered → Toggled movement: " + isMoving);
//                 twistAccumulated = 0f;
//                 twistTimer = 0f;
//             }

//             if (twistTimer > twistDetectionTime)
//             {
//                 // Reset if time is up
//                 twistAccumulated = 0f;
//                 twistTimer = 0f;
//                 isMoving = true; // Reset to moving state
//             }

//             previousYaw = currentYaw;
//         }
//         else
//         {
//             // Not looking down — reset
//             twistAccumulated = 0f;
//             twistTimer = 0f;
//             previousYaw = headCamera.eulerAngles.y;
//             isMoving = true; // Reset to moving state
//         }
//     }




// }







// using UnityEngine;
// using UnityEngine.UI; // For UI
// using TMPro; // If you use TextMeshPro

// [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
// public class PlayerMovement : MonoBehaviour
// {
//     public Transform headCamera;
//     public float walkSpeed = 4f;
//     public float sprintSpeed = 7f;
//     public float lookDownThreshold = 15.0f;

//     public float kickStrength = 5f;
//     public float kickCooldown = 1f;

//     public TextMeshProUGUI movementStatusText; // UI Text for ON/OFF indicator

//     private float lastKickTime = -999f;
//     private Rigidbody rb;
//     private bool isGrounded;

//     // Twist detection
//     public float twistSensitivity = 45f;
//     public float twistDetectionTime = 1.5f;
//     private bool isMoving = true;

//     private float twistAccumulated = 0f;
//     private float twistTimer = 0f;
//     private float previousYaw = 0f;

//     void Start()
//     {
//         rb = GetComponent<Rigidbody>();
//         rb.freezeRotation = true;
//         rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

//         // UpdateMovementStatusUI();
//     }

//     void Update()
//     {
//         if (headCamera == null) return;

//         if (movementStatusText != null)
//         {
//             movementStatusText.text = isMoving ? "Status: Moving" : "Status: Stopped";
//         }

//         CheckGrounded();
//         DetectTwistToToggle();
//     }

//     void FixedUpdate()
//     {
//         if (headCamera == null) return;

//         HandleMovement();
//     }

//     //     void LateUpdate()
//     // {
//     //     if (movementStatusText != null)
//     //     {
//     //         // Keep text always in front of the player
//     //         movementStatusText.transform.localPosition = new Vector3(0, -0.3f, 2f);
//     //         movementStatusText.transform.localRotation = Quaternion.identity; // Face forward
//     //     }
//     // }

//     void CheckGrounded()
//     {
//         Ray ray = new Ray(transform.position, Vector3.down);
//         isGrounded = Physics.Raycast(ray, 1.1f);
//     }

//     void HandleMovement()
//     {
//         if (!isGrounded || !isMoving) return;

//         Vector3 forward = Vector3.ProjectOnPlane(headCamera.forward, Vector3.up).normalized;

//         float pitch = headCamera.eulerAngles.x;
//         if (pitch > 180f) pitch -= 360f;

//         float speed = pitch > lookDownThreshold ? sprintSpeed : walkSpeed;

//         rb.MovePosition(rb.position + forward * speed * Time.fixedDeltaTime);
//     }

//     void DetectTwistToToggle()
//     {
//         float pitch = headCamera.eulerAngles.x;
//         if (pitch > 180f) pitch -= 360f;

//         if (pitch > lookDownThreshold)
//         {
//             float currentYaw = headCamera.eulerAngles.y;
//             float yawDelta = Mathf.DeltaAngle(previousYaw, currentYaw);

//             twistAccumulated += Mathf.Abs(yawDelta);
//             twistTimer += Time.deltaTime;

//             if (twistAccumulated > twistSensitivity)
//             {
//                 isMoving = !isMoving;
//                 Debug.Log("Twist triggered → Toggled movement: " + isMoving);
//                 // UpdateMovementStatusUI();
//                 twistAccumulated = 0f;
//                 twistTimer = 0f;
//             }

//             if (twistTimer > twistDetectionTime)
//             {
//                 twistAccumulated = 0f;
//                 twistTimer = 0f;
//             }

//             previousYaw = currentYaw;
//         }
//         else
//         {
//             twistAccumulated = 0f;
//             twistTimer = 0f;
//             previousYaw = headCamera.eulerAngles.y;
//         }
//     }

//     // void UpdateMovementStatusUI()
//     // {
//     //     if (movementStatusText != null)
//     //     {
//     //         movementStatusText.text = isMoving ? "MOVING" : "STOPPED";
//     //         movementStatusText.color = isMoving ? Color.green : Color.red;
//     //     }
//     // }

//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Ball") && Time.time > lastKickTime + kickCooldown)
//         {
//             Rigidbody ballRb = other.GetComponent<Rigidbody>();
//             if (ballRb != null)
//             {
//                 Vector3 kickDir = headCamera.forward;
//                 kickDir.y = 0f;

//                 ballRb.linearVelocity = Vector3.zero;
//                 ballRb.AddForce(kickDir.normalized * kickStrength, ForceMode.Impulse);

//                 lastKickTime = Time.time;

//                 Debug.Log("Ball kicked toward look direction!");
//             }
//         }
//     }
// }



// ======





using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform headCamera;
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float lookDownThreshold = 15.0f;

    [Header("Kick Settings")]
    public float kickStrength = 5f;
    public float kickCooldown = 1f;

    [Header("Twist Detection Settings")]
    public float twistAngleRequired = 90f; // Total angle needed to trigger
    public float twistTimeWindow = 2f; // Time window to complete twist
    public float minTwistSpeed = 30f; // Minimum degrees per second to count as twist
    public float lookDownAngleTolerance = 5f; // Tolerance for "looking down" angle

    [Header("Debug Settings")]
    public bool showDebugInfo = true;
    public TextMeshProUGUI debugText; // Optional: for debugging twist values

    private float lastKickTime = -999f;
    private Rigidbody rb;
    private bool isGrounded;
    private bool isMoving = true;
    private bool wasMoving = true;

    // Improved twist detection variables
    private bool isInTwistMode = false;
    private float twistStartTime;
    private float totalTwistAngle = 0f;
    private float lastValidYaw = 0f;
    private bool wasLookingDown = false;
    private float lastFrameTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
        lastFrameTime = Time.time;
        lastValidYaw = headCamera.eulerAngles.y;
        
        UpdateHUDMovementStatus();
    }

    void Update()
    {
        if (headCamera == null) return;

        CheckGrounded();
        ImprovedTwistDetection();
        
        // Update HUD if movement status changed
        if (isMoving != wasMoving)
        {
            UpdateHUDMovementStatus();
            wasMoving = isMoving;
        }

        // Debug display
        if (showDebugInfo && debugText != null)
        {
            UpdateDebugDisplay();
        }
    }

    void FixedUpdate()
    {
        if (headCamera == null) return;
        HandleMovement();
    }

    void ImprovedTwistDetection()
    {
        float pitch = GetNormalizedPitch();
        bool isLookingDown = pitch > (lookDownThreshold - lookDownAngleTolerance) && 
                           pitch < (lookDownThreshold + 45f); // Max down angle

        // Debug current state
        if (showDebugInfo)
        {
            Debug.Log($"Pitch: {pitch:F1}°, Looking Down: {isLookingDown}, Twist Mode: {isInTwistMode}, Total Twist: {totalTwistAngle:F1}°");
        }

        if (isLookingDown)
        {
            if (!wasLookingDown)
            {
                // Just started looking down - initialize twist detection
                StartTwistDetection();
            }
            else if (isInTwistMode)
            {
                // Continue tracking twist while looking down
                TrackTwistMovement();
            }
        }
        else
        {
            if (wasLookingDown && isInTwistMode)
            {
                // Stopped looking down - reset twist detection
                ResetTwistDetection();
            }
        }

        wasLookingDown = isLookingDown;
    }

    void StartTwistDetection()
    {
        isInTwistMode = true;
        twistStartTime = Time.time;
        totalTwistAngle = 0f;
        lastValidYaw = headCamera.eulerAngles.y;
        lastFrameTime = Time.time;

        if (showDebugInfo)
        {
            Debug.Log("🔄 Started twist detection mode");
        }
    }

    void TrackTwistMovement()
    {
        float currentYaw = headCamera.eulerAngles.y;
        float deltaTime = Time.time - lastFrameTime;
        
        if (deltaTime > 0f)
        {
            // Calculate the shortest angle difference
            float yawDelta = Mathf.DeltaAngle(lastValidYaw, currentYaw);
            float twistSpeed = Mathf.Abs(yawDelta) / deltaTime;

            // Only count significant movements (filter out small jitters)
            if (twistSpeed > minTwistSpeed && Mathf.Abs(yawDelta) > 2f)
            {
                totalTwistAngle += Mathf.Abs(yawDelta);
                lastValidYaw = currentYaw;

                if (showDebugInfo)
                {
                    Debug.Log($"🌪️ Twist movement: {yawDelta:F1}° (Speed: {twistSpeed:F1}°/s, Total: {totalTwistAngle:F1}°)");
                }
            }
        }

        lastFrameTime = Time.time;

        // Check if twist is complete
        if (totalTwistAngle >= twistAngleRequired)
        {
            ExecuteTwist();
        }
        
        // Check if time window expired
        if (Time.time - twistStartTime > twistTimeWindow)
        {
            if (showDebugInfo && totalTwistAngle > 0)
            {
                Debug.Log($"⏰ Twist timeout - only {totalTwistAngle:F1}° in {twistTimeWindow}s");
            }
            ResetTwistDetection();
        }
    }

    void ExecuteTwist()
    {
        isMoving = !isMoving;
        
        string status = isMoving ? "STARTED" : "STOPPED";
        Debug.Log($"✅ TWIST COMPLETE! Movement {status} (Total twist: {totalTwistAngle:F1}° in {Time.time - twistStartTime:F1}s)");
        
        ResetTwistDetection();
    }

    void ResetTwistDetection()
    {
        isInTwistMode = false;
        totalTwistAngle = 0f;
        twistStartTime = 0f;
    }

    float GetNormalizedPitch()
    {
        float pitch = headCamera.eulerAngles.x;
        // Normalize to -180 to 180 range
        if (pitch > 180f) pitch -= 360f;
        // Convert to positive angle (0-90 for looking down)
        return Mathf.Abs(pitch);
    }

    void UpdateDebugDisplay()
    {
        if (debugText != null)
        {
            float pitch = GetNormalizedPitch();
            string status = isInTwistMode ? "DETECTING" : "WAITING";
            debugText.text = $"Pitch: {pitch:F1}°\n" +
                           $"Twist: {status}\n" +
                           $"Angle: {totalTwistAngle:F1}°/{twistAngleRequired}°\n" +
                           $"Moving: {isMoving}";
        }
    }

    void CheckGrounded()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        isGrounded = Physics.Raycast(ray, 1.1f);
    }

    void HandleMovement()
    {
        if (!isGrounded || !isMoving) return;

        Vector3 forward = Vector3.ProjectOnPlane(headCamera.forward, Vector3.up).normalized;
        float pitch = GetNormalizedPitch();
        float speed = pitch > lookDownThreshold ? sprintSpeed : walkSpeed;

        rb.MovePosition(rb.position + forward * speed * Time.fixedDeltaTime);
    }

    void UpdateHUDMovementStatus()
    {
        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.UpdateMovementStatus(isMoving);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") && Time.time > lastKickTime + kickCooldown)
        {
            Rigidbody ballRb = other.GetComponent<Rigidbody>();
            if (ballRb != null)
            {
                Vector3 kickDir = headCamera.forward;
                kickDir.y = 0f;

                ballRb.linearVelocity = Vector3.zero;
                ballRb.AddForce(kickDir.normalized * kickStrength, ForceMode.Impulse);

                lastKickTime = Time.time;
                Debug.Log("Ball kicked toward look direction!");
            }
        }
    }

    // Public methods
    public bool IsMoving() => isMoving;
    public void SetMoving(bool moving) 
    { 
        isMoving = moving;
        UpdateHUDMovementStatus();
    }
    
    // Manual test method
    [ContextMenu("Test Twist Toggle")]
    public void TestTwistToggle()
    {
        isMoving = !isMoving;
        UpdateHUDMovementStatus();
        Debug.Log($"Manual twist test - Movement: {isMoving}");
    }
}