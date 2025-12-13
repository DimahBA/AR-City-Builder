using System.Collections.Generic;
using UnityEngine;

public class ServiceManager : MonoBehaviour
{
    public static ServiceManager Instance { get; private set; }
    
    [Header("References")]
    public GameUI gameUI;
    
    // Track which services are active
    private Dictionary<Building, bool> serviceActiveStatus;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        serviceActiveStatus = new Dictionary<Building, bool>();
    }
    
    void Start()
    {
        if (gameUI == null)
        {
            gameUI = FindObjectOfType<GameUI>();
        }
    }
    
    /// <summary>
    /// Process daily operating costs for all service buildings
    /// Returns true if all services could be paid, false if some shut down
    /// </summary>
    public bool ProcessDailyServiceCosts()
    {
        if (BuildingManager.Instance == null || gameUI == null)
        {
            Debug.LogError("[ServiceManager] Missing BuildingManager or GameUI!");
            return false;
        }
        
        List<Building> services = BuildingManager.Instance.GetBuildingsByType(BuildingType.Service);
        
        Debug.Log($"[ServiceManager] Processing {services.Count} services for daily costs");
        
        bool allServicesPaid = true;
        int totalCost = 0;
        int activeServices = 0;
        int shutdownServices = 0;
        
        foreach (Building service in services)
        {
            if (service == null || service.buildingData == null) continue;
            
            int operatingCost = service.buildingData.dailyOperatingCost;
            
            // Check if we can afford this service
            if (gameUI.CanAfford(operatingCost))
            {
                // Pay for the service
                gameUI.SpendMoney(operatingCost);
                totalCost += operatingCost;
                
                // Mark or keep service as active
                SetServiceActive(service, true);
                activeServices++;
                
                Debug.Log($"[ServiceManager] Paid ${operatingCost} for {service.buildingData.buildingName}");
            }
            else
            {
                // Cannot afford - shut down service
                SetServiceActive(service, false);
                shutdownServices++;
                allServicesPaid = false;
                
                Debug.LogWarning($"[ServiceManager] Cannot afford {service.buildingData.buildingName} (cost: ${operatingCost}). Service SHUT DOWN!");
            }
        }
        
        if (services.Count > 0)
        {
            Debug.Log($"[ServiceManager] Daily service summary: {activeServices} active, {shutdownServices} shut down. Total cost: ${totalCost}");
        }
        
        return allServicesPaid;
    }
    
    /// <summary>
    /// Set a service building's active status and update indicator
    /// </summary>
    public void SetServiceActive(Building service, bool isActive)
    {
        serviceActiveStatus[service] = isActive;
        
        // Update status indicator
        ServiceStatusIndicator indicator = service.GetComponent<ServiceStatusIndicator>();
        if (indicator == null)
        {
            indicator = service.gameObject.AddComponent<ServiceStatusIndicator>();
        }
        
        indicator.SetActiveState(isActive);
        
        // Log the state change
        if (!isActive)
        {
            Debug.LogWarning($"[ServiceManager] {service.buildingData.buildingName} SHUT DOWN - insufficient funds!");
        }
    }

    
    /// <summary>
    /// Check if a specific service is currently active
    /// </summary>
    public bool IsServiceActive(Building service)
    {
        if (service == null) return false;
        
        // Default to active if not tracked yet
        if (!serviceActiveStatus.ContainsKey(service))
        {
            serviceActiveStatus[service] = true;
        }
        
        return serviceActiveStatus[service];
    }
    
    /// <summary>
    /// Get count of active services
    /// </summary>
    public int GetActiveServiceCount()
    {
        int count = 0;
        foreach (var kvp in serviceActiveStatus)
        {
            if (kvp.Value) count++;
        }
        return count;
    }
}
 