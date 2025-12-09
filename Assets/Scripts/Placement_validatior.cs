using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacementValidator : MonoBehaviour
{
    [Header("Placement Settings")]
    [Tooltip("Maximum distance from road to be considered adjacent")]
    public float adjacencyDistance = 2f;
    
    [Tooltip("Minimum distance from road to avoid collision")]
    public float minDistanceFromRoad = 0.5f;
    
    [Header("Visual Feedback")]
    public Color validPlacementColor = Color.white;
    public Color notNearRoadColor = new Color(1f, 0.5f, 0f); // Orange
    public Color cannotAffordColor = Color.red;
    
    private Renderer[] buildingRenderers;
    private Dictionary<Renderer, Material[]> originalMaterials;
    private Material invalidMaterial;
    private Material warningMaterial;
    private bool isPlacementValid = false;
    private bool isNearRoad = false;
    
    private Building buildingScript;
    private GameUI gameUI;
    
    void Start()
    {
        // Get all renderers in the building (including inactive children)
        buildingRenderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        
        // Store original materials
        originalMaterials = new Dictionary<Renderer, Material[]>();
        foreach (Renderer rend in buildingRenderers)
        {
            originalMaterials[rend] = rend.materials;
        }
        
        // Create materials for feedback
        invalidMaterial = new Material(Shader.Find("Standard"));
        invalidMaterial.color = cannotAffordColor;
        
        warningMaterial = new Material(Shader.Find("Standard"));
        warningMaterial.color = notNearRoadColor;
        
        // Get references
        buildingScript = GetComponent<Building>();
        gameUI = FindObjectOfType<GameUI>();
    }

    
    void Update()
    {
        ValidatePlacement();
    }
    
    void ValidatePlacement()
    {
        // Find all road objects
        GameObject[] roads = GameObject.FindGameObjectsWithTag("road");
        
        bool nearRoad = false;
        bool collidingWithRoad = false;
        
        if (roads.Length > 0)
        {
            // Check distance to each road
            foreach (GameObject road in roads)
            {
                float distance = GetDistanceToRoad(road);
                
                // Check if too close (colliding)
                if (distance < minDistanceFromRoad)
                {
                    collidingWithRoad = true;
                    break;
                }
                
                // Check if within adjacent range
                if (distance <= adjacencyDistance)
                {
                    nearRoad = true;
                }
            }
        }
        
        // Update state
        isNearRoad = nearRoad;
        isPlacementValid = !collidingWithRoad;
        
        // Apply visual feedback based on state
        UpdateVisualFeedback();
    }
    
    void UpdateVisualFeedback()
    {
        // Don't change colors if already paid
        if (buildingScript != null && buildingScript.HasPaid())
        {
            RestoreOriginalMaterials();
            return;
        }
        
        // Check if can afford
        bool canAfford = gameUI != null && buildingScript != null && 
                         gameUI.CanAfford(buildingScript.GetCost());
        
        // Determine which state we're in
        if (!canAfford)
        {
            // RED: Cannot afford
            ApplyMaterial(invalidMaterial);
        }
        else if (!isPlacementValid)
        {
            // RED: Colliding with road (invalid placement)
            ApplyMaterial(invalidMaterial);
        }
        else if (!isNearRoad)
        {
            // ORANGE: Valid placement but not near road
            ApplyMaterial(warningMaterial);
        }
        else
        {
            // WHITE: Valid placement and near road
            RestoreOriginalMaterials();
        }
    }
    
    void ApplyMaterial(Material material)
    {
        foreach (Renderer rend in buildingRenderers)
        {
            Material[] mats = new Material[rend.materials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = material;
            }
            rend.materials = mats;
        }
    }
    
    void RestoreOriginalMaterials()
    {
        foreach (Renderer rend in buildingRenderers)
        {
            if (originalMaterials.ContainsKey(rend))
            {
                rend.materials = originalMaterials[rend];
            }
        }
    }
    
    float GetDistanceToRoad(GameObject road)
    {
        // Get the closest point on the road's collider
        Collider roadCollider = road.GetComponent<Collider>();
        
        if (roadCollider != null)
        {
            // Get building bounds center
            Vector3 buildingCenter = GetComponent<Collider>() != null 
                ? GetComponent<Collider>().bounds.center 
                : transform.position;
            
            // Get closest point on road
            Vector3 closestPoint = roadCollider.ClosestPoint(buildingCenter);
            
            // Calculate distance
            return Vector3.Distance(buildingCenter, closestPoint);
        }
        else
        {
            // Fallback to simple distance calculation
            return Vector3.Distance(transform.position, road.transform.position);
        }
    }
    
    // Public method to check if placement is valid (useful for confirming placement)
    public bool IsPlacementValid()
    {
        return isPlacementValid && isNearRoad;
    }
    
    // Optional: Check if near road (for other scripts)
    public bool IsNearRoad()
    {
        return isNearRoad;
    }
    
    // Optional: Visualize the detection ranges in editor
    void OnDrawGizmosSelected()
    {
        GameObject[] roads = GameObject.FindGameObjectsWithTag("road");
        
        foreach (GameObject road in roads)
        {
            // Draw adjacency range
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(road.transform.position, adjacencyDistance);
            
            // Draw collision range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(road.transform.position, minDistanceFromRoad);
        }
    }
}
