using UnityEngine;

public class ServiceVisualizer : MonoBehaviour
{
    [Header("Visual Settings")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Gray
    
    private Renderer[] buildingRenderers;
    private bool isActive = true;
    private Material inactiveMaterial;
    private Building buildingScript;
    
    // Store original materials
    private System.Collections.Generic.Dictionary<Renderer, Material[]> originalMaterials;
    
    void Start()
    {
        buildingScript = GetComponent<Building>();
        
        // DON'T apply any visuals until the building is actually paid for!
        if (buildingScript == null || !buildingScript.HasPaid())
        {
            return; // Wait until building is placed and paid
        }
        
        InitializeVisuals();
    }
    
    void InitializeVisuals()
    {
        // Check if we need to use smooth follower
        GameObject targetObject = gameObject;
        CustomTrackableEventHandler handler = GetComponent<CustomTrackableEventHandler>();
        if (handler != null && handler.smoothFollowerObject != null)
        {
            targetObject = handler.smoothFollowerObject;
        }
        
        // Get all renderers
        buildingRenderers = targetObject.GetComponentsInChildren<Renderer>(includeInactive: true);
        
        // Store original materials
        originalMaterials = new System.Collections.Generic.Dictionary<Renderer, Material[]>();
        foreach (Renderer rend in buildingRenderers)
        {
            originalMaterials[rend] = rend.materials;
        }
        
        // Create inactive material
        inactiveMaterial = new Material(Shader.Find("Standard"));
        inactiveMaterial.color = inactiveColor;
    }
    
    public void SetActiveState(bool active)
    {
        isActive = active;
        
        // Only update visuals if building is paid for
        if (buildingScript != null && buildingScript.HasPaid())
        {
            if (originalMaterials == null || originalMaterials.Count == 0)
            {
                InitializeVisuals();
            }
            UpdateVisuals();
        }
    }
    
    void UpdateVisuals()
    {
        // Don't change materials if building isn't paid yet
        if (buildingScript == null || !buildingScript.HasPaid())
        {
            return;
        }
        
        if (buildingRenderers == null) return;
        
        foreach (Renderer rend in buildingRenderers)
        {
            if (rend == null) continue;
            
            if (isActive)
            {
                // Restore original materials
                if (originalMaterials != null && originalMaterials.ContainsKey(rend))
                {
                    rend.materials = originalMaterials[rend];
                }
            }
            else
            {
                // Apply gray material to show inactive
                Material[] grayMaterials = new Material[rend.materials.Length];
                for (int i = 0; i < grayMaterials.Length; i++)
                {
                    grayMaterials[i] = inactiveMaterial;
                }
                rend.materials = grayMaterials;
            }
        }
    }
}

