using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }
    
    // Registry of all buildings organized by type
    private Dictionary<BuildingType, List<Building>> buildingsByType;
    
    // Quick lookup of all buildings
    private List<Building> allBuildings;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Initialize dictionaries
        buildingsByType = new Dictionary<BuildingType, List<Building>>();
        allBuildings = new List<Building>();
        
        // Initialize a list for each building type
        foreach (BuildingType type in System.Enum.GetValues(typeof(BuildingType)))
        {
            buildingsByType[type] = new List<Building>();
        }
        
        Debug.Log("BuildingManager initialized");
    }
    
    /// <summary>
    /// Register a building when it's placed and paid for
    /// </summary>
    public void RegisterBuilding(Building building)
    {
        if (building == null || building.buildingData == null)
        {
            Debug.LogWarning("Attempted to register null building or building with no data");
            return;
        }
        
        // Avoid duplicate registration
        if (allBuildings.Contains(building))
        {
            Debug.LogWarning($"Building {building.buildingData.buildingName} already registered");
            return;
        }
        
        BuildingType type = building.buildingData.buildingType;
        
        buildingsByType[type].Add(building);
        allBuildings.Add(building);
        
        Debug.Log($"Registered {type} building: {building.buildingData.buildingName}. Total {type}s: {buildingsByType[type].Count}");
    }
    
    /// <summary>
    /// Unregister a building when it's destroyed or removed
    /// </summary>
    public void UnregisterBuilding(Building building)
    {
        if (building == null || building.buildingData == null)
        {
            return;
        }
        
        BuildingType type = building.buildingData.buildingType;
        
        buildingsByType[type].Remove(building);
        allBuildings.Remove(building);
        
        Debug.Log($"Unregistered {type} building: {building.buildingData.buildingName}. Total {type}s: {buildingsByType[type].Count}");
    }
    
    /// <summary>
    /// Get all buildings of a specific type
    /// </summary>
    public List<Building> GetBuildingsByType(BuildingType type)
    {
        return buildingsByType[type];
    }
    
    /// <summary>
    /// Get all buildings
    /// </summary>
    public List<Building> GetAllBuildings()
    {
        return allBuildings;
    }
    
    /// <summary>
    /// Get count of buildings by type
    /// </summary>
    public int GetBuildingCount(BuildingType type)
    {
        return buildingsByType[type].Count;
    }
    
    /// <summary>
    /// Get all buildings within a radius of a position
    /// </summary>
    public List<Building> GetBuildingsInRadius(Vector3 position, float radius, BuildingType? typeFilter = null)
    {
        List<Building> buildingsInRadius = new List<Building>();
        float radiusSquared = radius * radius;
        
        List<Building> buildingsToCheck = typeFilter.HasValue 
            ? buildingsByType[typeFilter.Value] 
            : allBuildings;
        
        foreach (Building building in buildingsToCheck)
        {
            if (building == null) continue;
            
            float distanceSquared = (building.transform.position - position).sqrMagnitude;
            if (distanceSquared <= radiusSquared)
            {
                buildingsInRadius.Add(building);
            }
        }
        
        return buildingsInRadius;
    }
    
    /// <summary>
    /// Get the closest N buildings of a specific type
    /// </summary>
    public List<Building> GetClosestBuildings(Vector3 position, BuildingType type, int count)
    {
        List<Building> buildings = new List<Building>(buildingsByType[type]);
        
        // Sort by distance
        buildings.Sort((a, b) =>
        {
            float distA = (a.transform.position - position).sqrMagnitude;
            float distB = (b.transform.position - position).sqrMagnitude;
            return distA.CompareTo(distB);
        });
        
        // Return only the requested count
        int resultCount = Mathf.Min(count, buildings.Count);
        return buildings.GetRange(0, resultCount);
    }
    
    /// <summary>
    /// Clear all registered buildings (useful for scene resets)
    /// </summary>
    public void ClearAllBuildings()
    {
        foreach (var list in buildingsByType.Values)
        {
            list.Clear();
        }
        allBuildings.Clear();
        
        Debug.Log("All buildings cleared from registry");
    }
}
