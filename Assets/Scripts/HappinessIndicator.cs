using UnityEngine;

public class HappinessIndicator : MonoBehaviour
{
    [Header("Indicator Settings")]
    [Tooltip("Extra height above the building's top")]
    public float extraHeightOffset = 0.5f;
    
    [Tooltip("Should the indicator bob up and down?")]
    public bool enableBobbing = true;
    
    [Tooltip("Speed of bobbing animation")]
    public float bobbingSpeed = 1f;
    
    [Tooltip("How much to bob up and down")]
    public float bobbingAmount = 0.2f;
    
    [Tooltip("Should the diamond rotate?")]
    public bool enableRotation = true;
    
    [Tooltip("Rotation speed")]
    public float rotationSpeed = 50f;
    
    [Header("Diamond Settings")]
    [Tooltip("Size of the diamond")]
    public float diamondScale = 0.3f;
    
    [Tooltip("How tall/stretched the diamond should be")]
    [Range(1f, 3f)]
    public float diamondStretch = 1.5f;
    
    [Header("Color Thresholds")]
    [Tooltip("Happiness above this is green")]
    public float happyThreshold = 70f;
    
    [Tooltip("Happiness below this is red")]
    public float unhappyThreshold = 40f;
    
    [Header("References")]
    private GameObject diamondObject;
    private Building parentBuilding;
    private float calculatedHeight;
    private Material diamondMaterial;
    private GameObject targetObject;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    
    void Start()
    {
        parentBuilding = GetComponent<Building>();
        
        if (parentBuilding == null || parentBuilding.buildingData == null)
        {
            Debug.LogWarning("[HappinessIndicator] No Building component found!");
            enabled = false;
            return;
        }
        
        // Only create indicator for houses
        if (parentBuilding.buildingData.buildingType != BuildingType.House)
        {
            enabled = false;
            return;
        }
        
        FindTargetObject();
        CalculateBuildingHeight();
        CreateDiamondIndicator();
    }
    
    void FindTargetObject()
    {
        // First, check if this building has a smooth follower
        CustomTrackableEventHandler handler = GetComponent<CustomTrackableEventHandler>();
        if (handler != null && handler.smoothFollowerObject != null)
        {
            targetObject = handler.smoothFollowerObject;
            Debug.Log($"[HappinessIndicator] Using smooth follower: {targetObject.name}");
        }
        else
        {
            // Fallback to the building itself
            targetObject = gameObject;
            Debug.Log($"[HappinessIndicator] Using building object itself: {targetObject.name}");
        }
    }
    
    void CalculateBuildingHeight()
    {
        if (targetObject == null)
        {
            calculatedHeight = 2f; // Fallback default
            return;
        }
        
        // Get all renderers in the target object and its children
        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();
        
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"[HappinessIndicator] No renderers found on {targetObject.name}");
            calculatedHeight = 2f; // Fallback
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
        
