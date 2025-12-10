using System.Collections;
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
        Debug.Log($"=== Day {currentDay} ended ===");
        
        // Process commercial buildings
        ProcessCommercialBuildings();
        
        // Update UI with current day
        if (gameUI != null)
        {
            gameUI.UpdateDayDisplay(currentDay);
        }
        
        Debug.Log($"Day {currentDay} complete. Current money: ${gameUI.GetCurrentMoney()}");
    }
    
    void ProcessCommercialBuildings()
    {
        // Find all commercial buildings that have been paid for
        Building[] allBuildings = FindObjectsOfType<Building>();
        int commercialCount = 0;
        
        foreach (Building building in allBuildings)
        {
            // Check if this is a commercial building and has been paid
            if (building.HasPaid() && building.buildingData != null 
                && building.buildingData.buildingName.ToLower().Contains("commercial"))
            {
                commercialCount++;
                
                // Generate income for this commercial building
                int income = 100;
                gameUI.AddMoney(income);
                
                Debug.Log($"Commercial building generated ${income}");
            }
        }
        
        if (commercialCount > 0)
        {
            Debug.Log($"Total commercial buildings: {commercialCount}. Total income: ${commercialCount * 100}");
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
