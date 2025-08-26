using UnityEngine;
using System.Collections.Generic;

public class TeleportationManager : MonoBehaviour
{
    public static TeleportationManager Instance;
    
    [Header("Teleport Spots")]
    public Transform[] shooterSpots = new Transform[3]; // Left, Center, Right
    public Transform[] keeperSpots = new Transform[3]; // Left, Center, Right for goalkeeper
    public Transform benchSpot;
    
    [Header("Player")]
    public Transform player; // Assign your XRRig here
    
    [Header("Debug")]
    public bool showDebugSpheres = true;
    
    private string currentSpot = "BenchSpot";
    private Dictionary<string, Transform> teleportSpots = new Dictionary<string, Transform>();
    
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
    }
    
    void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject xrRig = GameObject.Find("XRRig");
            if (xrRig != null) 
            {
                player = xrRig.transform;
                Debug.Log("Found XRRig for teleportation");
            }
            else
            {
                // Try main camera as fallback
                player = Camera.main?.transform;
                if (player != null)
                {
                    Debug.Log("Using main camera for teleportation");
                }
                else
                {
                    Debug.LogError("No player/camera found for teleportation!");
                }
            }
        }
        
        // Create teleport spots if they don't exist
        CreateTeleportSpotsIfNeeded();
        RegisterAllTeleportSpots();
        
        Debug.Log("TeleportationManager ready");
    }
    
    void CreateTeleportSpotsIfNeeded()
    {
        // Create parent object if needed
        GameObject spotsParent = GameObject.Find("TeleportSpots");
        if (spotsParent == null)
        {
            spotsParent = new GameObject("TeleportSpots");
            Debug.Log("Created TeleportSpots container");
        }
        
        // Create shooter spots if needed
        if (shooterSpots[0] == null || shooterSpots[1] == null || shooterSpots[2] == null)
        {
            // Left shooter spot
            GameObject leftSpot = GameObject.Find("LeftShooterSpot");
            if (leftSpot == null)
            {
                leftSpot = new GameObject("LeftShooterSpot");
                leftSpot.transform.SetParent(spotsParent.transform);
                leftSpot.transform.position = new Vector3(-3, 0, -8);
            }
            shooterSpots[0] = leftSpot.transform;
            
            // Center shooter spot
            GameObject centerSpot = GameObject.Find("CenterShooterSpot");
            if (centerSpot == null)
            {
                centerSpot = new GameObject("CenterShooterSpot");
                centerSpot.transform.SetParent(spotsParent.transform);
                centerSpot.transform.position = new Vector3(0, 0, -8);
            }
            shooterSpots[1] = centerSpot.transform;
            
            // Right shooter spot
            GameObject rightSpot = GameObject.Find("RightShooterSpot");
            if (rightSpot == null)
            {
                rightSpot = new GameObject("RightShooterSpot");
                rightSpot.transform.SetParent(spotsParent.transform);
                rightSpot.transform.position = new Vector3(3, 0, -8);
            }
            shooterSpots[2] = rightSpot.transform;
            
            Debug.Log("Created shooter spots");
        }
        
        // Create goalkeeper spots if needed
        if (keeperSpots.Length < 3 || keeperSpots[0] == null || keeperSpots[1] == null || keeperSpots[2] == null)
        {
            // Resize array if needed
            if (keeperSpots.Length < 3)
            {
                keeperSpots = new Transform[3];
            }
            
            // Left keeper spot
            GameObject leftKeepSpot = GameObject.Find("LeftKeepSpot");
            if (leftKeepSpot == null)
            {
                leftKeepSpot = new GameObject("LeftKeepSpot");
                leftKeepSpot.transform.SetParent(spotsParent.transform);
                leftKeepSpot.transform.position = new Vector3(-3, 0, 8);
            }
            keeperSpots[0] = leftKeepSpot.transform;
            
            // Center keeper spot
            GameObject centerKeepSpot = GameObject.Find("CenterKeepSpot");
            if (centerKeepSpot == null)
            {
                centerKeepSpot = new GameObject("CenterKeepSpot");
                centerKeepSpot.transform.SetParent(spotsParent.transform);
                centerKeepSpot.transform.position = new Vector3(0, 0, 8);
            }
            keeperSpots[1] = centerKeepSpot.transform;
            
            // Right keeper spot
            GameObject rightKeepSpot = GameObject.Find("RightKeepSpot");
            if (rightKeepSpot == null)
            {
                rightKeepSpot = new GameObject("RightKeepSpot");
                rightKeepSpot.transform.SetParent(spotsParent.transform);
                rightKeepSpot.transform.position = new Vector3(3, 0, 8);
            }
            keeperSpots[2] = rightKeepSpot.transform;
            
            Debug.Log("Created goalkeeper spots");
        }
        
        // Create bench spot if needed
        if (benchSpot == null)
        {
            GameObject bench = GameObject.Find("BenchSpot");
            if (bench == null)
            {
                bench = new GameObject("BenchSpot");
                bench.transform.SetParent(spotsParent.transform);
                bench.transform.position = new Vector3(0, 0, -12);
            }
            benchSpot = bench.transform;
            Debug.Log("Created bench spot");
        }
        
        // Create visual indicators for debugging
        if (showDebugSpheres)
        {
            CreateDebugSpheres();
        }
    }
    
    void RegisterAllTeleportSpots()
    {
        teleportSpots.Clear();
        
        // Register shooter spots
        if (shooterSpots[0] != null) teleportSpots.Add("LeftShooterSpot", shooterSpots[0]);
        if (shooterSpots[1] != null) teleportSpots.Add("CenterShooterSpot", shooterSpots[1]);
        if (shooterSpots[2] != null) teleportSpots.Add("RightShooterSpot", shooterSpots[2]);
        
        // Register goalkeeper spots
        if (keeperSpots[0] != null) teleportSpots.Add("LeftKeepSpot", keeperSpots[0]);
        if (keeperSpots[1] != null) teleportSpots.Add("CenterKeepSpot", keeperSpots[1]);
        if (keeperSpots[2] != null) teleportSpots.Add("RightKeepSpot", keeperSpots[2]);
        
        // Register bench spot
        if (benchSpot != null) teleportSpots.Add("BenchSpot", benchSpot);
        
        Debug.Log($"Registered {teleportSpots.Count} teleport spots");
    }
    
    public bool TeleportToSpot(string spotName)
    {
        Debug.Log($"Attempting to teleport to: {spotName}");
        
        if (player == null)
        {
            Debug.LogError("TeleportationManager: Player not found!");
            return false;
        }
        
        Transform targetSpot = null;
        
        // First check dictionary
        if (teleportSpots.TryGetValue(spotName, out targetSpot))
        {
            // Spot found in dictionary
            Debug.Log($"Found spot {spotName} in dictionary");
        }
        else
        {
            // Try to find it in the array
            targetSpot = GetSpotTransformFromArrays(spotName);
            
            if (targetSpot != null)
            {
                Debug.Log($"Found spot {spotName} in arrays");
            }
            else
            {
                // If still not found, try to find by name
                GameObject spot = GameObject.Find(spotName);
                if (spot != null)
                {
                    targetSpot = spot.transform;
                    // Add to dictionary for future use
                    teleportSpots[spotName] = targetSpot;
                    Debug.Log($"Found spot {spotName} by GameObject.Find");
                }
                else
                {
                    Debug.LogError($"Teleport spot '{spotName}' not found!");
                    LogAvailableTeleportSpots();
                    return false;
                }
            }
        }
        
        if (targetSpot != null)
        {
            // Set position
            Vector3 targetPos = targetSpot.position;
            
            // Preserve player's height if needed
            targetPos.y = player.position.y;
            
            // Move player
            player.position = targetPos;
            currentSpot = spotName;
            
            Debug.Log($"Successfully teleported to {spotName} at {targetPos}");
            return true;
        }
        
        return false;
    }
    
    Transform GetSpotTransformFromArrays(string spotName)
    {
        switch (spotName)
        {
            case "LeftShooterSpot": 
                return shooterSpots[0];
            case "CenterShooterSpot": 
                return shooterSpots[1];
            case "RightShooterSpot": 
                return shooterSpots[2];
            case "LeftKeepSpot": 
                return keeperSpots[0];
            case "CenterKeepSpot": 
                return keeperSpots[1];
            case "RightKeepSpot": 
                return keeperSpots[2];
            case "BenchSpot": 
                return benchSpot;
            default: 
                return null;
        }
    }
    
    void CreateDebugSpheres()
    {
        // Clear existing debug spheres
        GameObject[] existingSpheres = GameObject.FindGameObjectsWithTag("DebugSphere");
        foreach (GameObject sphere in existingSpheres)
        {
            Destroy(sphere);
        }
        
        // Create for shooter spots
        for (int i = 0; i < shooterSpots.Length; i++)
        {
            if (shooterSpots[i] != null)
            {
                CreateDebugSphere(shooterSpots[i].position, 0.2f, Color.blue);
            }
        }
        
        // Create for keeper spots
        for (int i = 0; i < keeperSpots.Length; i++)
        {
            if (keeperSpots[i] != null)
            {
                CreateDebugSphere(keeperSpots[i].position, 0.2f, Color.red);
            }
        }
        
        // Create for bench spot
        if (benchSpot != null)
        {
            CreateDebugSphere(benchSpot.position, 0.2f, Color.green);
        }
    }
    
    void CreateDebugSphere(Vector3 position, float radius, Color color)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * radius * 2f;
        
        // Try to add the tag (create it if it doesn't exist)
        try {
            sphere.tag = "DebugSphere";
        } catch {
            Debug.Log("DebugSphere tag not found - visual spheres will still work");
        }
        
        // Set material
        Renderer renderer = sphere.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            renderer.material = material;
        }
        
        // Make it a child of this object for organization
        sphere.transform.parent = this.transform;
        sphere.name = "DebugSphere_" + position.ToString();
        
        // Remove collider to prevent interference
        Collider collider = sphere.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }
    
    public void LogAvailableTeleportSpots()
    {
        Debug.Log("==== AVAILABLE TELEPORT SPOTS ====");
        
        // Log from arrays
        Debug.Log("Shooter Spots:");
        for (int i = 0; i < shooterSpots.Length; i++)
        {
            if (shooterSpots[i] != null)
            {
                Debug.Log($"- {shooterSpots[i].name}: {shooterSpots[i].position}");
            }
            else
            {
                Debug.Log($"- Shooter spot {i} is null");
            }
        }
        
        Debug.Log("Keeper Spots:");
        for (int i = 0; i < keeperSpots.Length; i++)
        {
            if (keeperSpots[i] != null)
            {
                Debug.Log($"- {keeperSpots[i].name}: {keeperSpots[i].position}");
            }
            else
            {
                Debug.Log($"- Keeper spot {i} is null");
            }
        }
        
        if (benchSpot != null)
        {
            Debug.Log($"Bench Spot: {benchSpot.position}");
        }
        else
        {
            Debug.Log("Bench spot is null");
        }
        
        // Log from dictionary
        Debug.Log("All Registered Spots:");
        foreach (var spot in teleportSpots)
        {
            Debug.Log($"- {spot.Key}: {spot.Value.position}");
        }
        
        Debug.Log("==================================");
    }
    
    public string GetCurrentSpot()
    {
        return currentSpot;
    }
    
    // Call this to refresh spot registration if new spots are added
    public void RefreshTeleportSpots()
    {
        CreateTeleportSpotsIfNeeded();
        RegisterAllTeleportSpots();
        
        if (showDebugSpheres)
        {
            CreateDebugSpheres();
        }
        
        Debug.Log("Teleport spots refreshed");
    }
}