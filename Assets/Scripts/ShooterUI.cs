// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Collections;

// // ShooterUI Controller
// public class ShooterUI : MonoBehaviour
// {
//     [Header("Phase Panels")]
//     public GameObject positionSelectionPhase;
//     public GameObject aimingPhase;
//     public GameObject resultPhase;
    
//     [Header("Position Selection")]
//     public Button leftPositionButton;
//     public Button centerPositionButton;
//     public Button rightPositionButton;
//     public TextMeshProUGUI positionTimerText;
//     public Slider positionTimerBar;
    
//     [Header("Aiming")]
//     public Slider powerMeter;
//     public Image crosshairImage;
//     public Button shootButton;
//     public TextMeshProUGUI aimTimerText;
    
//     [Header("Result")]
//     public TextMeshProUGUI resultTitle;
//     public TextMeshProUGUI shotDescription;
//     public Button nextRoundButton;
    
//     private HeadAimingController headAiming;
//     private HeadPowerController headPower;
    
//     void Start()
//     {
//         SetupButtons();
//         headAiming = FindObjectOfType<HeadAimingController>();
//         headPower = FindObjectOfType<HeadPowerController>();
//     }
    
//     void SetupButtons()
//     {
//         if (leftPositionButton) leftPositionButton.onClick.AddListener(() => OnPositionSelected("LeftShooterSpot"));
//         if (centerPositionButton) centerPositionButton.onClick.AddListener(() => OnPositionSelected("CenterShooterSpot"));
//         if (rightPositionButton) rightPositionButton.onClick.AddListener(() => OnPositionSelected("RightShooterSpot"));
//         if (shootButton) shootButton.onClick.AddListener(OnShootButtonPressed);
//         if (nextRoundButton) nextRoundButton.onClick.AddListener(OnNextRoundPressed);
//     }
    
//     public void ShowPositionSelectionPhase()
//     {
//         HideAllPhases();
//         if (positionSelectionPhase) positionSelectionPhase.SetActive(true);
//         StartPositionTimer();
//     }
    
//     public void ShowAimingPhase()
//     {
//         HideAllPhases();
//         if (aimingPhase) aimingPhase.SetActive(true);
//         StartAiming();
//     }
    
//     public void ShowResultPhase()
//     {
//         HideAllPhases();
//         if (resultPhase) resultPhase.SetActive(true);
//     }
    
//     void HideAllPhases()
//     {
//         if (positionSelectionPhase) positionSelectionPhase.SetActive(false);
//         if (aimingPhase) aimingPhase.SetActive(false);
//         if (resultPhase) resultPhase.SetActive(false);
//     }
    
//     void StartPositionTimer()
//     {
//         StartCoroutine(UpdatePositionTimer());
//     }
    
//     IEnumerator UpdatePositionTimer()
//     {
//         float timeLimit = 15f;
//         float elapsed = 0f;
        
//         while (elapsed < timeLimit && positionSelectionPhase.activeInHierarchy)
//         {
//             elapsed += Time.deltaTime;
//             float remaining = timeLimit - elapsed;
            
//             if (positionTimerText) positionTimerText.text = $"CHOOSE IN: {remaining:F0}s";
//             if (positionTimerBar) positionTimerBar.value = remaining;
            
//             yield return null;
//         }
//     }
    
//     void StartAiming()
//     {
//         if (headAiming) headAiming.enabled = true;
//         if (headPower) headPower.enabled = true;
//         StartCoroutine(UpdateAimingTimer());
//     }
    
//     IEnumerator UpdateAimingTimer()
//     {
//         float timeLimit = 20f;
//         float elapsed = 0f;
        
//         while (elapsed < timeLimit && aimingPhase.activeInHierarchy)
//         {
//             elapsed += Time.deltaTime;
//             float remaining = timeLimit - elapsed;
            
//             if (aimTimerText) aimTimerText.text = $"SHOOT IN: {remaining:F0}s";
            
//             // Update power meter from head controller
//             if (headPower && powerMeter)
//                 powerMeter.value = headPower.GetCurrentPower();
            
//             yield return null;
//         }
//     }
    
//     void OnPositionSelected(string position)
//     {
//         Debug.Log($"Position selected: {position}");
//         GameManager.Instance.OnPositionSelected(position);
        
//         // Teleport to selected position
//         TeleportationManager.Instance.TeleportToSpot(position);
//     }
    
//     void OnShootButtonPressed()
//     {
//         Vector3 aimDirection = headAiming ? headAiming.GetAimDirection() : Vector3.forward;
//         float power = headPower ? headPower.GetCurrentPower() : 50f;
        
//         Debug.Log($"Shot taken - Direction: {aimDirection}, Power: {power}");
//         GameManager.Instance.OnShotTaken(aimDirection, power);
        
//         // Disable aiming controls
//         if (headAiming) headAiming.enabled = false;
//         if (headPower) headPower.enabled = false;
//     }
    
//     void OnNextRoundPressed()
//     {
//         GameManager.Instance.NextTurn();
//     }
    
//     public void ShowResult(string title, string description, bool isPositive)
//     {
//         ShowResultPhase();
        
//         if (resultTitle)
//         {
//             resultTitle.text = title;
//             resultTitle.color = isPositive ? Color.green : Color.red;
//         }
        
//         if (shotDescription) shotDescription.text = description;
//     }
// }