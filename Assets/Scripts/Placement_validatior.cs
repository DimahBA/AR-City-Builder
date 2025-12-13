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
    
    [Header("Smooth Follower Reference")]
    [Tooltip("If using a smooth follower, assign it here to color that object instead")]
    public GameObject smoothFollowerObject;
    
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
        // If smooth follower is assigned, get renderers from there instead
        GameObject targetObject = smoothFollowerObject != null ? smoothFollowerObject : gameObject;
        
        // Get all renderers in the building (including inactive children)
        buildingRenderers = targetObject.GetComponentsInChildren<Renderer>(includeInactive: true);
        
        Debug.Log($"[PlacementValidator] Found {buildingRenderers.Length} renderers on {targetObject.name}");
        
        // Store original materials
        originalMaterials = new Dictionary<Renderer, Material[]>();
        foreach (Renderer rend in buildingRenderers)
        {
            // Clone the materials so we don't modify the original assets
            Material[] clonedMats = new Material[rend.materials.Length];
            for (int i = 0; i < rend.materials.Length; i++)
            {
                clonedMats[i] = new Material(rend.materials[i]);
            }
            originalMaterials[rend] = clonedMats;
            rend.materials = clonedMats; // Apply cloned materials
        }
        
        // Create materials for feedback
        invalidMaterial = new Material(Shader.Find("Standard"));
        invalidMaterial.color = cannotAffordColor;
        
        warningMaterial = new Material(Shader.Find("Standard"));
        warningMaterial.color = notNearRoadColor;
        
        // Get references
        buildingScript = GetComponent<Building>();
        gameUI = FindObjectOfType<GameUI>();
        
        // Try to auto-find smooth follower if not assigned
        if (smoothFollowerObject == null)
        {
            CustomTrackableEventHandler handler = GetComponent<CustomTrackableEventHandler>();
            if (handler != null && handler.smoothFollowerObject != null)
            {
                smoothFollowerObject = handler.smoothFollowerObject;
                Debug.Log($"[PlacementValidator] Auto-found smooth follower: {smoothFollowerObject.name}");
                
                // Re-initialize with smooth follower renderers
                targetObject = smoothFollowerObject;
                buildingRenderers = targetObject.GetComponentsInChildren<Renderer>(includeInactive: true);
                
                originalMaterials.Clear();
                foreach (Renderer rend in buildingRenderers)
                {
                    Material[] clonedMats = new Material[rend.materials.Length];
                    for (int i = 0; i < rend.materials.Length; i++)
                    {
                        clonedMats[i] = new Material(rend.materials[i]);
                    }
                    originalMaterials[rend] = clonedMats;
                    rend.materials = clonedMats;
                }
            }
        }
    }

    
    void Update()
    {
        ValidatePlacement();
    }
    
    void ValidatePlacement()
    {
        GameObject[] roads = GameObject.FindGameObjectsWithTag("road");
        
        bool nearRoad = false;
        bool collidingWithRoad = false;
        
        if (roads.Length > 0)
        {
            foreach (GameObject road in roads)
            {
                float distance = GetDistanceToRoad(road);
                
                if (distance < minDistanceFromRoad)
                {
                    collidingWithRoad = true;
                    break;
                }
                
                if (distance <= adjacencyDistance)
                {
                    nearRoad = true;
                }
            }
        }
        
        isNearRoad = nearRoad;
        isPlacementValid = !collidingWithRoad;
        
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
            if (rend == null) continue;
            
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
            if (rend == null) continue;
            
            if (originalMaterials.ContainsKey(rend))
            {
                rend.materials = originalMaterials[rend];
            }
        }
    }
    
    float GetDistanceToRoad(GameObject road)
    {
        // Use smooth follower position if available, otherwise use this object's position
        Vector3 buildingPos = smoothFollowerObject != null ? 
                              smoothFollowerObject.transform.position : 
                              transform.position;
        Vector3 roadPos = road.transform.position;
        
        // Only consider XZ plane (ignore height difference)
        buildingPos.y = 0;
        roadPos.y = 0;
        
        return Vector3.Distance(buildingPos, roadPos);
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