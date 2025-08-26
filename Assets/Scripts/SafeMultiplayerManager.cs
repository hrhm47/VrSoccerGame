using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class SafeMultiplayerManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject createGamePanel;
    public GameObject joinGamePanel;
    public GameObject waitingRoomPanel;
    public GameObject howToPlayPanel;
    public GameObject nameEntryPanel; // VR name entry keyboard
    
    [Header("UI Input Fields")]
    public TMP_InputField playerNameInput;
    public TMP_InputField joinRoomCodeInput;
    
    [Header("VR Name Entry")]
    public TextMeshProUGUI nameDisplayText; // Shows current name being typed
    public Button[] letterButtons; // A-Z buttons for VR typing
    public Button backspaceButton;
    public Button confirmNameButton;
    public Button clearNameButton;
    
    [Header("UI Text Elements")]
    public TextMeshProUGUI connectionStatusText;
    public TextMeshProUGUI roomCodeDisplayText;
    public TextMeshProUGUI waitingRoomInfoText;
    public TextMeshProUGUI playersInRoomText;
    public TextMeshProUGUI errorMessageText;
    
    [Header("UI Buttons")]
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button startGameButton;
    public Button copyCodeButton;
    
    [Header("Game Settings")]
    public Toggle bestOf5Toggle;
    public Toggle bestOf7Toggle;
    
    [Header("Audio")]
    public AudioClip buttonClickSound;
    public AudioClip errorSound;
    public AudioClip successSound;
    private AudioSource audioSource;
    
    // VR Keyboard state
    private string currentTypedText = "";
    private bool isVRMode = false;
    private TMP_InputField currentTargetInputField; // Which input field is being edited
    private bool isKeyboardOpen = false;
    
    private string currentRoomCode;
    private string playerName;
    
    void Start()
    {
        try
        {
            InitializeManager();
            DebugCanvasSetup();
        }
        catch (Exception e)
        {
            HandleError("Failed to initialize game", e);
        }
    }
    
    void DebugCanvasSetup()
    {
        Canvas[] allCanvases = FindObjectsOfType<Canvas>(true);
        Debug.Log($"Total canvases found: {allCanvases.Length}");
        
        foreach (Canvas canvas in allCanvases)
        {
            Debug.Log($"Canvas: {canvas.name}, Active: {canvas.gameObject.activeInHierarchy}, Parent: {(canvas.transform.parent != null ? canvas.transform.parent.name : "None")}");
        }
    }


    void InitializeManager()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Check if we're in VR mode
        isVRMode = UnityEngine.XR.XRSettings.enabled || Application.platform == RuntimePlatform.Android;
        
        ShowMainMenu();
        UpdateConnectionStatus("Ready to play!", Color.green);
        
        // Setup VR keyboard
        if (isVRMode)
        {
            SetupVRKeyboard();
            SetupInputFieldDetectors();
        }
        
        LoadSavedPlayerName();
    }
    
    void SetupVRKeyboard()
    {
        // Setup letter buttons for VR typing
        if (letterButtons != null && letterButtons.Length > 0)
        {
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            for (int i = 0; i < letterButtons.Length && i < alphabet.Length; i++)
            {
                string letter = alphabet[i].ToString();
                letterButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = letter;
                
                letterButtons[i].onClick.RemoveAllListeners();
                letterButtons[i].onClick.AddListener(() => AddLetterToText(letter));
            }
        }
        
        // Setup special buttons
        if (backspaceButton != null)
        {
            backspaceButton.onClick.RemoveAllListeners();
            backspaceButton.onClick.AddListener(RemoveLastLetter);
        }
        
        if (confirmNameButton != null)
        {
            confirmNameButton.onClick.RemoveAllListeners();
            confirmNameButton.onClick.AddListener(ConfirmKeyboardInput);
        }
        
        if (clearNameButton != null)
        {
            clearNameButton.onClick.RemoveAllListeners();
            clearNameButton.onClick.AddListener(ClearKeyboardText);
        }
    }
    
    void SetupInputFieldDetectors()
    {
        // Find and add detectors to ALL input fields in the scene
        TMP_InputField[] allInputFields = FindObjectsOfType<TMP_InputField>();
        
        foreach (TMP_InputField inputField in allInputFields)
        {
            AddInputFieldDetector(inputField);
            Debug.Log($"Added detector to input field: {inputField.name}");
        }
        
        Debug.Log($"Total input fields detected: {allInputFields.Length}");
    }
    
    void AddInputFieldDetector(TMP_InputField inputField)
    {
        // Add collider if not present
        BoxCollider collider = inputField.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = inputField.gameObject.AddComponent<BoxCollider>();
            RectTransform rectTransform = inputField.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                collider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 50f);
            }
        }
        
        // Add input field detector component
        VRInputFieldDetector detector = inputField.GetComponent<VRInputFieldDetector>();
        if (detector == null)
        {
            detector = inputField.gameObject.AddComponent<VRInputFieldDetector>();
        }
        detector.multiplayerManager = this;
        detector.inputField = inputField;
        
        Debug.Log($"Added detector to input field: {inputField.name}");
    }
    
    // Called by gaze system when input field is gazed at
    public void OnInputFieldGazed(TMP_InputField inputField)
    {
        if (isVRMode && !isKeyboardOpen)
        {
            OpenKeyboardForInputField(inputField);
        }
    }
    
    void OpenKeyboardForInputField(TMP_InputField inputField)
    {
        currentTargetInputField = inputField;
        currentTypedText = inputField.text; // Start with current text
        isKeyboardOpen = true;
        
        // Show keyboard panel
        if (nameEntryPanel != null)
        {
            nameEntryPanel.SetActive(true);
        }
        
        UpdateKeyboardDisplay();
        PlayButtonSound();
        
        Debug.Log($"Opened keyboard for input field: {inputField.name}");
    }
    
    void CloseKeyboard()
    {
        currentTargetInputField = null;
        isKeyboardOpen = false;
        
        if (nameEntryPanel != null)
        {
            nameEntryPanel.SetActive(false);
        }
        
        Debug.Log("Keyboard closed");
    }
    
    // VR Keyboard Methods
    public void AddLetterToText(string letter)
    {
        try
        {
            PlayButtonSound();
            
            if (currentTypedText.Length < 12)
            {
                currentTypedText += letter.ToUpper();
                UpdateKeyboardDisplay();
            }
            else
            {
                ShowErrorMessage("Text cannot be longer than 12 characters");
            }
        }
        catch (Exception e)
        {
            HandleError("Failed to add letter", e);
        }
    }
    
    public void RemoveLastLetter()
    {
        try
        {
            PlayButtonSound();
            
            if (currentTypedText.Length > 0)
            {
                currentTypedText = currentTypedText.Substring(0, currentTypedText.Length - 1);
                UpdateKeyboardDisplay();
            }
        }
        catch (Exception e)
        {
            HandleError("Failed to remove letter", e);
        }
    }
    
    public void ClearKeyboardText()
    {
        try
        {
            PlayButtonSound();
            currentTypedText = "";
            UpdateKeyboardDisplay();
        }
        catch (Exception e)
        {
            HandleError("Failed to clear text", e);
        }
    }
    
    public void ConfirmKeyboardInput()
    {
        try
        {
            PlayButtonSound();
            
            if (ValidateKeyboardInput())
            {
                // Apply text to the target input field
                if (currentTargetInputField != null)
                {
                    currentTargetInputField.text = currentTypedText;
                    
                    // If it's the player name input, also save it
                    if (currentTargetInputField == playerNameInput)
                    {
                        playerName = currentTypedText;
                        PlayerPrefs.SetString("PlayerName", playerName);
                    }
                }
                
                PlaySound(successSound);
                CloseKeyboard();
            }
        }
        catch (Exception e)
        {
            HandleError("Failed to confirm input", e);
        }
    }
    
    bool ValidateKeyboardInput()
    {
        if (string.IsNullOrEmpty(currentTypedText) || currentTypedText.Length < 2)
        {
            ShowErrorMessage("Text must be at least 2 characters");
            return false;
        }
        
        if (currentTypedText.Length > 12)
        {
            ShowErrorMessage("Text cannot be longer than 12 characters");
            return false;
        }
        
        return true;
    }
    
    void UpdateKeyboardDisplay()
    {
        if (nameDisplayText != null)
        {
            nameDisplayText.text = string.IsNullOrEmpty(currentTypedText) ? "Enter text..." : currentTypedText;
        }
    }
    
    void LoadSavedPlayerName()
    {
        try
        {
            string savedName = PlayerPrefs.GetString("PlayerName", "");
            if (!string.IsNullOrEmpty(savedName))
            {
                playerName = savedName;
                
                if (playerNameInput != null)
                    playerNameInput.text = savedName;
            }
        }
        catch (Exception e)
        {
            HandleError("Failed to load saved name", e);
        }
    }
    
    // Original UI Button Functions (keeping all existing functionality)
    public void OnCreateGameClicked()
    {
        try
        {
            PlayButtonSound();
            ShowCreateGamePanel();
        }
        catch (Exception e)
        {
            HandleError("Failed to open create game", e);
        }
    }
    
    public void OnJoinGameClicked()
    {
        try
        {
            PlayButtonSound();
            ShowJoinGamePanel();
        }
        catch (Exception e)
        {
            HandleError("Failed to open join game", e);
        }
    }
    
    public void OnHowToPlayClicked()
    {
        try
        {
            PlayButtonSound();
            ShowHowToPlayPanel();
        }
        catch (Exception e)
        {
            HandleError("Failed to open how to play", e);
        }
    }
    
    public void OnBackToMainMenu()
    {
        try
        {
            PlayButtonSound();
            ShowMainMenu();
            ClearErrorMessage();
            
            // Close keyboard if open
            if (isKeyboardOpen)
            {
                CloseKeyboard();
            }
        }
        catch (Exception e)
        {
            HandleError("Failed to return to main menu", e);
        }
    }
    
    public void OnCreateRoomClicked()
    {
        try
        {
            if (ValidatePlayerName())
            {
                CreateRoom();
            }
        }
        catch (Exception e)
        {
            HandleError("Failed to create room", e);
        }
    }
    
    public void OnJoinRoomClicked()
    {
        try
        {
            if (ValidatePlayerName() && ValidateRoomCode())
            {
                JoinRoom();
            }
        }
        catch (Exception e)
        {
            HandleError("Failed to join room", e);
        }
    }
    public void OnStartGameClicked()
{
    try
    {
        PlayButtonSound();
        
        if (string.IsNullOrEmpty(currentRoomCode))
        {
            ShowErrorMessage("No active room found");
            return;
        }
        
        UpdateConnectionStatus("Starting game...", Color.green);
        
        // Hide ALL menu panels first
        HideAllPanels();
        
        // Make sure Penalty Field is deactivated initially
        GameObject penaltyField = GameObject.Find("PenaltyField");
        if (penaltyField)
        {
            Debug.Log("Ensuring PenaltyField is inactive before game start");
            penaltyField.SetActive(false);
        }
        
        // Hide main menu canvas
        GameObject mainMenuCanvas = GameObject.Find("MainMenuCanvas");
        if (mainMenuCanvas) mainMenuCanvas.SetActive(false);
        
        // Find or CREATE GameCanvas if not found
        GameObject gameCanvasObj = GameObject.Find("GameCanvas");
        if (gameCanvasObj == null)
        {
            Debug.Log("GameCanvas not found - creating one now");
            
            // Create a new GameCanvas
            gameCanvasObj = new GameObject("GameCanvas");
            
            // Add Canvas component
            Canvas gameCanvas = gameCanvasObj.AddComponent<Canvas>();
            gameCanvas.renderMode = RenderMode.WorldSpace;
            
            // Add CanvasScaler
            CanvasScaler scaler = gameCanvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add GraphicRaycaster
            gameCanvasObj.AddComponent<GraphicRaycaster>();
            
            // Create essential UI elements
            CreateGameUI(gameCanvasObj);
        }
        
        // Activate GameCanvas
        gameCanvasObj.SetActive(true);
        
        // Make teleport UI visible and move it to the front
        GameObject teleportUI = GameObject.Find("TeleportUI");
        if (teleportUI)
        {
            teleportUI.SetActive(true);
            
            // Set teleport UI to be the last sibling (front-most)
            teleportUI.transform.SetAsLastSibling();
            
            // Ensure buttons are visible by adjusting color contrast
            UpdateTeleportButtonsVisibility(teleportUI);
        }
        else
        {
            Debug.LogWarning("TeleportUI not found - creating simple teleport UI");
            CreateTeleportUI(gameCanvasObj);
        }
        
        Debug.Log("GameCanvas activated successfully");
        
        // Wait a frame for GameManager to initialize, then start game
        StartCoroutine(StartGameDelayed());
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Error in OnStartGameClicked: {e.Message}");
        Debug.LogError($"Stack trace: {e.StackTrace}");
        HandleError("Failed to start game", e);
    }
}



    // Helper method to create essential game UI elements
