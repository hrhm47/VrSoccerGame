// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class TeleportUIController : MonoBehaviour
// {
//     [Header("Buttons")]
//     public Button leftSpotButton;
//     public Button centerSpotButton;
//     public Button rightSpotButton;
//     public Button benchSpotButton;
    
//     [Header("Display")]
//     public TextMeshProUGUI currentPositionText;
    
//     void Start()
//     {
//         SetupButtons();
//     }
    
//     void SetupButtons()
//     {
//         if (leftSpotButton) leftSpotButton.onClick.AddListener(() => TeleportToSpot("Left"));
//         if (centerSpotButton) centerSpotButton.onClick.AddListener(() => TeleportToSpot("Center"));
//         if (rightSpotButton) rightSpotButton.onClick.AddListener(() => TeleportToSpot("Right"));
//         if (benchSpotButton) benchSpotButton.onClick.AddListener(() => TeleportToSpot("Bench"));
//     }
    
//     void TeleportToSpot(string direction)
//     {
//         string spotName = direction + (GameManager.Instance.currentRole == PlayerRole.Shooter ? "ShooterSpot" : "KeepSpot");
//         if (direction == "Bench") spotName = "BenchSpot";
        
//         TeleportationManager.Instance?.TeleportToSpot(spotName);
//         UpdateCurrentPosition(spotName);
//     }
    
//     void UpdateCurrentPosition(string spotName)
//     {
//         if (currentPositionText)
//         {
//             string displayName = spotName.Replace("ShooterSpot", "").Replace("KeepSpot", "").Replace("Spot", "");
//             currentPositionText.text = $"CURRENT: {displayName.ToUpper()}";
//         }
//     }
// }


// ===========================================
// TeleportUIController.cs - Add to TeleportUI Panel
// ===========================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeleportUIController : MonoBehaviour
{
    [Header("Buttons")]
    public Button leftSpotButton;
    public Button centerSpotButton;
    public Button rightSpotButton;
    public Button benchSpotButton;
    
    [Header("Display")]
    public TextMeshProUGUI currentPositionText;
    public TextMeshProUGUI teleportInstructions;
    
    void Start()
    {
        SetupButtons();
        UpdateDisplay();
    }
    
    void SetupButtons()
    {
        if (leftSpotButton) leftSpotButton.onClick.AddListener(() => TeleportToSpot("Left"));
        if (centerSpotButton) centerSpotButton.onClick.AddListener(() => TeleportToSpot("Center"));
        if (rightSpotButton) rightSpotButton.onClick.AddListener(() => TeleportToSpot("Right"));
        if (benchSpotButton) benchSpotButton.onClick.AddListener(() => TeleportToSpot("Bench"));
    }
    
    void TeleportToSpot(string direction)
    {
        string spotName;
        
        if (direction == "Bench")
        {
            spotName = "BenchSpot";
        }
        else
        {
            PlayerRole role = GameManager.Instance.currentRole;
            spotName = direction + (role == PlayerRole.Shooter ? "ShooterSpot" : "KeepSpot");
        }
        
        TeleportationManager.Instance?.TeleportToSpot(spotName);
        UpdateDisplay();
        
        Debug.Log($"Teleported to {spotName}");
    }
    
    void UpdateDisplay()
    {
        if (currentPositionText && TeleportationManager.Instance != null)
        {
            string current = TeleportationManager.Instance.GetCurrentSpot();
            string displayName = current.Replace("ShooterSpot", "").Replace("KeepSpot", "").Replace("Spot", "");
            currentPositionText.text = $"CURRENT: {displayName.ToUpper()}";
        }
    }
}
