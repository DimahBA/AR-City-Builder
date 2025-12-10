using System.Collections;
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
        // 1. Update house happiness based on services/factories
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.UpdateHappiness();
        }
        
        // 2. Update population count (after happiness/abandonment checks)
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.UpdatePopulation();
        }
        
        // 3. Process commercial buildings (still using flat income for now)
        ProcessCommercialBuildings();
        
        // 4. Update UI with current day
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
        
        Debug.Log($"[DayManager] Processing commercial buildings...");
        
        // Get all commercial buildings from the registry (much faster than FindObjectsOfType!)
        List<Building> commercialBuildings = BuildingManager.Instance.GetBuildingsByType(BuildingType.Commercial);
        
        Debug.Log($"[DayManager] Found {commercialBuildings.Count} commercial buildings in registry");
        
        int totalIncome = 0;
        
        foreach (Building building in commercialBuildings)
        {
            if (building == null || building.buildingData == null) 
            {
                Debug.LogWarning("[DayManager] Skipping null building or building with null data");
                continue;
            }
            
            // Generate income for this commercial building
            int income = building.buildingData.baseIncome;
            gameUI.AddMoney(income);
            totalIncome += income;
            
            Debug.Log($"[DayManager] {building.buildingData.buildingName} generated ${income}");
        }
        
        if (commercialBuildings.Count > 0)
        {
            Debug.Log($"[DayManager] Total commercial buildings: {commercialBuildings.Count}. Total income: ${totalIncome}");
        }
        else
        {
            Debug.LogWarning("[DayManager] No commercial buildings generating income");
        }
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