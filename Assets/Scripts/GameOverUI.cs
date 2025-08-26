// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Collections;

// // GameOverUI Controller
// public class GameOverUI : MonoBehaviour
// {
//     [Header("Winner Display")]
//     public TextMeshProUGUI winnerTitle;
//     public TextMeshProUGUI winnerSubtitle;
    
//     [Header("Score Display")]
//     public TextMeshProUGUI finalScore;
//     public TextMeshProUGUI matchDuration;
    
//     [Header("Stats Display")]
//     public TextMeshProUGUI player1ShotsText;
//     public TextMeshProUGUI player1SavesText;
//     public TextMeshProUGUI player2ShotsText;
//     public TextMeshProUGUI player2SavesText;
    
//     [Header("Action Buttons")]
//     public Button playAgainButton;
//     public Button mainMenuButton;
//     public Button quitButton;
    
//     void Start()
//     {
//         SetupButtons();
//     }
    
//     void SetupButtons()
//     {
//         if (playAgainButton) playAgainButton.onClick.AddListener(OnPlayAgainPressed);
//         if (mainMenuButton) mainMenuButton.onClick.AddListener(OnMainMenuPressed);
//         if (quitButton) quitButton.onClick.AddListener(OnQuitPressed);
//     }
    
//     public void ShowGameOver(string winner, int p1Score, int p2Score, int p1Shots, int p2Shots, int p1Saves, int p2Saves)
//     {
//         gameObject.SetActive(true);
        
//         // Winner display
//         if (winnerTitle)
//         {
//             if (winner == "TIE")
//             {
//                 winnerTitle.text = "IT'S A TIE!";
//                 winnerTitle.color = Color.yellow;
//             }
//             else
//             {
//                 winnerTitle.text = $"{winner.ToUpper()} WINS!";
//                 winnerTitle.color = Color.gold;
//             }
//         }
        
//         // Final score
//         if (finalScore) finalScore.text = $"FINAL SCORE: {p1Score}-{p2Score}";
        
//         // Match stats
//         if (player1ShotsText) player1ShotsText.text = $"Player 1: {p1Shots} shots, {p1Score} goals";
//         if (player1SavesText) player1SavesText.text = $"Player 1: {p1Saves} saves";
//         if (player2ShotsText) player2ShotsText.text = $"Player 2: {p2Shots} shots, {p2Score} goals";
//         if (player2SavesText) player2SavesText.text = $"Player 2: {p2Saves} saves";
//     }
    
//     void OnPlayAgainPressed()
//     {
//         GameManager.Instance.RestartGame();
//     }
    
//     void OnMainMenuPressed()
//     {
//         GameManager.Instance.ReturnToMainMenu();
//     }
    
//     void OnQuitPressed()
//     {
//         Application.Quit();
//     }
// }

