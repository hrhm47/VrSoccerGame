// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Collections;

// // GoalkeeperUI Controller
// public class GoalkeeperUI : MonoBehaviour
// {
//     [Header("Phase Panels")]
//     public GameObject predictionPhase;
//     public GameObject waitingPhase;
//     public GameObject resultPhase;
    
//     [Header("Prediction")]
//     public Button diveLeftButton;
//     public Button stayCenterButton;
//     public Button diveRightButton;
//     public TextMeshProUGUI predictionTimerText;
//     public Slider predictionTimerBar;
    
//     [Header("Waiting")]
//     public TextMeshProUGUI waitingTitle;
//     public TextMeshProUGUI yourChoiceText;
//     public TextMeshProUGUI confidenceText;
    
//     [Header("Result")]
//     public TextMeshProUGUI saveResultTitle;
//     public TextMeshProUGUI saveDescription;
//     public Button nextRoundButton;
    
//     private string selectedPrediction = "";
    
//     void Start()
//     {
//         SetupButtons();
//     }
    
//     void SetupButtons()
//     {
//         if (diveLeftButton) diveLeftButton.onClick.AddListener(() => OnPredictionSelected("LeftKeepSpot"));
//         if (stayCenterButton) stayCenterButton.onClick.AddListener(() => OnPredictionSelected("CenterKeepSpot"));
//         if (diveRightButton) diveRightButton.onClick.AddListener(() => OnPredictionSelected("RightKeepSpot"));
//         if (nextRoundButton) nextRoundButton.onClick.AddListener(OnNextRoundPressed);
//     }
    
//     public void ShowPredictionPhase()
//     {
//         HideAllPhases();
//         if (predictionPhase) predictionPhase.SetActive(true);
//         StartPredictionTimer();
//     }
    
//     public void ShowWaitingPhase()
//     {
//         HideAllPhases();
//         if (waitingPhase) waitingPhase.SetActive(true);
//         UpdateWaitingDisplay();
//     }
    
//     public void ShowResultPhase()
//     {
//         HideAllPhases();
//         if (resultPhase) resultPhase.SetActive(true);
//     }
    
//     void HideAllPhases()
//     {
//         if (predictionPhase) predictionPhase.SetActive(false);
//         if (waitingPhase) waitingPhase.SetActive(false);
//         if (resultPhase) resultPhase.SetActive(false);
//     }
    
//     void StartPredictionTimer()
//     {
//         StartCoroutine(UpdatePredictionTimer());
//     }
    
//     IEnumerator UpdatePredictionTimer()
//     {
//         float timeLimit = 12f;
//         float elapsed = 0f;
        
//         while (elapsed < timeLimit && predictionPhase.activeInHierarchy)
//         {
//             elapsed += Time.deltaTime;
//             float remaining = timeLimit - elapsed;
            
//             if (predictionTimerText) predictionTimerText.text = $"DECIDE IN: {remaining:F0}s";
//             if (predictionTimerBar) predictionTimerBar.value = remaining;
            
//             yield return null;
//         }
//     }
    
//     void OnPredictionSelected(string prediction)
//     {
//         selectedPrediction = prediction;
//         Debug.Log($"Goalkeeper prediction: {prediction}");
        
//         GameManager.Instance.OnPositionSelected(prediction);
        
//         // Teleport to selected position
//         TeleportationManager.Instance.TeleportToSpot(prediction);
//     }
    
//     void UpdateWaitingDisplay()
//     {
//         if (yourChoiceText && !string.IsNullOrEmpty(selectedPrediction))
//         {
//             string displayName = selectedPrediction.Replace("KeepSpot", "").Replace("Keep", "");
//             yourChoiceText.text = $"You chose: {displayName.ToUpper()}";
//         }
        
//         if (confidenceText)
//         {
//             // Simple confidence calculation
//             string confidence = selectedPrediction.Contains("Center") ? "MEDIUM" : "HIGH";
//             confidenceText.text = $"Confidence: {confidence}";
//         }
//     }
    
//     void OnNextRoundPressed()
//     {
//         GameManager.Instance.NextTurn();
//     }
    
//     public void ShowResult(string title, string description, bool isPositive)
//     {
//         ShowResultPhase();
        
//         if (saveResultTitle)
//         {
//             saveResultTitle.text = title;
//             saveResultTitle.color = isPositive ? Color.blue : Color.red;
//         }
        
//         if (saveDescription) saveDescription.text = description;
//     }
// }
