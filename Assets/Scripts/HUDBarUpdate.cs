// using TMPro;
// using UnityEngine;

// public class PlayerHUD : MonoBehaviour
// {
//     public TextMeshProUGUI GoalTextBUpdate;
//     public TextMeshProUGUI GoalTextAUpdate;
//     public TextMeshProUGUI MovementStatusText;
//     public TextMeshProUGUI healthText;

//     private int goals = 0;
//     private int health = 100;

//     void Start()
//     {
//         UpdateHUD();
//     }

//     void Update()
//     {
//         // Example — update status based on player input
//         if (Input.GetKey(KeyCode.W))
//             MovementStatusText.text = "Status: Moving";
//         else
//             MovementStatusText.text = "Status: Idle";
//     }

//     public void AddGoal()
//     {
//         goals++;
//         GoalTextAUpdate.text += goals;
//         GoalTextBUpdate.text += goals;
//     }

//     public void TakeDamage(int amount)
//     {
//         health -= amount;
//         healthText.text = "health: " +health;
//     }

//     private void UpdateHUD()
//     {
//         GoalTextAUpdate.text += goals;
//         GoalTextBUpdate.text += goals;
//         MovementStatusText.text = "Status: Idle";
//         healthText.text =  "health: "+health;
//     }
// }


using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [Header("UI Text Elements")]
    public TextMeshProUGUI GoalTextBUpdate;
    public TextMeshProUGUI GoalTextAUpdate;
    public TextMeshProUGUI MovementStatusText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI gameStatusText; // Optional: for game over, winner, etc.

    [Header("Game Settings")]
    public int maxHealth = 100;
    public int winningGoals = 5; // Optional: set winning condition

    // Private variables
    private int teamAGoals = 0;
    private int teamBGoals = 0;
    private int health = 100;
    private bool gameEnded = false;

    // Singleton pattern for easy access from other scripts
    public static PlayerHUD Instance;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        health = maxHealth;
        UpdateHUD();
    }

    void Update()
    {
        // Movement status will be updated by PlayerMovement script
        // No need to handle it here anymore
    }

    // Method to add goal for specific team
    public void AddGoal(string teamName)
    {
        if (gameEnded) return;

        if (teamName == "Team A")
        {
            teamAGoals++;
            Debug.Log($"Team A scored! Total goals: {teamAGoals}");
        }
        else if (teamName == "Team B")
        {
            teamBGoals++;
            Debug.Log($"Team B scored! Total goals: {teamBGoals}");
        }

        UpdateGoalDisplay();
        CheckWinCondition();
    }

    // Method to update movement status (called from PlayerMovement)
    public void UpdateMovementStatus(bool isMoving)
    {
        if (MovementStatusText != null)
        {
            MovementStatusText.text = isMoving ? "Status: Moving" : "Status: Stopped";
            MovementStatusText.color = isMoving ? Color.green : Color.red;
        }
    }

    // Method to take damage
    public void TakeDamage(int amount)
    {
        if (gameEnded) return;

        health -= amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        
        UpdateHealthDisplay();
        
        if (health <= 0)
        {
            GameOver();
        }
    }

    // Method to restore health
    public void RestoreHealth(int amount)
    {
        if (gameEnded) return;

        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthDisplay();
    }

    // Update all HUD elements
    private void UpdateHUD()
    {
        UpdateGoalDisplay();
        UpdateHealthDisplay();
        UpdateMovementStatus(false); // Default to stopped
        
        if (gameStatusText != null)
        {
            gameStatusText.text = "Game Active";
            gameStatusText.color = Color.white;
        }
    }

    // Update goal display
    private void UpdateGoalDisplay()
    {
        if (GoalTextAUpdate != null)
        {
            GoalTextAUpdate.text = $"Team A: {teamAGoals}";
        }
        
        if (GoalTextBUpdate != null)
        {
            GoalTextBUpdate.text = $"Team B: {teamBGoals}";
        }
    }

    // Update health display
    private void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            healthText.text = $"Health: {health}";
            
            // Change color based on health
            if (health > 70)
                healthText.color = Color.green;
            else if (health > 30)
                healthText.color = Color.yellow;
            else
                healthText.color = Color.red;
        }
    }

    // Check if someone won
    private void CheckWinCondition()
    {
        if (winningGoals > 0)
        {
            if (teamAGoals >= winningGoals)
            {
                GameWon("Team A");
            }
            else if (teamBGoals >= winningGoals)
            {
                GameWon("Team B");
            }
        }
    }

    // Handle game win
    private void GameWon(string winnerTeam)
    {
        gameEnded = true;
        
        if (gameStatusText != null)
        {
            gameStatusText.text = $"{winnerTeam} Wins!";
            gameStatusText.color = Color.blue;
        }
        
        Debug.Log($"Game Over! {winnerTeam} wins with {(winnerTeam == "Team A" ? teamAGoals : teamBGoals)} goals!");
    }

    // Handle game over (health reached 0)
    private void GameOver()
    {
        gameEnded = true;
        
        if (gameStatusText != null)
        {
            gameStatusText.text = "Game Over";
            gameStatusText.color = Color.red;
        }
        
        Debug.Log("Game Over! Health reached 0!");
    }

    // Reset game
    public void ResetGame()
    {
        teamAGoals = 0;
        teamBGoals = 0;
        health = maxHealth;
        gameEnded = false;
        UpdateHUD();
        
        Debug.Log("Game Reset!");
    }

    // Getter methods for other scripts
    public int GetTeamAGoals() => teamAGoals;
    public int GetTeamBGoals() => teamBGoals;
    public int GetHealth() => health;
    public bool IsGameEnded() => gameEnded;
}