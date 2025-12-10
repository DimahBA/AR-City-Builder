using UnityEngine;

public class Building : MonoBehaviour
{
    [Header("Building Configuration")]
    public BuildingData buildingData;
    
    private BuildingPlacementValidator placementValidator;
    private GameUI gameUI;
    private bool hasPaid = false;
    private bool isPaymentProcessed = false;
    private bool isRegistered = false;
    
    [Header("House State (Runtime)")]
    private int currentPopulation = 0;
    private float happiness = 100f;
    private int daysUnhappy = 0;
    private bool isAbandoned = false;
    
    // Add this to check if we're actually being tracked
    private DefaultObserverEventHandler trackingHandler;
    private bool isTracked = false;

    void Start()
    {
        if (buildingData == null)
        {
            Debug.Log("this is not a building");
            return;
        }
        
        placementValidator = GetComponent<BuildingPlacementValidator>();
        gameUI = FindObjectOfType<GameUI>();
        
        // Get the tracking handler to know when we're actually tracked
        trackingHandler = GetComponent<DefaultObserverEventHandler>();
        if (trackingHandler == null)
        {
            trackingHandler = GetComponent<CustomTrackableEventHandler>();
        }
    }
    
    public void OnTrackingFound()
    {
        isTracked = true;
        Debug.Log($"[Building] {buildingData?.buildingName} is now being tracked");
    }
    
    public void OnTrackingLost()
    {
        isTracked = false;
        Debug.Log($"[Building] {buildingData?.buildingName} lost tracking");
    }
    
    void Update()
    {
        if (buildingData == null) return;
        
        // ONLY process payment if we're actually being tracked!
        if (!isTracked) return;
        
        // Rest of your existing payment logic...
        if (!hasPaid && placementValidator != null && placementValidator.IsPlacementValid())
        {
            if (gameUI != null && gameUI.CanAfford(buildingData.cost))
            {
                if (!isPaymentProcessed)
                {
                    gameUI.SpendMoney(buildingData.cost);
                    hasPaid = true;
                    isPaymentProcessed = true;
                    Debug.Log($"[Building] Paid ${buildingData.cost} for {buildingData.buildingName}");
                    RegisterWithManager();
                }
            }
        }
    }
    
    void RegisterWithManager()
    {
        if (!isRegistered && BuildingManager.Instance != null)
        {
            Debug.Log($"[Building] Registering {buildingData.buildingName} as type: {buildingData.buildingType}");
            
            BuildingManager.Instance.RegisterBuilding(this);
            isRegistered = true;
            
            // Initialize house population when registered
            if (buildingData.buildingType == BuildingType.House)
            {
                currentPopulation = buildingData.housingCapacity;
                //happiness = buildingData.baseHappiness;
                isAbandoned = false;
                // Add happiness indicator if PopulationManager wants them shown
                if (PopulationManager.Instance != null && PopulationManager.Instance.showHappinessIndicators)
                {
                    if (GetComponent<HappinessIndicator>() == null)
                    {
                        gameObject.AddComponent<HappinessIndicator>();
                    }
                }
                Debug.Log($"[Building] House '{buildingData.buildingName}' initialized with {currentPopulation} residents");
            }
            else
            {
                Debug.Log($"[Building] Non-house building registered: {buildingData.buildingType}");
            }
            
            Debug.Log($"[Building] Successfully registered {buildingData.buildingName} as {buildingData.buildingType}");
        }
        else if (BuildingManager.Instance == null)
        {
            Debug.LogError("[Building] Cannot register - BuildingManager.Instance is NULL! Make sure BuildingManager exists in scene.");
        }
        else if (isRegistered)
        {
            Debug.LogWarning($"[Building] {buildingData.buildingName} is already registered!");
        }
    }
    
    void UnregisterFromManager()
    {
        if (isRegistered && BuildingManager.Instance != null)
        {
            BuildingManager.Instance.UnregisterBuilding(this);
            isRegistered = false;
        }
    }
    
    void OnDestroy()
    {
        // Make sure to unregister when building is destroyed
        UnregisterFromManager();
    }
    
    public bool HasPaid()
    {
        return hasPaid;
    }
    
    public int GetCost()
    {
        return buildingData != null ? buildingData.cost : 0;
    }
    
    public bool IsRegistered()
    {
        return isRegistered;
    }
    
    // House-specific methods
    public int GetCurrentPopulation()
    {
        return isAbandoned ? 0 : currentPopulation;
    }
    
    public float GetHappiness()
    {
        return happiness;
    }
    
    public void SetHappiness(float newHappiness)
    {
        happiness = Mathf.Clamp(newHappiness, 0f, 100f);
    }
    
    public bool IsAbandoned()
    {
        return isAbandoned;
    }
    
    public void CheckAbandonment(float abandonmentThreshold, int daysBeforeAbandonment)
    {
        if (buildingData.buildingType != BuildingType.House) return;
        
        if (happiness < abandonmentThreshold)
        {
            daysUnhappy++;
            
            if (daysUnhappy >= daysBeforeAbandonment && !isAbandoned)
            {
                isAbandoned = true;
                currentPopulation = 0;
                Debug.LogWarning($"[Building] House {buildingData.buildingName} has been ABANDONED after {daysUnhappy} days of low happiness!");
            }
        }
        else
        {
            // Reset counter if happiness recovers
            if (daysUnhappy > 0)
            {
                Debug.Log($"[Building] House {buildingData.buildingName} happiness recovered. Resetting unhappy counter.");
            }
            daysUnhappy = 0;
            
            // Can recover from abandonment if happiness improves
            if (isAbandoned && happiness >= abandonmentThreshold + 10f)
            {
                isAbandoned = false;
                currentPopulation = buildingData.housingCapacity;
                Debug.Log($"[Building] House {buildingData.buildingName} has been RE-OCCUPIED! Population restored.");
            }
        }
    }
}