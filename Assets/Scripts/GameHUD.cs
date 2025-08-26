using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// GameHUD Controller
public class GameHUD : MonoBehaviour
{
    [Header("Score Elements")]
    public TextMeshProUGUI player1NameText;
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2NameText;
    public TextMeshProUGUI player2ScoreText;
    
    [Header("Game Info Elements")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI gameModeText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText;
    
    [Header("Connection Elements")]
    public TextMeshProUGUI roleText;
    public TextMeshProUGUI opponentText;
    public Image connectionIcon;
    
    public void UpdateScore(string p1Name, int p1Score, string p2Name, int p2Score)
    {
        if (player1NameText) player1NameText.text = p1Name;
        if (player1ScoreText) player1ScoreText.text = p1Score.ToString();
        if (player2NameText) player2NameText.text = p2Name;
        if (player2ScoreText) player2ScoreText.text = p2Score.ToString();
    }
    
    public void UpdateRound(int current, int max)
    {
        if (roundText) roundText.text = $"ROUND {current} OF {max}";
    }
    
    // public void UpdateRole(PlayerRole role)
    // {
    //     if (roleText) roleText.text = role.ToString().ToUpper();
    //     if (opponentText) opponentText.text = $"vs {(role == PlayerRole.Shooter ? "KEEPER" : "SHOOTER")}";
    // }
    
    public void UpdateGameStatus(string status)
    {
        if (statusText) statusText.text = status;
    }
    
    public void UpdateTimer(float remainingTime)
    {
        if (timerText)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
            
            // Change color based on time remaining
            if (remainingTime < 10f)
                timerText.color = Color.red;
            else if (remainingTime < 20f)
                timerText.color = Color.yellow;
            else
                timerText.color = Color.white;
        }
    }
}