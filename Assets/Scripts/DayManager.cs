using System.Collections.Generic;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    [Header("Day Configuration")]
    [Tooltip("Length of one day in seconds")]
    public float dayLengthInSeconds = 10f;
    
    [Header("References")]
    public GameUI gameUI;
    
    private int currentDay = 0;
    private float dayTimer = 0f;
    private bool isRunning = false;
    
    void Start()
    {
        if (gameUI == null)
        {
            gameUI = FindObjectOfType<GameUI>();
        }
        
        StartDaySystem();
    }
    
    void Update()
    {
        if (!isRunning) return;

        dayTimer += Time.deltaTime;

        if (gameUI != null)
            gameUI.UpdateDayProgress(GetDayProgress());

        if (dayTimer >= dayLengthInSeconds)
        {
            ProcessDayEnd();
            dayTimer = 0f;
        }
    }

    
    public void StartDaySystem()
    {
        isRunning = true;
        Debug.Log("Day system started");
    }
    
    public void StopDaySystem()
    {
        isRunning = false;
        Debug.Log("Day system stopped");
    }
    
    void ProcessDayEnd()
    {
        currentDay++;
        Debug.Log($"=== Day {currentDay} started ===");
        
        // Process systems in order:
        
        // 1. Process service operating costs (before happiness calculation!)
        if (ServiceManager.Instance != null)
        {
            ServiceManager.Instance.ProcessDailyServiceCosts();
        }
        
        // 2. Update house happiness based on ACTIVE services and factories
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.UpdateHappiness();
        }
        
        // 3. Update population count (after happiness/abandonment checks)
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.UpdatePopulation();
        }
        
        // 4. Process commercial buildings with smart income formula
        ProcessCommercialBuildings();
        
        // 5. Update UI with current day
        if (gameUI != null)
        {
            gameUI.UpdateDayDisplay(currentDay);
        }
        
        Debug.Log($"Day {currentDay} complete. Current money: ${gameUI.GetCurrentMoney()}");
    }

    
    void ProcessCommercialBuildings()
    {
        if (BuildingManager.Instance == null)
        {
            Debug.LogError("[DayManager] BuildingManager.Instance is NULL! Cannot process buildings.");
            return;
        }
        
        if (PopulationManager.Instance == null)
        {
            Debug.LogError("[DayManager] PopulationManager.Instance is NULL! Cannot calculate commercial income.");
            return;
        }
        
        Debug.Log($"[DayManager] Processing commercial buildings...");
        
        // Get all commercial buildings and factories
        List<Building> commercialBuildings = BuildingManager.Instance.GetBuildingsByType(BuildingType.Commercial);
        List<Building> factories = BuildingManager.Instance.GetBuildingsByType(BuildingType.Factory);
        
        Debug.Log($"[DayManager] Found {commercialBuildings.Count} commercial buildings and {factories.Count} factories in registry");
        
        // Check if there are enough factories for commercial buildings
        if (factories.Count == 0)
        {
            Debug.LogWarning("[DayManager] No factories exist! Commercial buildings cannot generate income.");
            return;
        }
        
        // Only process commercial buildings up to the number of factories
        int commercialBuildingsToProcess = Mathf.Min(commercialBuildings.Count, factories.Count);
        
        if (commercialBuildingsToProcess < commercialBuildings.Count)
        {
            Debug.LogWarning($"[DayManager] Only {factories.Count} factory(ies) for {commercialBuildings.Count} commercial building(s). " +
                        $"Only {commercialBuildingsToProcess} commercial building(s) will generate income.");
        }
        
        int totalIncome = 0;
        int processedCount = 0;
        
        foreach (Building building in commercialBuildings)
        {
            if (building == null || building.buildingData == null) 
            {
                Debug.LogWarning("[DayManager] Skipping null building or building with null data");
                continue;
            }
            
            // Only process up to the number of factories
            if (processedCount >= factories.Count)
            {
                Debug.Log($"[DayManager] {building.buildingData.buildingName} cannot generate income - no factory available");
                continue;
            }
            
            int income = CalculateCommercialIncome(building);
            
            if (income > 0)
            {
                gameUI.AddMoney(income);
                totalIncome += income;
                processedCount++;
                Debug.Log($"[DayManager] {building.buildingData.buildingName} generated ${income} (using factory {processedCount}/{factories.Count})");
            }
            else
            {
                Debug.Log($"[DayManager] {building.buildingData.buildingName} generated $0 (insufficient population or happiness)");
                // Note: This still counts as using up a factory slot
                processedCount++;
            }
        }
        
        if (commercialBuildings.Count > 0)
        {
            Debug.Log($"[DayManager] Total commercial buildings: {commercialBuildings.Count}. " +
                    $"Factories available: {factories.Count}. " +
                    $"Commercial buildings generating income: {processedCount}. " +
                    $"Total income: ${totalIncome}");
        }
        else
        {
            Debug.LogWarning("[DayManager] No commercial buildings generating income");
        }
    }

/// <summary>
/// Calculate income (or loss) for a commercial building based on population and happiness
/// </summary>
int CalculateCommercialIncome(Building commercial)
{
    BuildingData data = commercial.buildingData;
    Vector3 position = commercial.transform.position;

    // --------------------------------------------------
    // Population check
    // --------------------------------------------------
    int nearbyPopulation = PopulationManager.Instance.GetPopulationInRadius(
        position,
        data.commercialRadius
    );

    Debug.Log($"[Commercial] {data.buildingName}: Nearby population = {nearbyPopulation}");

    if (nearbyPopulation < data.minPopulationThreshold)
    {
        Debug.Log($"[Commercial] {data.buildingName}: Below minimum population → income = $0");
        return 0;
    }

    // --------------------------------------------------
    // Population step multiplier
    // --------------------------------------------------
    int steps = Mathf.Clamp((nearbyPopulation - 10) / 10, 0, 4);
    float populationMultiplier = 1f + (steps * 0.5f);

    Debug.Log($"[Commercial] {data.buildingName}: Population steps = {steps}, multiplier = x{populationMultiplier:F1}");

    // --------------------------------------------------
    // Happiness check
    // --------------------------------------------------
    float happiness = PopulationManager.Instance.GetAverageHappinessInRadius(
        position,
        data.commercialRadius
    );

    float reallyHappyThreshold = data.happinessThreshold + 20f;

    float income;

    if (happiness < data.happinessThreshold)
    {
        // Lose 50% of base income per day
        income = -data.baseIncome * 0.5f;
        Debug.Log($"[Commercial] {data.buildingName}: Unhappy → base loss = {income}");
    }
    else if (happiness >= reallyHappyThreshold)
    {
        // Base income + 30% bonus
        income = data.baseIncome * 1.3f;
        Debug.Log($"[Commercial] {data.buildingName}: Really happy → bonus income = {income}");
    }
    else
    {
        // Normal income
        income = data.baseIncome;
        Debug.Log($"[Commercial] {data.buildingName}: Normal happiness → base income = {income}");
    }

    // --------------------------------------------------
    // Apply population multiplier
    // --------------------------------------------------
    income *= populationMultiplier;

    int finalIncome = Mathf.RoundToInt(income);

    Debug.Log($"[Commercial] {data.buildingName}: Final income = ${finalIncome}");

    return finalIncome;
}




    
    // Getters
    public int GetCurrentDay()
    {
        return currentDay;
    }
    
    public float GetDayProgress()
    {
        return dayTimer / dayLengthInSeconds;
    }
    
    public float GetTimeUntilNextDay()
    {
        return dayLengthInSeconds - dayTimer;
    }
}