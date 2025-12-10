using UnityEngine;

public enum BuildingType
{
    House,
    Service,
    Factory,
    Commercial,
    Road
}

[CreateAssetMenu(fileName = "New Building", menuName = "AR City/Building Data")]
public class BuildingData : ScriptableObject
{
    [Header("Basic Info")]
    public string buildingName;
    public BuildingType buildingType;
    public int cost;
    [TextArea(3, 5)]
    public string description;
    
    [Header("House Settings")]
    [Tooltip("How many residents this house can hold (Houses only)")]
    public int housingCapacity = 10;
    
    [Header("Service Settings")]
    [Tooltip("Daily operating cost for services (Services only)")]
    public int dailyOperatingCost = 50;
    [Tooltip("Happiness bonus provided to nearby houses")]
    public float happinessBonus = 10f;
    [Tooltip("Radius in which this service affects houses")]
    public float serviceRadius = 5f;
    
    [Header("Factory Settings")]
    [Tooltip("Daily maintenance cost (Factories only)")]
    public int dailyMaintenanceCost = 20;
    [Tooltip("Happiness penalty from pollution/noise")]
    public float pollutionPenalty = 15f;
    [Tooltip("Radius in which pollution affects houses")]
    public float pollutionRadius = 7f;
    
    [Header("Commercial Settings")]
    [Tooltip("Base income per day before modifiers")]
    public int baseIncome = 100;
    [Tooltip("Radius to check for nearby population")]
    public float commercialRadius = 6f;
    [Tooltip("Minimum nearby population needed for any income")]
    public int minPopulationThreshold = 5;
    [Tooltip("Multiplier when nearby happiness is low (e.g., 0.5 = half income)")]
    [Range(0f, 1f)]
    public float lowHappinessMultiplier = 0.5f;
    [Tooltip("Happiness threshold below which income is reduced")]
    public float happinessThreshold = 50f;
}