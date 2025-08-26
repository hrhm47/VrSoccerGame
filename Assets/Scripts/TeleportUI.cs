// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Collections;

// // TeleportUI Controller
// public class TeleportUI : MonoBehaviour
// {
//     [Header("UI Elements")]
//     public TextMeshProUGUI currentPositionText;
//     public TextMeshProUGUI teleportInstructions;
//     public Button leftSpotButton;
//     public Button centerSpotButton;
//     public Button rightSpotButton;
//     public Button benchSpotButton;
//     public Slider teleportProgress;
    
//     [Header("Button Colors")]
//     public Color availableColor = Color.green;
//     public Color unavailableColor = Color.gray;
//     public Color currentColor = Color.yellow;
    
//     // private PlayerRole currentRole;
    
//     void Start()
//     {
//         SetupButtons();
//     }
    
//     void SetupButtons()
//     {
//         if (leftSpotButton) leftSpotButton.onClick.AddListener(() => OnTeleportButtonPressed("Left"));
//         if (centerSpotButton) centerSpotButton.onClick.AddListener(() => OnTeleportButtonPressed("Center"));
//         if (rightSpotButton) rightSpotButton.onClick.AddListener(() => OnTeleportButtonPressed("Right"));
//         if (benchSpotButton) benchSpotButton.onClick.AddListener(() => OnTeleportButtonPressed("Bench"));
//     }
    
//     // public void SetRole(PlayerRole role)
//     // {
//     //     currentRole = role;
//     //     UpdateButtonStates();
//     // }
    
//     void UpdateButtonStates()
//     {
//         string currentSpot = TeleportationManager.Instance.GetCurrentSpot();
        
//         // Update button colors and interactability
//         UpdateButton(leftSpotButton, GetSpotName("Left"), currentSpot);
//         UpdateButton(centerSpotButton, GetSpotName("Center"), currentSpot);
//         UpdateButton(rightSpotButton, GetSpotName("Right"), currentSpot);
//         UpdateButton(benchSpotButton, "BenchSpot", currentSpot);
//     }
    
//     void UpdateButton(Button button, string spotName, string currentSpot)
//     {
//         if (button == null) return;
        
//         bool isCurrentSpot = currentSpot == spotName;
//         bool isAvailable = CanAccessSpot(spotName);
        
//         // Set button color
//         Image buttonImage = button.GetComponent<Image>();
//         if (buttonImage != null)
//         {
//             if (isCurrentSpot)
//                 buttonImage.color = currentColor;
//             else if (isAvailable)
//                 buttonImage.color = availableColor;
//             else
//                 buttonImage.color = unavailableColor;
//         }
        
//         // Set interactability
//         button.interactable = isAvailable && !isCurrentSpot;
        
//         // Update text
//         TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
//         if (buttonText != null && isCurrentSpot)
//         {
//             buttonText.text += "\n(CURRENT)";
//         }
//     }
    
//     string GetSpotName(string direction)
//     {
//         if (currentRole == PlayerRole.Shooter)
//             return direction + "ShooterSpot";
//         else
//             return direction + "KeepSpot";
//     }
    
//     bool CanAccessSpot(string spotName)
//     {
//         if (spotName == "BenchSpot") return true;
//         if (spotName.Contains("Shooter") && currentRole == PlayerRole.Shooter) return true;
//         if (spotName.Contains("Keep") && currentRole == PlayerRole.Goalkeeper) return true;
//         return false;
//     }
    
//     void OnTeleportButtonPressed(string direction)
//     {
//         string targetSpot = direction == "Bench" ? "BenchSpot" : GetSpotName(direction);
//         TeleportationManager.Instance.TeleportToSpot(targetSpot);
//         UpdateButtonStates();
//     }
    
//     public void UpdateCurrentPosition(string spotName)
//     {
//         if (currentPositionText != null)
//         {
//             string displayName = spotName.Replace("ShooterSpot", "").Replace("KeepSpot", "").Replace("Spot", "");
//             currentPositionText.text = $"CURRENT: {displayName.ToUpper()}";
//         }
//     }
    
//     public void UpdateTeleportProgress(float progress)
//     {
//         if (teleportProgress != null)
//             teleportProgress.value = progress;
//     }
// }