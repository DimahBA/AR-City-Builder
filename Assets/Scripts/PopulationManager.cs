using System.Collections.Generic;
using UnityEngine;

public class PopulationManager : MonoBehaviour
{
    public static PopulationManager Instance { get; private set; }
    
    [Header("Happiness Settings")]
    [Tooltip("Happiness below this triggers abandonment countdown")]
    public float abandonmentThreshold = 30f;
    
    [Tooltip("Days of low happiness before house is abandoned")]
    public int daysBeforeAbandonment = 3;
    
    [Header("References")]
    public GameUI gameUI;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void Start()
    {
        if (gameUI == null)
        {
            gameUI = FindObjectOfType<GameUI>();
        }
    }
    
    /// <summary>
    /// Calculate happiness for all houses based on nearby services and factories
    /// Called once per day by DayManager
    /// </summary>
    public void UpdateHappiness()
    {
        if (BuildingManager.Instance == null)
        {
            Debug.LogError("[Population] BuildingManager.Instance is NULL!");
            return;
        }
        
        List<Building> houses = BuildingManager.Instance.GetBuildingsByType(BuildingType.House);
        List<Building> services = BuildingManager.Instance.GetBuildingsByType(BuildingType.Service);
        List<Building> factories = BuildingManager.Instance.GetBuildingsByType(BuildingType.Factory);
        
        Debug.Log($"[Population] UpdateHappiness: Found {houses.Count} houses, {services.Count} services, {factories.Count} factories");
        
        foreach (Building house in houses)
        {
            if (house == null || house.buildingData == null) continue;
            
            // Start with base happiness (slowly decay toward 50 if no services/factories)
            float newHappiness = house.GetHappiness();
            newHappiness = Mathf.Lerp(newHappiness, 50f, 0.1f); // Slow decay to neutral
            
            // Add bonuses from nearby services
            float serviceBonus = 0f;
            foreach (Building service in services)
            {
                if (service == null || service.buildingData == null) continue;
                
                float distance = Vector3.Distance(house.transform.position, service.transform.position);
                if (distance <= service.buildingData.serviceRadius)
                {
                    serviceBonus += service.buildingData.happinessBonus;
                }
            }
            
            // Add penalties from nearby factories
            float factoryPenalty = 0f;
            foreach (Building factory in factories)
            {
                if (factory == null || factory.buildingData == null) continue;
                
                float distance = Vector3.Distance(house.transform.position, factory.transform.position);
                if (distance <= factory.buildingData.pollutionRadius)
                {
                    factoryPenalty += factory.buildingData.pollutionPenalty;
                }
            }
            
            // Apply bonuses and penalties
            newHappiness += serviceBonus;
            newHappiness -= factoryPenalty;
            
            // Set the new happiness (clamped 0-100)
            house.SetHappiness(newHappiness);
            
            // Check for abandonment
            house.CheckAbandonment(abandonmentThreshold, daysBeforeAbandonment);
            
            Debug.Log($"[Population] {house.buildingData.buildingName}: Happiness = {newHappiness:F1} (Services: +{serviceBonus:F1}, Factories: -{factoryPenalty:F1})");
        }
    }
    
    /// <summary>
    /// Calculate total city population and update UI
    /// Called once per day by DayManager
    /// </summary>
    public void UpdatePopulation()
    {
        if (BuildingManager.Instance == null) return;
        
        List<Building> houses = BuildingManager.Instance.GetBuildingsByType(BuildingType.House);
        
        int totalPopulation = 0;
        int activeHouses = 0;
        int abandonedHouses = 0;
        
        foreach (Building house in houses)
        {
            if (house == null || house.buildingData == null) continue;
            
            if (house.IsAbandoned())
            {
                abandonedHouses++;
            }
            else
            {
                activeHouses++;
                totalPopulation += house.GetCurrentPopulation();
            }
        }
        
        // Update UI
        if (gameUI != null)
        {
            gameUI.UpdatePopulation(totalPopulation);
        }
        
        Debug.Log($"[Population] Total: {totalPopulation} residents in {activeHouses} houses ({abandonedHouses} abandoned)");
    }
    
    /// <summary>
    /// Get average happiness in a radius (used by commercial buildings)
    /// </summary>
    public float GetAverageHappinessInRadius(Vector3 position, float radius)
    {
        if (BuildingManager.Instance == null) return 50f;
        
        List<Building> nearbyHouses = BuildingManager.Instance.GetBuildingsInRadius(position, radius, BuildingType.House);
        
        if (nearbyHouses.Count == 0) return 50f; // Neutral if no houses nearby
        
        float totalHappiness = 0f;
        int validHouses = 0;
        
        foreach (Building house in nearbyHouses)
        {
            if (house == null || house.IsAbandoned()) continue;
            
            totalHappiness += house.GetHappiness();
            validHouses++;
        }
        
        return validHouses > 0 ? totalHappiness / validHouses : 50f;
    }
    
    /// <summary>
    /// Get population count in a radius (used by commercial buildings)
    /// </summary>
    public int GetPopulationInRadius(Vector3 position, float radius)
    {
        if (BuildingManager.Instance == null) return 0;
        
        List<Building> nearbyHouses = BuildingManager.Instance.GetBuildingsInRadius(position, radius, BuildingType.House);
        
        int population = 0;
        foreach (Building house in nearbyHouses)
        {
            if (house == null) continue;
            population += house.GetCurrentPopulation();
        }
        
        return population;
    }
}