void CreateGameUI(GameObject parentCanvas)
{
    // Create GameHUD
    GameObject gameHUD = new GameObject("GameHUD");
    gameHUD.transform.SetParent(parentCanvas.transform, false);
    RectTransform gameHUDRect = gameHUD.AddComponent<RectTransform>();
    gameHUDRect.anchorMin = new Vector2(0, 0);
    gameHUDRect.anchorMax = new Vector2(1, 1);
    gameHUDRect.offsetMin = Vector2.zero;
    gameHUDRect.offsetMax = Vector2.zero;
    
    // Create ShooterUI
    GameObject shooterUI = new GameObject("ShooterUI");
    shooterUI.transform.SetParent(parentCanvas.transform, false);
    RectTransform shooterUIRect = shooterUI.AddComponent<RectTransform>();
    shooterUIRect.anchorMin = new Vector2(0, 0);
    shooterUIRect.anchorMax = new Vector2(1, 1);
    shooterUIRect.offsetMin = Vector2.zero;
    shooterUIRect.offsetMax = Vector2.zero;
    
    // Create TeleportUI
    GameObject teleportUI = new GameObject("TeleportUI");
    teleportUI.transform.SetParent(parentCanvas.transform, false);
    RectTransform teleportUIRect = teleportUI.AddComponent<RectTransform>();
    teleportUIRect.anchorMin = new Vector2(0, 0);
    teleportUIRect.anchorMax = new Vector2(1, 1);
    teleportUIRect.offsetMin = Vector2.zero;
    teleportUIRect.offsetMax = Vector2.zero;
    
    // Create GameOverUI
    GameObject gameOverUI = new GameObject("GameOverUI");
    gameOverUI.transform.SetParent(parentCanvas.transform, false);
    RectTransform gameOverUIRect = gameOverUI.AddComponent<RectTransform>();
    gameOverUIRect.anchorMin = new Vector2(0, 0);
    gameOverUIRect.anchorMax = new Vector2(1, 1);
    gameOverUIRect.offsetMin = Vector2.zero;
    gameOverUIRect.offsetMax = Vector2.zero;
    gameOverUI.SetActive(false);
    
    // Create PositionSelectionPhase in ShooterUI
    GameObject positionPhase = new GameObject("PositionSelectionPhase");
    positionPhase.transform.SetParent(shooterUI.transform, false);
    RectTransform positionPhaseRect = positionPhase.AddComponent<RectTransform>();
    positionPhaseRect.anchorMin = new Vector2(0.5f, 0.5f);
    positionPhaseRect.anchorMax = new Vector2(0.5f, 0.5f);
    positionPhaseRect.sizeDelta = new Vector2(800, 600);
    positionPhaseRect.anchoredPosition = Vector2.zero;
    
    // Add position buttons
    CreateButton(positionPhase, "LeftPositionButton", new Vector2(-200, 0), "Left");
    CreateButton(positionPhase, "CenterPositionButton", new Vector2(0, 0), "Center");
    CreateButton(positionPhase, "RightPositionButton", new Vector2(200, 0), "Right");
    
    // Create AimingPhase in ShooterUI
    GameObject aimingPhase = new GameObject("AimingPhase");
    aimingPhase.transform.SetParent(shooterUI.transform, false);
    RectTransform aimingPhaseRect = aimingPhase.AddComponent<RectTransform>();
    aimingPhaseRect.anchorMin = new Vector2(0.5f, 0.5f);
    aimingPhaseRect.anchorMax = new Vector2(0.5f, 0.5f);
    aimingPhaseRect.sizeDelta = new Vector2(800, 600);
    aimingPhaseRect.anchoredPosition = Vector2.zero;
    
    // Add shoot button
    CreateButton(aimingPhase, "ShootButton", new Vector2(0, 0), "SHOOT!");
    
    // Create ResultPhase in ShooterUI
    GameObject resultPhase = new GameObject("ResultPhase");
    resultPhase.transform.SetParent(shooterUI.transform, false);
    RectTransform resultPhaseRect = resultPhase.AddComponent<RectTransform>();
    resultPhaseRect.anchorMin = new Vector2(0.5f, 0.5f);
    resultPhaseRect.anchorMax = new Vector2(0.5f, 0.5f);
    resultPhaseRect.sizeDelta = new Vector2(800, 600);
    resultPhaseRect.anchoredPosition = Vector2.zero;
    
    // Add result text and next round button
    GameObject resultTitle = new GameObject("ResultTitle");
    resultTitle.transform.SetParent(resultPhase.transform, false);
    RectTransform resultTitleRect = resultTitle.AddComponent<RectTransform>();
    resultTitleRect.anchorMin = new Vector2(0.5f, 0.5f);
    resultTitleRect.anchorMax = new Vector2(0.5f, 0.5f);
    resultTitleRect.sizeDelta = new Vector2(400, 100);
    resultTitleRect.anchoredPosition = new Vector2(0, 100);
    TMPro.TextMeshProUGUI resultText = resultTitle.AddComponent<TMPro.TextMeshProUGUI>();
    resultText.text = "GOAL!";
    resultText.fontSize = 72;
    resultText.color = Color.green;
    resultText.alignment = TMPro.TextAlignmentOptions.Center;
    
    CreateButton(resultPhase, "NextRoundButton", new Vector2(0, -100), "NEXT ROUND");
    
    // Initially hide aiming and result phases
    aimingPhase.SetActive(false);
    resultPhase.SetActive(false);
}