        Debug.Log($"[HappinessIndicator] Calculated height for {targetObject.name}: {calculatedHeight}");
    }
    
    void CreateDiamondIndicator()
    {
        // Create a new GameObject for the diamond
        diamondObject = new GameObject("HappinessDiamond");
        diamondObject.transform.SetParent(transform);
        diamondObject.transform.localPosition = new Vector3(0, calculatedHeight, 0);
        diamondObject.transform.localScale = Vector3.one * diamondScale;
        
        // Add mesh components
        meshFilter = diamondObject.AddComponent<MeshFilter>();
        meshRenderer = diamondObject.AddComponent<MeshRenderer>();
        
        // Create the diamond mesh
        CreateDiamondMesh();
        
        // Create and setup material
        diamondMaterial = new Material(Shader.Find("Standard"));
        diamondMaterial.EnableKeyword("_EMISSION");
        meshRenderer.material = diamondMaterial;
        
        // Initial color update
        UpdateIndicatorColor();
        
        Debug.Log($"[HappinessIndicator] Created diamond indicator at height {calculatedHeight}");
    }
    
    void CreateDiamondMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Diamond";
        
        // Define vertices for a diamond shape (octahedron)
        // The diamond has 6 vertices: top, bottom, and 4 middle points
        Vector3[] vertices = new Vector3[]
        {
            // Top point
            new Vector3(0, 1f * diamondStretch, 0),
            
            // Middle ring (square in the middle)
            new Vector3(0.5f, 0, 0.5f),
            new Vector3(0.5f, 0, -0.5f),
            new Vector3(-0.5f, 0, -0.5f),
            new Vector3(-0.5f, 0, 0.5f),
            
            // Bottom point
            new Vector3(0, -1f * diamondStretch, 0)
        };
        
        // Define triangles (faces of the diamond)
        int[] triangles = new int[]
        {
            // Top pyramid
            0, 1, 2,  // Front-right face
            0, 2, 3,  // Back-right face
            0, 3, 4,  // Back-left face
            0, 4, 1,  // Front-left face
            
            // Bottom pyramid
            5, 2, 1,  // Front-right face
            5, 3, 2,  // Back-right face
            5, 4, 3,  // Back-left face
            5, 1, 4   // Front-left face
        };
        
        // Calculate normals for proper lighting
        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = vertices[i].normalized;
        }
        
        // Assign to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals(); // Recalculate for better lighting
        
        meshFilter.mesh = mesh;
    }
    
    void Update()
    {
        if (diamondObject == null || parentBuilding == null) return;
        
        // Update color based on happiness
        UpdateIndicatorColor();
        
        // Bobbing animation
        if (enableBobbing)
        {
            float newY = calculatedHeight + Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;
            diamondObject.transform.localPosition = new Vector3(0, newY, 0);
        }
        
        // Rotation animation (like The Sims plumbob)
        if (enableRotation)
        {
            diamondObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        // Hide if house is abandoned
        if (parentBuilding.IsAbandoned())
        {
            diamondObject.SetActive(false);
        }
        else
        {
            diamondObject.SetActive(true);
        }
    }
    
    void UpdateIndicatorColor()
    {
        if (diamondMaterial == null || parentBuilding == null) return;
        
        float happiness = parentBuilding.GetHappiness();
        Color color;
        
        if (happiness >= happyThreshold)
        {
            // Green - Happy (classic Sims green)
            color = new Color(0.2f, 0.8f, 0.2f, 1f);
        }
        else if (happiness <= unhappyThreshold)
        {
            // Red - Unhappy
            color = new Color(0.8f, 0.2f, 0.2f, 1f);
        }
        else
        {
            // Yellow/Orange - Neutral
            float t = (happiness - unhappyThreshold) / (happyThreshold - unhappyThreshold);
            color = Color.Lerp(
                new Color(0.8f, 0.2f, 0.2f, 1f), // Red
                new Color(0.9f, 0.9f, 0.2f, 1f), // Yellow
                t
            );
        }
        
        // Apply color with transparency
        color.a = 0.9f; // Slight transparency
        diamondMaterial.color = color;
        
        // Add emission for glow effect
        diamondMaterial.SetColor("_EmissionColor", color * 0.6f);
        
        // Optional: Make it pulse slightly when very happy or very unhappy
        if (happiness >= 90f || happiness <= 20f)
        {
            float pulse = Mathf.Sin(Time.time * 3f) * 0.1f + 1f;
            diamondObject.transform.localScale = Vector3.one * diamondScale * pulse;
        }
        else
        {
            diamondObject.transform.localScale = Vector3.one * diamondScale;
        }
    }
    
    void OnDestroy()
    {
        // Clean up
        if (diamondObject != null)
        {
            Destroy(diamondObject);
        }
        if (diamondMaterial != null)
        {
            Destroy(diamondMaterial);
        }
    }
}
