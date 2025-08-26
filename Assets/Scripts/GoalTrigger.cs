// using UnityEngine;

// public class GoalTrigger : MonoBehaviour
// {
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     public string teamName = "Team A";


//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Ball"))
//         {
//             Debug.Log($"GOAL! {teamName} scored!");
//             // Add goal count or celebration here
//         }
//     }
// }


using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [Header("Goal Settings")]
    public string teamName = "Team A"; // Set this in Inspector for each goal
    public bool playGoalEffect = true;
    public AudioClip goalSound; // Optional: add goal sound
    public GameObject goalEffect; // Optional: add particle effect
    
    [Header("Ball Reset Settings")]
    public Transform ballResetPosition; // Where to reset ball after goal
    public float resetDelay = 2f; // Delay before resetting ball
    
    private AudioSource audioSource;
    private bool goalScored = false; // Prevent multiple triggers
    
    void Start()
    {
        // Get or add AudioSource for goal sound
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && goalSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the ball and goal hasn't been scored yet
        if (other.CompareTag("Ball") && !goalScored)
        {
            goalScored = true;
            HandleGoal(other.gameObject);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Reset goal trigger when ball leaves (for next goal)
        if (other.CompareTag("Ball"))
        {
            goalScored = false;
        }
    }

    private void HandleGoal(GameObject ball)
    {
        // Update HUD score
        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.AddGoal(teamName);
        }
        else
        {
            Debug.LogError("PlayerHUD instance not found! Make sure PlayerHUD script is active.");
        }

        // Play goal sound
        if (playGoalEffect && goalSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(goalSound);
        }

        // Show goal effect
        if (playGoalEffect && goalEffect != null)
        {
            GameObject effect = Instantiate(goalEffect, ball.transform.position, Quaternion.identity);
            Destroy(effect, 3f); // Clean up after 3 seconds
        }

        // Log the goal
        Debug.Log($"GOAL! {teamName} scored!");

        // Reset ball position after delay
        if (ballResetPosition != null)
        {
            Invoke("ResetBall", resetDelay);
        }
    }

    private void ResetBall()
    {
        GameObject ball = GameObject.FindGameObjectWithTag("Ball");
        if (ball != null && ballResetPosition != null)
        {
            // Stop ball movement
            Rigidbody ballRb = ball.GetComponent<Rigidbody>();
            if (ballRb != null)
            {
                ballRb.linearVelocity = Vector3.zero;
                ballRb.angularVelocity = Vector3.zero;
            }

            // Reset position
            ball.transform.position = ballResetPosition.position;
            ball.transform.rotation = ballResetPosition.rotation;
            
            Debug.Log("Ball reset to starting position");
        }
    }

    // Method to manually trigger goal (useful for testing)
    public void TestGoal()
    {
        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.AddGoal(teamName);
            Debug.Log($"Test goal triggered for {teamName}");
        }
    }
}