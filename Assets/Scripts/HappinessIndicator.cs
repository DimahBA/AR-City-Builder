using UnityEngine;

public class HappinessIndicator : MonoBehaviour
{
    [Header("Indicator Settings")]
    [Tooltip("How high above the building to float")]
    public float heightOffset = 2f;
    
    [Tooltip("Should the indicator bob up and down?")]
    public bool enableBobbing = true;
    
    [Tooltip("Speed of bobbing animation")]
    public float bobbingSpeed = 1f;
    
    [Tooltip("How much to bob up and down")]
    public float bobbingAmount = 0.2f;
    
    [Header("Color Thresholds")]
    [Tooltip("Happiness above this is green")]
    public float happyThreshold = 70f;
    
    [Tooltip("Happiness below this is red")]
    public float unhappyThreshold = 40f;
    
    [Header("References")]
    private GameObject indicatorBall;
    private Building parentBuilding;
    private float startHeight;
    private Material ballMaterial;
    
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
        
        CreateIndicator();
    }
    
    void CreateIndicator()
    {
        // Create a sphere GameObject
        indicatorBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicatorBall.name = "HappinessIndicator";
        indicatorBall.transform.SetParent(transform);
        indicatorBall.transform.localScale = Vector3.one * 0.3f; // Small sphere
        
        // Position above the building
        indicatorBall.transform.localPosition = new Vector3(0, heightOffset, 0);
        startHeight = heightOffset;
        
        // Get or create material
        Renderer renderer = indicatorBall.GetComponent<Renderer>();
        ballMaterial = new Material(Shader.Find("Standard"));
        ballMaterial.EnableKeyword("_EMISSION");
        renderer.material = ballMaterial;
        
        // Remove collider (we don't need physics)
        Collider collider = indicatorBall.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
        
        // Initial color update
        UpdateIndicatorColor();
        
        Debug.Log($"[HappinessIndicator] Created indicator for {parentBuilding.buildingData.buildingName}");
    }
    
    void Update()
    {
        if (indicatorBall == null || parentBuilding == null) return;
        
        // Update color based on happiness
        UpdateIndicatorColor();
        
        // Bobbing animation
        if (enableBobbing)
        {
            float newY = startHeight + Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;
            indicatorBall.transform.localPosition = new Vector3(0, newY, 0);
        }
        
        // Hide if house is abandoned
        if (parentBuilding.IsAbandoned())
        {
            indicatorBall.SetActive(false);
        }
        else
        {
            indicatorBall.SetActive(true);
        }
    }
    
    void UpdateIndicatorColor()
    {
        if (ballMaterial == null || parentBuilding == null) return;
        
        float happiness = parentBuilding.GetHappiness();
        Color color;
        
        if (happiness >= happyThreshold)
        {
            // Green - Happy
            color = Color.green;
        }
        else if (happiness <= unhappyThreshold)
        {
            // Red - Unhappy
            color = Color.red;
        }
        else
        {
            // Yellow - Neutral (interpolate between red and green)
            float t = (happiness - unhappyThreshold) / (happyThreshold - unhappyThreshold);
            color = Color.Lerp(Color.red, Color.yellow, t);
        }
        
        // Apply color
        ballMaterial.color = color;
        ballMaterial.SetColor("_EmissionColor", color * 0.5f); // Add glow
    }
    
    void OnDestroy()
    {
        // Clean up
        if (indicatorBall != null)
        {
            Destroy(indicatorBall);
        }
        if (ballMaterial != null)
        {
            Destroy(ballMaterial);
        }
    }
}