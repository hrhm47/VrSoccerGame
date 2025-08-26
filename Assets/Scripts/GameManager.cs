// ===========================================
// COMPLETE GameManager.cs - ALL PROBLEMS FIXED
// ===========================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public enum PlayerRole
{
    Shooter,
    Goalkeeper
}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game State")]
    public bool isGameActive = false;
    public string currentPhase = "PositionSelection";
    public int currentRound = 1;
    public int maxRounds = 5;
    public float turnTimeLimit = 30f;
    private float turnTimer = 0f;
    
    public PlayerRole currentRole = PlayerRole.Shooter;

    
    [Header("Player Info")]
    public string player1Name = "Player";
    public int playerScore = 0;
    public int aiScore = 0;
    
    [Header("3D Objects")]
    public GameObject ball;
    public GameObject goal;
    public Transform goalCenter;
    
    [Header("Shot Data")]
    public Vector3 shotTarget = Vector3.zero;
    public float shotPower = 75f;
    public string selectedPosition = "";
    
    // UI References
    private Canvas gameCanvas;
    private GameObject gameHUD;
    private GameObject shooterUI;
    private GameObject gameOverUI;
    private GameObject teleportUI;
    
        void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // IMPORTANT: Ensure penalty field is initially inactive
        GameObject penaltyField = GameObject.Find("PenaltyField");
        if (penaltyField) 
        {
            penaltyField.SetActive(false);
            Debug.Log("Penalty field deactivated at startup");
        }
    }
    
    void Start()
    {
        // Find UI components but DO NOT create 3D environment yet
        FindGameCanvas();
        FindUIReferences();
        
        // Just get references if they exist
        ball = GameObject.Find("Ball");
        goal = GameObject.Find("Goal");
        goalCenter = GameObject.Find("GoalCenter")?.transform;
        
        // Connect buttons
        Invoke("ConnectAllButtons", 0.5f);
        
        // Initially game is not active
        isGameActive = false;
        
        Debug.Log("Game initialization complete - waiting for game start");
    }
    
    IEnumerator InitializeGame()
    {
        yield return new WaitForSeconds(0.2f);
        
        FindGameCanvas();
        FindUIReferences();
        Create3DEnvironment();
        ConnectAllButtons();
        
        Debug.Log("Game initialization complete");
    }
    
    void Update()
    {
        if (isGameActive)
        {
            UpdateTimer();
        }
    }
    
    void FindGameCanvas()
    {
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.name == "GameCanvas" && canvas.gameObject.activeInHierarchy)
            {
                gameCanvas = canvas;
                Debug.Log("GameCanvas found");
                return;
            }
        }
    }
    
    void FindUIReferences()
    {
        if (gameCanvas != null)
        {
            gameHUD = FindInChildren(gameCanvas.gameObject, "GameHUD");
            shooterUI = FindInChildren(gameCanvas.gameObject, "ShooterUI");
            teleportUI = FindInChildren(gameCanvas.gameObject, "TeleportUI");
            gameOverUI = FindInChildren(gameCanvas.gameObject, "GameOverUI");
            
            Debug.Log("UI references found");
        }
    }
    
    GameObject FindInChildren(GameObject parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name) return child.gameObject;
        }
        return null;
    }
    
    void Create3DEnvironment()
    {
        // Find or create penalty field
        GameObject penaltyField = GameObject.Find("PenaltyField");
        if (penaltyField == null)
        {
            penaltyField = new GameObject("PenaltyField");
            
            // Create field ground
            GameObject fieldGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
            fieldGround.name = "FieldGround";
            fieldGround.transform.SetParent(penaltyField.transform);
            fieldGround.transform.localPosition = Vector3.zero;
            fieldGround.transform.localScale = new Vector3(2, 1, 3);
            
            // Add material with green color to look like grass
            Renderer fieldRenderer = fieldGround.GetComponent<Renderer>();
            if (fieldRenderer)
            {
                Material fieldMaterial = new Material(Shader.Find("Standard"));
                fieldMaterial.color = new Color(0.1f, 0.6f, 0.1f); // Green color for grass
                fieldRenderer.material = fieldMaterial;
            }
            
            // Create goal
            goal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            goal.name = "Goal";
            goal.transform.SetParent(penaltyField.transform);
            goal.transform.position = new Vector3(0, 1.2f, 10);
            goal.transform.localScale = new Vector3(7, 2.5f, 0.3f);
            
            // Create goal center reference
            GameObject goalCenterObj = new GameObject("GoalCenter");
            goalCenterObj.transform.SetParent(penaltyField.transform);
            goalCenterObj.transform.position = new Vector3(0, 1.2f, 10);
            goalCenter = goalCenterObj.transform;
            
            // Create ball
            ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Ball";
            ball.transform.SetParent(penaltyField.transform);
            ball.transform.position = new Vector3(0, 0.15f, -8);
            ball.transform.localScale = Vector3.one * 0.3f;
            
            // Add Rigidbody to ball
            Rigidbody ballRb = ball.AddComponent<Rigidbody>();
            ballRb.useGravity = true;
            ballRb.mass = 0.5f;
            
            Debug.Log("3D Environment created");
        }
        else
        {
            // Activate existing penalty field
            penaltyField.SetActive(true);
            Debug.Log("Existing penalty field activated");
            
            // Find existing objects
            ball = GameObject.Find("Ball");
            goal = GameObject.Find("Goal");
            goalCenter = GameObject.Find("GoalCenter")?.transform;
            if (goalCenter == null && goal != null) goalCenter = goal.transform;
        }
        
        // Refresh teleport spots after environment is created
        if (TeleportationManager.Instance != null)
        {
            TeleportationManager.Instance.RefreshTeleportSpots();
        }
    }

    
    void ConnectAllButtons()
    {
        // Connect ShooterUI position buttons - PROBLEM 1 & 2 SOLVED
        ConnectButton("LeftPositionButton", () => OnPositionSelected("Left"));
        ConnectButton("CenterPositionButton", () => OnPositionSelected("Center"));
        ConnectButton("RightPositionButton", () => OnPositionSelected("Right"));
        
        // Connect shoot button - PROBLEM 3 SOLVED
        ConnectButton("ShootButton", () => OnShootPressed());
        
        // Connect next round button
        ConnectButton("NextRoundButton", () => OnNextRound());
        
        // Connect teleport buttons
        ConnectButton("LeftSpotButton", () => OnTeleportPressed("Left"));
        ConnectButton("CenterSpotButton", () => OnTeleportPressed("Center"));
        ConnectButton("RightSpotButton", () => OnTeleportPressed("Right"));
        ConnectButton("BenchSpotButton", () => OnTeleportPressed("Bench"));
        
        // Connect game over buttons
        ConnectButton("PlayAgainButton", () => RestartGame());
        ConnectButton("MainMenuButton", () => ReturnToMainMenu());
        
        Debug.Log("All buttons connected successfully");
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
        }
    }

    public void OnGoalkeeperPositionSelected(string position)
    {
        selectedPosition = position.Replace("KeepSpot", ""); // Extract "Left"/"Center"/"Right"
        Debug.Log($"🧤 Goalkeeper position selected: {selectedPosition}");

        // Teleport player to chosen goalkeeper spot
        TeleportationManager.Instance?.TeleportToSpot(position);

        // Switch to result phase to show outcome
        GameObject aimingPhase = FindInChildren(shooterUI, "AimingPhase");
        GameObject resultPhase = FindInChildren(shooterUI, "ResultPhase");
        
        if (aimingPhase) aimingPhase.SetActive(false);
        if (resultPhase) resultPhase.SetActive(true);

        // Trigger AI shot
        StartCoroutine(ExecuteAIShot());
        
        currentPhase = "ShotInProgress";
        UpdateHUD();
    }

    public void OnResumeGameClicked()
    {
        Debug.Log("▶️ Resume button pressed - Returning to gameplay");

        // Hide pause menu
        GameObject pauseMenu = GameObject.Find("PauseMenu");
        if (pauseMenu) pauseMenu.SetActive(false);

        // Resume game state
        isGameActive = true;
        Time.timeScale = 1f; // In case game was paused via time scaling

        // Update status
        UpdateHUD();
        Debug.Log("Game resumed");
    }


    
    // AI shot execution logic for goalkeeper mode
    IEnumerator ExecuteAIShot()
    {
        yield return new WaitForSeconds(0.5f);

        // AI randomly chooses shot direction
        string[] directions = { "Left", "Center", "Right" };
        string aiShot = directions[Random.Range(0, directions.Length)];
        Debug.Log($"🤖 AI shoots to: {aiShot}");

        // Set shot target
        switch (aiShot)
        {
            case "Left":
                shotTarget = new Vector3(-2.5f, 1f, 10f);
                break;
            case "Center":
                shotTarget = new Vector3(0f, 1.2f, 10f);
                break;
            case "Right":
                shotTarget = new Vector3(2.5f, 1f, 10f);
                break;
        }

        // Add randomness
        shotTarget += new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.2f, 0.2f), 0);

        // Animate ball
        yield return StartCoroutine(AnimateBall());

        // Determine if player saved it
        bool isGoal = !IsPlayerInCorrectPosition(aiShot);
        ShowGoalkeeperResult(isGoal);
    }


    // Check if player positioned correctly to save
    bool IsPlayerInCorrectPosition(string aiShotDirection)
    {
        return selectedPosition == aiShotDirection;
    }

    // Show the result of the goalkeeper save attempt
    void ShowGoalkeeperResult(bool isGoal)
    {
        currentPhase = "Result";

        // Update score
        if (isGoal)
        {
            aiScore++;
            Debug.Log("🥅 AI scored! Player failed to save.");
        }
        else
        {
            Debug.Log("🧤 Player saved the shot!");
        }

        // Update result UI
        TextMeshProUGUI resultTitle = GameObject.Find("ResultTitle")?.GetComponent<TextMeshProUGUI>();
        if (resultTitle)
        {
            resultTitle.text = isGoal ? "GOAL!" : "SAVE!";
            resultTitle.color = isGoal ? Color.red : Color.cyan;
        }

        // Proceed to next round
        StartCoroutine(AIShootAfterDelay());
        UpdateHUD();
    }

    
    public void StartGame(string playerName)
    {
        this.player1Name = playerName;
        playerScore = 0;
        aiScore = 0;
        currentRound = 1;
        isGameActive = true;
        turnTimer = 0f;
        
        // Create/activate 3D environment NOW, not before
        Create3DEnvironment();
        ShowGameUI();
        StartShootingPhase();
        
        Debug.Log($"Game started - {playerName} vs AI");
    }
    
        void ShowGameUI()
    {
        if (gameHUD) gameHUD.SetActive(true);
        if (shooterUI) shooterUI.SetActive(true);
        if (teleportUI) teleportUI.SetActive(true);
        if (gameOverUI) gameOverUI.SetActive(false);
        
        // Make TeleportUI visible by bringing it to front
        if (teleportUI && teleportUI.transform.parent != null)
        {
            teleportUI.transform.SetAsLastSibling();
            MakeTeleportButtonsMoreVisible();
        }
        
        Debug.Log("Game UI shown and teleport buttons enhanced");
    }

    
    void MakeTeleportButtonsMoreVisible()
    {
        Button[] buttons = teleportUI.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            // Make background more visible
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage)
            {
                buttonImage.color = new Color(0.2f, 0.3f, 0.8f, 1f); // Darker blue
            }
            
            // Make text more visible
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText)
            {
                buttonText.color = Color.white;
                buttonText.fontStyle = FontStyles.Bold;
                buttonText.fontSize += 4; // Increase size
            }
            
            // Make button bigger
            RectTransform rt = button.GetComponent<RectTransform>();
            if (rt)
            {
                rt.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            }
        }
    }


    
    void StartShootingPhase()
    {
        currentPhase = "PositionSelection";
        turnTimer = 0f;
        selectedPosition = "";
        
        // Show position selection phase
        GameObject positionPhase = FindInChildren(shooterUI, "PositionSelectionPhase");
        GameObject aimingPhase = FindInChildren(shooterUI, "AimingPhase");
        GameObject resultPhase = FindInChildren(shooterUI, "ResultPhase");
        
        if (positionPhase) positionPhase.SetActive(true);
        if (aimingPhase) aimingPhase.SetActive(false);
        if (resultPhase) resultPhase.SetActive(false);
        
        UpdateHUD();
        
        Debug.Log("Position selection phase - Click Left/Center/Right position buttons");
    }
    
    // PROBLEM 1 & 2 SOLVED - Position buttons now work
    public void OnPositionSelected(string position)
    {
        selectedPosition = position;
        Debug.Log($"Position selected: {position}");
        
        // Teleport player to shooting position
        string spotName = position + "ShooterSpot";
        TeleportationManager.Instance?.TeleportToSpot(spotName);
        
        // Switch to aiming phase - PROBLEM 3 SOLVED
        GameObject positionPhase = FindInChildren(shooterUI, "PositionSelectionPhase");
        GameObject aimingPhase = FindInChildren(shooterUI, "AimingPhase");
        
        if (positionPhase) positionPhase.SetActive(false);
        if (aimingPhase) aimingPhase.SetActive(true);
        
        currentPhase = "Aiming";
        turnTimer = 0f; // Reset timer for aiming
        
        UpdateHUD();
        Debug.Log("Aiming phase - Click SHOOT button to score");
    }
    
    // PROBLEM 3 SOLVED - Shooting now works
    public void OnShootPressed()
    {
        Debug.Log("SHOOT button pressed - Taking shot!");
        
        // Calculate shot based on selected position
        CalculateShotParameters();
        
        // Execute shot
        StartCoroutine(ExecuteShot());
    }
    
    void CalculateShotParameters()
    {
        // Set shot target based on selected position
        switch (selectedPosition)
        {
            case "Left":
                shotTarget = new Vector3(-2.5f, 1f, 10f);
                shotPower = Random.Range(70f, 90f);
                break;
            case "Center":
                shotTarget = new Vector3(0f, 1.2f, 10f);
                shotPower = Random.Range(80f, 95f);
                break;
            case "Right":
                shotTarget = new Vector3(2.5f, 1f, 10f);
                shotPower = Random.Range(70f, 90f);
                break;
            default:
                shotTarget = new Vector3(0f, 1f, 10f);
                shotPower = 75f;
                break;
        }
        
        // Add some randomness
        shotTarget += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.3f, 0.3f), 0);
        
        Debug.Log($"Shot calculated - Target: {shotTarget}, Power: {shotPower}");
    }
    
    IEnumerator ExecuteShot()
    {
        currentPhase = "ShotInProgress";
        
        // Switch to result phase
        GameObject aimingPhase = FindInChildren(shooterUI, "AimingPhase");
        GameObject resultPhase = FindInChildren(shooterUI, "ResultPhase");
        
        if (aimingPhase) aimingPhase.SetActive(false);
        if (resultPhase) resultPhase.SetActive(true);
        
        // Animate ball flying to goal
        yield return StartCoroutine(AnimateBall());
        
        // PROBLEM 5 SOLVED - AI opponent decides save/goal
        bool isGoal = DetermineGoalWithAI();
        ShowResult(isGoal);
    }
    
   IEnumerator AnimateBall()
{
    if (ball == null)
    {
        Debug.LogError("Ball is null! Cannot animate.");
        yield break;
    }

    Vector3 startPos = ball.transform.position;
    Vector3 endPos = shotTarget;

    Debug.Log($"Ball flying from {startPos} to {endPos}");

    float duration = 1.5f;
    float elapsed = 0f;

    Rigidbody ballRb = ball.GetComponent<Rigidbody>();
    if (ballRb != null)
    {
        ballRb.isKinematic = true; // Prevent physics during animation
    }

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float progress = elapsed / duration;

        // Parabolic arc
        Vector3 currentPos = Vector3.Lerp(startPos, endPos, progress);
        currentPos.y += Mathf.Sin(progress * Mathf.PI) * 2f;

        ball.transform.position = currentPos;
        yield return null;
    }

    // Final position
    ball.transform.position = endPos;

    // Wait briefly before resetting
    yield return new WaitForSeconds(0.5f);
    ball.transform.position = startPos;

    // Re-enable physics
    if (ballRb != null)
    {
        ballRb.isKinematic = false;
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
    }
}
    
    // PROBLEM 5 SOLVED - AI goalkeeper logic
    bool DetermineGoalWithAI()
    {
        // AI goalkeeper prediction logic
        string aiPrediction = GetAIPrediction();
        
        // Check if AI predicted correctly
        bool correctPrediction = IsCorrectPrediction(aiPrediction);
        
        // Determine result based on shot power and AI prediction
        bool isGoal;
        
        if (correctPrediction)
        {
            // AI predicted correctly - goal only if shot is very powerful or precise
            isGoal = shotPower > 85f || Random.Range(0f, 1f) < 0.3f;
        }
        else
        {
            // AI predicted wrong - high chance of goal
            isGoal = Random.Range(0f, 1f) < 0.8f;
        }
        
        Debug.Log($"AI predicted: {aiPrediction}, Correct: {correctPrediction}, Result: {(isGoal ? "GOAL" : "SAVE")}");
        
        return isGoal;
    }
    
    string GetAIPrediction()
    {
        // Simple AI: predict based on player's selected position with some randomness
        float randomFactor = Random.Range(0f, 1f);
        
        if (selectedPosition == "Left" && randomFactor < 0.6f) return "Left";
        if (selectedPosition == "Right" && randomFactor < 0.6f) return "Right";
        if (selectedPosition == "Center" && randomFactor < 0.5f) return "Center";
        
        // Random prediction if AI doesn't guess correctly
        string[] predictions = { "Left", "Center", "Right" };
        return predictions[Random.Range(0, predictions.Length)];
    }
    
    bool IsCorrectPrediction(string aiPrediction)
    {
        // Check if AI prediction matches shot direction
        Vector3 shotDirection = shotTarget - goalCenter.position;
        
        if (shotDirection.x < -1f && aiPrediction == "Left") return true;
        if (shotDirection.x > 1f && aiPrediction == "Right") return true;
        if (Mathf.Abs(shotDirection.x) <= 1f && aiPrediction == "Center") return true;
        
        return false;
    }
    
    void ShowResult(bool isGoal)
    {
        currentPhase = "Result";
        
        // Update score
        if (isGoal)
        {
            playerScore++;
            Debug.Log("GOAL! Player scored!");
        }
        else
        {
            Debug.Log("SAVED! AI goalkeeper made a save!");
        }
        
        // Update result UI
        TextMeshProUGUI resultTitle = GameObject.Find("ResultTitle")?.GetComponent<TextMeshProUGUI>();
        if (resultTitle)
        {
            resultTitle.text = isGoal ? "GOAL!" : "SAVED!";
            resultTitle.color = isGoal ? Color.green : Color.red;
        }
        
        // AI takes a shot after player
        StartCoroutine(AIShootAfterDelay());
        
        UpdateHUD();
    }
    
    IEnumerator AIShootAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        
        // Simple AI shot
        bool aiGoal = Random.Range(0f, 1f) < 0.5f; // 50% chance
        
        if (aiGoal)
        {
            aiScore++;
            Debug.Log("AI scored!");
        }
        else
        {
            Debug.Log("Player saved AI shot!");
        }
        
        // Auto advance to next round
        yield return new WaitForSeconds(1f);
        OnNextRound();
    }
    
    public void OnNextRound()
    {
        Debug.Log($"Next round - Round {currentRound}/{maxRounds}");
        
        if (currentRound >= maxRounds)
        {
            EndGame();
        }
        else
        {
            currentRound++;
            StartShootingPhase();
        }
    }
    
    void EndGame()
    {
        isGameActive = false;
        
        // Hide shooting UI, show game over
        if (shooterUI) shooterUI.SetActive(false);
        if (gameOverUI) gameOverUI.SetActive(true);
        
        // Determine winner
        string winner = playerScore > aiScore ? player1Name : 
                       aiScore > playerScore ? "AI" : "TIE";
        
        // Update game over UI
        TextMeshProUGUI winnerText = GameObject.Find("WinnerTitle")?.GetComponent<TextMeshProUGUI>();
        if (winnerText)
        {
            winnerText.text = winner == "TIE" ? "IT'S A TIE!" : $"{winner} WINS!";
            winnerText.color = winner == player1Name ? Color.green : 
                              winner == "AI" ? Color.red : Color.yellow;
        }
        
        TextMeshProUGUI scoreText = GameObject.Find("FinalScore")?.GetComponent<TextMeshProUGUI>();
        if (scoreText) scoreText.text = $"FINAL SCORE: {playerScore}-{aiScore}";
        
        Debug.Log($"Game Over! {winner} wins! Score: {playerScore}-{aiScore}");
    }
    
    
    public void OnTeleportPressed(string spotName)
    {
        Debug.Log($"Teleport requested to: {spotName}");
        
        if (TeleportationManager.Instance == null)
        {
            Debug.LogError("TeleportationManager instance is null!");
            return;
        }
        
        // Log all available teleport spots
        TeleportationManager.Instance.LogAvailableTeleportSpots();
        
        // Try to teleport
        bool success = TeleportationManager.Instance.TeleportToSpot(spotName);
        
        if (success)
        {
            Debug.Log($"Successfully teleported to {spotName}");
        }
        else
        {
            Debug.LogError($"Failed to teleport to {spotName}");
        }
    }
    
    void UpdateTimer()
    {
        turnTimer += Time.deltaTime;
        float remaining = Mathf.Max(0f, turnTimeLimit - turnTimer);
        
        TextMeshProUGUI timerText = GameObject.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
        if (timerText)
        {
            int minutes = Mathf.FloorToInt(remaining / 60);
            int seconds = Mathf.FloorToInt(remaining % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
            
            if (remaining < 5f) timerText.color = Color.red;
            else if (remaining < 10f) timerText.color = Color.yellow;
            else timerText.color = Color.white;
        }
        
        // Auto-advance on timeout
        if (remaining <= 0f)
        {
            if (currentPhase == "PositionSelection") OnPositionSelected("Center");
            else if (currentPhase == "Aiming") OnShootPressed();
        }
    }
    
    void UpdateHUD()
    {
        // Update player names and scores
        TextMeshProUGUI p1Name = GameObject.Find("Player1Name")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI p1Score = GameObject.Find("Player1Score")?.GetComponent<TextMeshProUGUI>();
        if (p1Name) p1Name.text = player1Name;
        if (p1Score) p1Score.text = playerScore.ToString();
        
        TextMeshProUGUI p2Name = GameObject.Find("Player2Name")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI p2Score = GameObject.Find("Player2Score")?.GetComponent<TextMeshProUGUI>();
        if (p2Name) p2Name.text = "AI";
        if (p2Score) p2Score.text = aiScore.ToString();
        
        // Update round
        TextMeshProUGUI roundText = GameObject.Find("RoundText")?.GetComponent<TextMeshProUGUI>();
        if (roundText) roundText.text = $"ROUND {currentRound} OF {maxRounds}";
        
        // Update role and status
        // In UpdateHUD()
        TextMeshProUGUI roleText = GameObject.Find("RoleText")?.GetComponent<TextMeshProUGUI>();
        if (roleText) 
            roleText.text = currentRole.ToString().ToUpper(); // Shows "SHOOTER" or "GOALKEEPER"
        // TextMeshProUGUI roleText = GameObject.Find("RoleText")?.GetComponent<TextMeshProUGUI>();
        // if (roleText) roleText.text = "SHOOTER";
        
        TextMeshProUGUI statusText = GameObject.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
        if (statusText) statusText.text = GetStatusText();
    }
    
    string GetStatusText()
    {
        switch (currentPhase)
        {
            case "PositionSelection": return "Choose shooting position";
            case "Aiming": return "Click SHOOT to score";
            case "ShotInProgress": return "Shot in progress...";
            case "Result": return "Round complete";
            default: return "Playing";
        }
    }
    
    public void RestartGame()
    {
        StartGame(player1Name);
    }
    
    public void ReturnToMainMenu()
    {
        if (gameCanvas) gameCanvas.gameObject.SetActive(false);
        GameObject penaltyField = GameObject.Find("PenaltyField");
        if (penaltyField) penaltyField.SetActive(false);
        
        SafeMultiplayerManager manager = FindObjectOfType<SafeMultiplayerManager>();
        if (manager) manager.ReturnToMainMenu();
    }
}


