using UnityEngine;
using UnityEngine.UI;

public class ButtonConnector : MonoBehaviour
{
    void Start()
    {
        // Wait a bit for UI to initialize, then connect buttons
        Invoke("ConnectAllButtons", 0.5f);
    }
    
    void ConnectAllButtons()
    {
        Debug.Log("Connecting all game buttons...");
        
        // Position selection buttons
        ConnectButton("LeftPositionButton", () => GameManager.Instance?.OnPositionSelected("Left"));
        ConnectButton("CenterPositionButton", () => GameManager.Instance?.OnPositionSelected("Center"));
        ConnectButton("RightPositionButton", () => GameManager.Instance?.OnPositionSelected("Right"));
        
        // Shoot button
        ConnectButton("ShootButton", () => GameManager.Instance?.OnShootPressed());
        
        // Next round button
        ConnectButton("NextRoundButton", () => GameManager.Instance?.OnNextRound());
        
        // Goalkeeper buttons - FIXED by using OnGoalkeeperPositionSelected
        ConnectButton("DiveLeftButton", () => GameManager.Instance?.OnGoalkeeperPositionSelected("LeftKeepSpot"));
        ConnectButton("StayCenterButton", () => GameManager.Instance?.OnGoalkeeperPositionSelected("CenterKeepSpot"));
        ConnectButton("DiveRightButton", () => GameManager.Instance?.OnGoalkeeperPositionSelected("RightKeepSpot"));
        
        // Game over buttons
        ConnectButton("PlayAgainButton", () => GameManager.Instance?.RestartGame());
        ConnectButton("MainMenuButton", () => GameManager.Instance?.ReturnToMainMenu());
        ConnectButton("QuitButton", () => Application.Quit());
        
        // Pause buttons - FIXED by using OnResumeGameClicked
        ConnectButton("ResumeButton", () => GameManager.Instance?.OnResumeGameClicked());
        ConnectButton("SettingsButton", () => Debug.Log("Settings clicked"));
        
        // Teleport buttons
        ConnectButton("LeftSpotButton", () => GameManager.Instance?.OnTeleportPressed("LeftShooterSpot"));
        ConnectButton("CenterSpotButton", () => GameManager.Instance?.OnTeleportPressed("CenterShooterSpot"));
        ConnectButton("RightSpotButton", () => GameManager.Instance?.OnTeleportPressed("RightShooterSpot"));
        ConnectButton("BenchSpotButton", () => GameManager.Instance?.OnTeleportPressed("BenchSpot"));
        
        Debug.Log("All buttons connected");
    }
    
    void ConnectButton(string buttonName, System.Action action)
    {
        GameObject buttonObj = GameObject.Find(buttonName);
        if (buttonObj != null)
        {
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    Debug.Log($"Button clicked: {buttonName}");
                    action.Invoke();
                });
                Debug.Log($"✓ Connected: {buttonName}");
            }
            else
            {
                Debug.LogError($"Object '{buttonName}' found, but it doesn't have a Button component!");
            }
        }
        else
        {
            Debug.LogWarning($"✗ Button not found: {buttonName}");
        }
    }
}