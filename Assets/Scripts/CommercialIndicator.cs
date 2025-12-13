using UnityEngine;

public class CommercialIndicator : MonoBehaviour
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
    public Color hasFactoryColor = new Color(0f, 1f, 0f, 1f); // Green
    public Color noFactoryColor = new Color(1f, 1f, 0f, 1f); // Yellow
    
    [Tooltip("Size of the indicator")]
    public float indicatorScale = 0.4f;
    
    private GameObject indicatorObject;
    private Material indicatorMaterial;
    private float calculatedHeight;
    private GameObject targetObject;
    private Building buildingScript;
    
    // State tracking
    private bool hasFactory = false;
    
    void Start()
    {
        buildingScript = GetComponent<Building>();
        FindTargetObject();
        CalculateBuildingHeight();
        CreateIndicator();
    }
    
    void FindTargetObject()
    {
        CustomTrackableEventHandler handler = GetComponent<CustomTrackableEventHandler>();
        if (handler != null && handler.smoothFollowerObject != null)
        {
            targetObject = handler.smoothFollowerObject;
            Debug.Log($"[CommercialIndicator] Using smooth follower: {targetObject.name}");
        }
        else
        {
            targetObject = gameObject;
            Debug.Log($"[CommercialIndicator] Using building object itself: {targetObject.name}");
        }
    }
    
    void CalculateBuildingHeight()
    {
        if (targetObject == null)
        {
            calculatedHeight = 2.5f;
            return;
        }
        
        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();
        
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"[CommercialIndicator] No renderers found on {targetObject.name}");
            calculatedHeight = 2.5f;
            return;
        }
        
        Bounds combinedBounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            combinedBounds.Encapsulate(renderer.bounds);
        }
        
        float buildingTop = combinedBounds.max.y;
        Vector3 topWorldPos = new Vector3(transform.position.x, buildingTop, transform.position.z);
        Vector3 topLocalPos = transform.InverseTransformPoint(topWorldPos);
        
        calculatedHeight = topLocalPos.y + extraHeightOffset;
        
        Debug.Log($"[CommercialIndicator] Calculated height for {targetObject.name}: {calculatedHeight}");
    }
    
    void CreateIndicator()
    {
        // Create cube indicator
        indicatorObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        indicatorObject.name = "CommercialIndicator";
        indicatorObject.transform.SetParent(transform);
        indicatorObject.transform.localScale = Vector3.one * indicatorScale;
        indicatorObject.transform.localPosition = new Vector3(0, calculatedHeight, 0);
        
        Renderer renderer = indicatorObject.GetComponent<Renderer>();
        indicatorMaterial = new Material(Shader.Find("Standard"));
        indicatorMaterial.EnableKeyword("_EMISSION");
        renderer.material = indicatorMaterial;
        
        Collider collider = indicatorObject.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
        
        UpdateCommercialStatus();
        
        Debug.Log($"[CommercialIndicator] Created indicator at height {calculatedHeight} for {targetObject.name}");
    }
    
    void Update()
    {
        if (indicatorObject == null) return;
        
        // Update status every 0.5 seconds to avoid constant checks
        if (Time.frameCount % 30 == 0)
        {
            UpdateCommercialStatus();
        }
        
        // Update position if using smooth follower
        if (targetObject != null && targetObject != gameObject)
        {
            UpdateIndicatorPosition();
        }
        
        // Bobbing animation
        if (enableBobbing)
        {
            Vector3 pos = indicatorObject.transform.localPosition;
            pos.y = calculatedHeight + Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;
            indicatorObject.transform.localPosition = pos;
        }
        
        // Rotate cube for visual interest
        indicatorObject.transform.Rotate(Vector3.up, 30f * Time.deltaTime);
    }
    
    void UpdateIndicatorPosition()
    {
        if (targetObject != null && targetObject != gameObject)
        {
            Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds combinedBounds = renderers[0].bounds;
                foreach (Renderer renderer in renderers)
                {
                    combinedBounds.Encapsulate(renderer.bounds);
                }
                
                float buildingTop = combinedBounds.max.y;
                Vector3 topWorldPos = new Vector3(transform.position.x, buildingTop, transform.position.z);
                Vector3 topLocalPos = transform.InverseTransformPoint(topWorldPos);
                
                float newHeight = topLocalPos.y + extraHeightOffset;
                if (Mathf.Abs(newHeight - calculatedHeight) > 0.1f)
                {
                    calculatedHeight = newHeight;
                }
            }
        }
    }
    
    void UpdateCommercialStatus()
    {
        if (buildingScript == null || buildingScript.buildingData == null || BuildingManager.Instance == null)
        {
            hasFactory = false;
            UpdateIndicatorVisual();
            return;
        }
        
        // Check if there are enough factories for this commercial
        int commercialCount = BuildingManager.Instance.GetBuildingCount(BuildingType.Commercial);
        int factoryCount = BuildingManager.Instance.GetBuildingCount(BuildingType.Factory);
        
        // Get this commercial's index in the list
        var commercialBuildings = BuildingManager.Instance.GetBuildingsByType(BuildingType.Commercial);
        int myIndex = commercialBuildings.IndexOf(buildingScript);
        
        // Can only generate income if there are enough factories and we're within the factory limit
        hasFactory = factoryCount > 0 && myIndex < factoryCount;
        
        UpdateIndicatorVisual();
        
        // Debug logging
        if (!hasFactory)
        {
            Debug.Log($"[CommercialIndicator] {buildingScript.buildingData.buildingName}: No factory available (Commercial #{myIndex + 1}, Factories: {factoryCount})");
        }
        else
        {
            Debug.Log($"[CommercialIndicator] {buildingScript.buildingData.buildingName}: Factory available - can generate income");
        }
    }
    
    void UpdateIndicatorVisual()
    {
        if (indicatorMaterial == null) return;
        
        Color color = hasFactory ? hasFactoryColor : noFactoryColor;
        
        indicatorMaterial.color = color;
        indicatorMaterial.SetColor("_EmissionColor", color * 0.5f);
    }
    
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
