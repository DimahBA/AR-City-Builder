using UnityEngine;

public class ServiceStatusIndicator : MonoBehaviour
{
    [Header("Indicator Settings")]
    [Tooltip("Extra height above the building's top")]
    public float extraHeightOffset = 0.5f;
    
    [Tooltip("Should the indicator bob up and down?")]
    public bool enableBobbing = true;
    
    [Tooltip("Speed of bobbing animation")]
    public float bobbingSpeed = 0.8f;
    
    [Tooltip("How much to bob up and down")]
    public float bobbingAmount = 0.15f;
    
    [Header("Visual Settings")]
    public Color activeColor = new Color(0f, 1f, 0f, 1f); // Green
    public Color inactiveColor = new Color(1f, 0f, 0f, 1f); // Red
    
    [Tooltip("Size of the indicator")]
    public float indicatorScale = 0.4f;
    
    [Header("Icon Type")]
    [Tooltip("What shape to use for service indicator")]
    public PrimitiveType indicatorShape = PrimitiveType.Cube;
    
    private GameObject indicatorObject;
    private Material indicatorMaterial;
    private float calculatedHeight;
    private bool isActive = true;
    private GameObject targetObject; // The object to calculate height from
    
    void Start()
    {
        FindTargetObject();
        CalculateBuildingHeight();
        CreateIndicator();
    }
    
    void FindTargetObject()
    {
        // First, check if this building has a smooth follower
        CustomTrackableEventHandler handler = GetComponent<CustomTrackableEventHandler>();
        if (handler != null && handler.smoothFollowerObject != null)
        {
            targetObject = handler.smoothFollowerObject;
            Debug.Log($"[ServiceStatusIndicator] Using smooth follower: {targetObject.name}");
        }
        else
        {
            // Fallback to the building itself
            targetObject = gameObject;
            Debug.Log($"[ServiceStatusIndicator] Using building object itself: {targetObject.name}");
        }
    }
    
    void CalculateBuildingHeight()
    {
        if (targetObject == null)
        {
            calculatedHeight = 2.5f; // Fallback default
            return;
        }
        
        // Get all renderers in the target object and its children
        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();
        
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"[ServiceStatusIndicator] No renderers found on {targetObject.name}");
            calculatedHeight = 2.5f; // Fallback
            return;
        }
        
        // Calculate the combined bounds of all renderers
        Bounds combinedBounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            combinedBounds.Encapsulate(renderer.bounds);
        }
        
        // Get the top of the building in local space
        float buildingTop = combinedBounds.max.y;
        
        // Convert to local space relative to this transform
        Vector3 topWorldPos = new Vector3(transform.position.x, buildingTop, transform.position.z);
        Vector3 topLocalPos = transform.InverseTransformPoint(topWorldPos);
        
        // Set the height as the local Y position plus extra offset
        calculatedHeight = topLocalPos.y + extraHeightOffset;
        
        Debug.Log($"[ServiceStatusIndicator] Calculated height for {targetObject.name}: {calculatedHeight} (building height: {topLocalPos.y})");
    }
    
    void CreateIndicator()
    {
        // Create the indicator object (cube to differentiate from happiness sphere)
        indicatorObject = GameObject.CreatePrimitive(indicatorShape);
        indicatorObject.name = "ServiceStatusIndicator";
        indicatorObject.transform.SetParent(transform);
        indicatorObject.transform.localScale = Vector3.one * indicatorScale;
        
        // Position above the calculated top of building
        indicatorObject.transform.localPosition = new Vector3(0, calculatedHeight, 0);
        
        // Create and setup material
        Renderer renderer = indicatorObject.GetComponent<Renderer>();
        indicatorMaterial = new Material(Shader.Find("Standard"));
        indicatorMaterial.EnableKeyword("_EMISSION");
        renderer.material = indicatorMaterial;
        
        // Remove collider
        Collider collider = indicatorObject.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
        
        // Initial state
        UpdateIndicatorVisual();
        
        Debug.Log($"[ServiceStatusIndicator] Created indicator at height {calculatedHeight} for {targetObject.name}");
    }
    
    void Update()
    {
        if (indicatorObject == null) return;
        
        // Update position to follow the target object if it exists
        if (targetObject != null && targetObject != gameObject)
        {
            // Recalculate position based on smooth follower's current position
            UpdateIndicatorPosition();
        }
        
        // Bobbing animation
        if (enableBobbing)
        {
            Vector3 pos = indicatorObject.transform.localPosition;
            pos.y = calculatedHeight + Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;
            indicatorObject.transform.localPosition = pos;
        }
        
        // Optional: Rotate for visual interest
        if (indicatorShape == PrimitiveType.Cube)
        {
            indicatorObject.transform.Rotate(Vector3.up, 30f * Time.deltaTime);
        }
    }
    
    void UpdateIndicatorPosition()
    {
        // If we're using a smooth follower, update the indicator position
        // to stay above the smooth follower's visual representation
        if (targetObject != null && targetObject != gameObject)
        {
            // Get the current bounds of the target
            Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds combinedBounds = renderers[0].bounds;
                foreach (Renderer renderer in renderers)
                {
                    combinedBounds.Encapsulate(renderer.bounds);
                }
                
                // Update height if building bounds changed
                float buildingTop = combinedBounds.max.y;
                Vector3 topWorldPos = new Vector3(transform.position.x, buildingTop, transform.position.z);
                Vector3 topLocalPos = transform.InverseTransformPoint(topWorldPos);
                
                // Only update if there's a significant change
                float newHeight = topLocalPos.y + extraHeightOffset;
                if (Mathf.Abs(newHeight - calculatedHeight) > 0.1f)
                {
                    calculatedHeight = newHeight;
                }
            }
        }
    }
    
    public void SetActiveState(bool active)
    {
        isActive = active;
        UpdateIndicatorVisual();
        
        if (!active)
        {
            Debug.Log("[ServiceStatusIndicator] Service SHUT DOWN - cannot afford operating cost!");
        }
        else
        {
            Debug.Log("[ServiceStatusIndicator] Service ACTIVE");
        }
    }
    
    void UpdateIndicatorVisual()
    {
        if (indicatorMaterial == null) return;
        
        Color color = isActive ? activeColor : inactiveColor;
        
        // Apply color
        indicatorMaterial.color = color;
        indicatorMaterial.SetColor("_EmissionColor", color * 0.5f); // Add glow
        
        // Optional: Make it pulse when inactive
        if (!isActive && indicatorObject != null)
        {
            // Add a pulsing effect for inactive services
            float pulse = Mathf.PingPong(Time.time * 2f, 1f);
            indicatorObject.transform.localScale = Vector3.one * indicatorScale * (1f + pulse * 0.2f);
        }
        else if (indicatorObject != null)
        {
            // Reset scale when active
            indicatorObject.transform.localScale = Vector3.one * indicatorScale;
        }
    }
    
    /// <summary>
    /// Recalculate the building height (useful if the building model changes)
    /// </summary>
    public void RecalculateHeight()
    {
        FindTargetObject();
        CalculateBuildingHeight();
        
        if (indicatorObject != null)
        {
            indicatorObject.transform.localPosition = new Vector3(0, calculatedHeight, 0);
        }
    }
    
    void OnDestroy()
    {
        // Clean up
        if (indicatorObject != null)
        {
            Destroy(indicatorObject);
        }
        if (indicatorMaterial != null)
        {
            Destroy(indicatorMaterial);
        }
    }
}