// Helper method to create teleport UI
void CreateTeleportUI(GameObject parentCanvas)
{
    GameObject teleportUI = new GameObject("TeleportUI");
    teleportUI.transform.SetParent(parentCanvas.transform, false);
    RectTransform teleportUIRect = teleportUI.AddComponent<RectTransform>();
    teleportUIRect.anchorMin = new Vector2(0.5f, 0);
    teleportUIRect.anchorMax = new Vector2(0.5f, 0);
    teleportUIRect.sizeDelta = new Vector2(800, 150);
    teleportUIRect.anchoredPosition = new Vector2(0, 100);
    
    // Add teleport buttons
    CreateButton(teleportUI, "LeftSpotButton", new Vector2(-300, 0), "LEFT", Color.blue);
    CreateButton(teleportUI, "CenterSpotButton", new Vector2(-100, 0), "CENTER", Color.blue);
    CreateButton(teleportUI, "RightSpotButton", new Vector2(100, 0), "RIGHT", Color.blue);
    CreateButton(teleportUI, "BenchSpotButton", new Vector2(300, 0), "BENCH", Color.blue);
    
    // Move to front
    teleportUI.transform.SetAsLastSibling();
}

// Helper method to create a button
GameObject CreateButton(GameObject parent, string name, Vector2 position, string text, Color? color = null)
{
    GameObject buttonObj = new GameObject(name);
    buttonObj.transform.SetParent(parent.transform, false);
    
    RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
    rectTransform.sizeDelta = new Vector2(180, 80);
    rectTransform.anchoredPosition = position;
    
    // Add image component (button background)
    Image image = buttonObj.AddComponent<Image>();
    image.color = color ?? new Color(0.2f, 0.2f, 0.2f, 1f);
    
    // Add button component
    Button button = buttonObj.AddComponent<Button>();
    ColorBlock colors = button.colors;
    colors.normalColor = color ?? new Color(0.2f, 0.2f, 0.2f, 1f);
    colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    button.colors = colors;
    
    // Add text
    GameObject textObj = new GameObject("Text");
    textObj.transform.SetParent(buttonObj.transform, false);
    
    RectTransform textRectTransform = textObj.AddComponent<RectTransform>();
    textRectTransform.anchorMin = Vector2.zero;
    textRectTransform.anchorMax = Vector2.one;
    textRectTransform.offsetMin = Vector2.zero;
    textRectTransform.offsetMax = Vector2.zero;
    
    TMPro.TextMeshProUGUI tmpText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
    tmpText.text = text;
    tmpText.fontSize = 24;
    tmpText.color = Color.white;
    tmpText.alignment = TMPro.TextAlignmentOptions.Center;
    tmpText.fontStyle = TMPro.FontStyles.Bold;
    
    return buttonObj;
}



    void UpdateTeleportButtonsVisibility(GameObject teleportUI)
    {
        // Find all buttons in the teleport UI
        UnityEngine.UI.Button[] buttons = teleportUI.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        
        foreach (UnityEngine.UI.Button button in buttons)
        {
            // Get the button background image component
            UnityEngine.UI.Image buttonImage = button.GetComponent<UnityEngine.UI.Image>();
            if (buttonImage)
            {
                // Set a more visible color (darker blue)
                buttonImage.color = new Color(0.2f, 0.3f, 0.8f, 1f);
                
                // Make button larger
                RectTransform rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform)
                {
                    // Increase scale by 20%
                    rectTransform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                }
            }
            
            // Get the button text component
            TMPro.TextMeshProUGUI buttonText = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText)
            {
                // Set to white text
                buttonText.color = Color.white;
                // Make text bold
                buttonText.fontStyle = TMPro.FontStyles.Bold;
                // Increase font size
                buttonText.fontSize += 4;
            }
        }
        
        Debug.Log("Teleport buttons visibility improved");
    }
    




    System.Collections.IEnumerator StartGameDelayed()
    {
        yield return null; // Wait one frame
        
        // Make sure teleport spots are registered
        TeleportationManager teleportManager = FindObjectOfType<TeleportationManager>();
        if (teleportManager)
        {
            teleportManager.RefreshTeleportSpots();
            Debug.Log("Teleport spots refreshed");
            
            // Enable debug spheres to help see teleport locations
            teleportManager.showDebugSpheres = true;
        }
        else
        {
            Debug.LogError("TeleportationManager not found!");
        }
        
        yield return null; // Wait another frame
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame(playerName);
            Debug.Log("Game started successfully");
            
            // Show a helpful message to guide the player
            ShowErrorMessage("Click a position button (Left/Center/Right) to begin!", false);
        }
        else
        {
            Debug.LogError("GameManager.Instance still null after delay!");
            ShowErrorMessage("GameManager not found - check setup");
        }
    }

    public void ReturnToMainMenu()
    {
        try
        {
            // Hide game UI
            GameObject gameCanvas = GameObject.Find("GameCanvas");
            if (gameCanvas != null)
            {
                gameCanvas.SetActive(false);
            }
            
            // Hide penalty field
            GameObject penaltyField = GameObject.Find("PenaltyField");
            if (penaltyField != null)
            {
                penaltyField.SetActive(false);
            }
            
            // Show main menu
            GameObject mainMenuCanvas = GameObject.Find("MainMenuCanvas");
            if (mainMenuCanvas != null)
            {
                mainMenuCanvas.SetActive(true);
            }
            
            // Reset game state
            if (GameManager.Instance != null)
            {
                GameManager.Instance.isGameActive = false;
            }
            
            // Show main menu panel
            ShowMainMenu();
            UpdateConnectionStatus("Ready to play!", Color.green);
            
            Debug.Log("Returned to main menu");
        }
        catch (System.Exception e)
        {
            HandleError("Failed to return to menu", e);
        }
    }


    
    public void OnCopyRoomCodeClicked()
    {
        try
        {
            PlayButtonSound();
            
            if (string.IsNullOrEmpty(currentRoomCode))
            {
                ShowErrorMessage("No room code to copy");
                return;
            }
            
            GUIUtility.systemCopyBuffer = currentRoomCode;
            Debug.Log($"Room code copied: {currentRoomCode}");
            
            StartCoroutine(ShowCopyFeedback());
        }
        catch (Exception e)
        {
            HandleError("Failed to copy room code", e);
        }
    }
    
    public void OnLeaveRoomClicked()
    {
        try
        {
            PlayButtonSound();
            Debug.Log("Leaving room...");
            
            currentRoomCode = "";
            ShowMainMenu();
            UpdateConnectionStatus("Left the room", Color.yellow);
            
            Invoke("ClearConnectionStatus", 2f);
        }
        catch (Exception e)
        {
            HandleError("Failed to leave room", e);
        }
    }

    public void SimulateSecondPlayerJoined()
    {
        playersInRoomText.text = "Players: 2/2";
        waitingRoomInfoText.text = "Both players ready!";
        waitingRoomInfoText.color = Color.green;
        startGameButton.gameObject.SetActive(true);
        
        
        Debug.Log("Second player joined the room!");
    }

    
    // Validation methods remain the same
    bool ValidatePlayerName()
    {
        try
        {
            if (isVRMode)
            {
                if (string.IsNullOrEmpty(playerName))
                {
                    ShowErrorMessage("Please enter your name first");
                    return false;
                }
            }
            else
            {
                if (playerNameInput == null)
                {
                    ShowErrorMessage("Name input field not found");
                    return false;
                }
                
                playerName = playerNameInput.text.Trim();
            }
            
            if (string.IsNullOrEmpty(playerName) || playerName.Length < 2)
            {
                ShowErrorMessage("Please enter a name (at least 2 characters)");
                return false;
            }
            
            if (playerName.Length > 12)
            {
                ShowErrorMessage("Name too long (max 12 characters)");
                return false;
            }
            
            return true;
        }
        catch (Exception e)
        {
            HandleError("Failed to validate player name", e);
            return false;
        }
    }
    
    bool ValidateRoomCode()
    {
        try
        {
            if (joinRoomCodeInput == null)
            {
                ShowErrorMessage("Room code input field not found");
                return false;
            }
            
            string code = joinRoomCodeInput.text.Trim().ToUpper();
            
            if (string.IsNullOrEmpty(code))
            {
                ShowErrorMessage("Please enter a room code");
                return false;
            }
            
            if (code.Length != 5)
            {
                ShowErrorMessage("Room code must be exactly 5 characters");
                return false;
            }
            
            return true;
        }
        catch (Exception e)
        {
            HandleError("Failed to validate room code", e);
            return false;
        }
    }
    
    void CreateRoom()
    {
        try
        {
            currentRoomCode = GenerateRoomCode();
            Debug.Log($"Created room: {currentRoomCode}");
            ShowWaitingRoom();
            PlaySound(successSound);
        }
        catch (Exception e)
        {
            HandleError("Failed to create room", e);
        }
    }
    
    void JoinRoom()
    {
        try
        {
            currentRoomCode = joinRoomCodeInput.text.Trim().ToUpper();
            Debug.Log($"Joining room: {currentRoomCode}");
            ShowWaitingRoom();
            
            if (playersInRoomText != null)
                playersInRoomText.text = "Players: 2/2";
            if (waitingRoomInfoText != null)
            {
                waitingRoomInfoText.text = "Both players ready!";
                waitingRoomInfoText.color = Color.green;
            }
            if (startGameButton != null)
                startGameButton.gameObject.SetActive(true);
                
            PlaySound(successSound);
        }
        catch (Exception e)
        {
            HandleError("Failed to join room", e);
        }
    }
    
    string GenerateRoomCode()
    {
        try
        {
            string[] words = { "CAT", "DOG", "SUN", "FUN", "GOAL", "KICK", "WIN", "PLAY", "STAR", "HERO" };
            string word = words[UnityEngine.Random.Range(0, words.Length)];
            int number = UnityEngine.Random.Range(10, 99);
            return $"{word}{number}";
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to generate room code: {e.Message}");
            return "GAME01";
        }
    }
    
    // UI Management methods remain the same
    void ShowMainMenu()
    {
        try
        {
            HideAllPanels();
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
        }
        catch (Exception e)
        {
            HandleError("Failed to show main menu", e);
        }
    }
    
    void ShowCreateGamePanel()
    {
        try
        {
            HideAllPanels();
            if (createGamePanel != null)
                createGamePanel.SetActive(true);
        }
        catch (Exception e)
        {
            HandleError("Failed to show create game panel", e);
        }
    }
    
    void ShowJoinGamePanel()
    {
        try
        {
            HideAllPanels();
            if (joinGamePanel != null)
                joinGamePanel.SetActive(true);
        }
        catch (Exception e)
        {
            HandleError("Failed to show join game panel", e);
        }
    }
    
    void ShowWaitingRoom()
    {
        try
        {
            HideAllPanels();
            if (waitingRoomPanel != null)
                waitingRoomPanel.SetActive(true);
            
            if (roomCodeDisplayText != null)
                roomCodeDisplayText.text = $"Room Code: {currentRoomCode}";
            if (playersInRoomText != null)
                playersInRoomText.text = "Players: 1/2";
            if (waitingRoomInfoText != null)
            {
                waitingRoomInfoText.text = "Waiting for friend to join...";
                waitingRoomInfoText.color = Color.yellow;
            }
            if (startGameButton != null)
                startGameButton.gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            HandleError("Failed to show waiting room", e);
        }
    }
    
    void ShowHowToPlayPanel()
    {
        try
        {
            HideAllPanels();
            if (howToPlayPanel != null)
                howToPlayPanel.SetActive(true);
        }
        catch (Exception e)
        {
            HandleError("Failed to show how to play panel", e);
        }
    }
    
    void HideAllPanels()
    {
        try
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (createGamePanel != null) createGamePanel.SetActive(false);
            if (joinGamePanel != null) joinGamePanel.SetActive(false);
            if (waitingRoomPanel != null) waitingRoomPanel.SetActive(false);
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
            // Don't hide nameEntryPanel here - it's controlled by keyboard logic
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to hide panels: {e.Message}");
        }
    }
    
    // Error handling and utility methods remain the same
    void HandleError(string message, Exception e)
    {
        Debug.LogError($"{message}: {e.Message}");
        ShowErrorMessage($"{message}. Please try again.");
        PlaySound(errorSound);
    }
    
    void ShowErrorMessage(string message, bool isError = true)
    {
        try
        {
            if (errorMessageText != null)
            {
                errorMessageText.text = message;
                errorMessageText.color = isError ? Color.red : Color.green;
                errorMessageText.gameObject.SetActive(true);
                
                Invoke("ClearErrorMessage", 3f);
            }
            
            Debug.Log($"User Message: {message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to show error message: {e.Message}");
        }
    }
    
    void ClearErrorMessage()
    {
        try
        {
            if (errorMessageText != null)
            {
                errorMessageText.text = "";
                errorMessageText.gameObject.SetActive(false);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to clear error message: {e.Message}");
        }
    }
    
    void UpdateConnectionStatus(string message, Color color)
    {
        try
        {
            if (connectionStatusText != null)
            {
                connectionStatusText.text = message;
                connectionStatusText.color = color;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update connection status: {e.Message}");
        }
    }
    
    void ClearConnectionStatus()
    {
        UpdateConnectionStatus("Ready to play!", Color.green);
    }
    
    System.Collections.IEnumerator ShowCopyFeedback()
    {   
        if (copyCodeButton != null)
        {
            var textComponent = copyCodeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                string originalText = textComponent.text;
                textComponent.text = "COPIED!";
                textComponent.color = Color.green;
                
                yield return new WaitForSeconds(2f);
                
                textComponent.text = originalText;
                textComponent.color = Color.white;
            }
        }   
    }
    
    void PlayButtonSound()
    {
        PlaySound(buttonClickSound);
    }
    
    void PlaySound(AudioClip clip)
    {
        try
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to play sound: {e.Message}");
        }
    }
}

// Helper component for input field gaze detection
public class VRInputFieldDetector : MonoBehaviour
{
    [HideInInspector]
    public SafeMultiplayerManager multiplayerManager;
    [HideInInspector]
    public TMP_InputField inputField;
    
    // This will be called by the XRGazeInput when user gazes at this input field
    public void OnGazeClick()
    {
        if (multiplayerManager != null && inputField != null)
        {
            multiplayerManager.OnInputFieldGazed(inputField);
        }
    }
}