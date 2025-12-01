using UnityEngine;

public class Building : MonoBehaviour
{
    [Header("Building Configuration")]
    public BuildingData buildingData;
    
    private BuildingPlacementValidator placementValidator;
    private GameUI gameUI;
    private bool hasPaid = false;
    private bool isPaymentProcessed = false;
    
    void Start()
    {
        placementValidator = GetComponent<BuildingPlacementValidator>();
        gameUI = FindObjectOfType<GameUI>();
        
        if (placementValidator == null)
        {
            Debug.LogError("BuildingPlacementValidator not found on " + gameObject.name);
        }
    }
    
    void Update()
    {
        // Only check payment once the building is in a valid position
        if (!hasPaid && placementValidator != null && placementValidator.IsPlacementValid())
        {
            // Check if we can afford it
            if (gameUI != null && gameUI.CanAfford(buildingData.cost))
            {
                // Only process payment once
                if (!isPaymentProcessed)
                {
                    // Deduct money
                    gameUI.SpendMoney(buildingData.cost);
                    hasPaid = true;
                    isPaymentProcessed = true;
                    
                    Debug.Log($"Paid ${buildingData.cost} for {buildingData.buildingName}");
                }
            }
        }
        else if (hasPaid && placementValidator != null && !placementValidator.IsPlacementValid())
        {
            // If building was paid but moved to invalid position, reset payment
            hasPaid = false;
        }
    }
    
    public bool HasPaid()
    {
        return hasPaid;
    }
    
    public int GetCost()
    {
        return buildingData != null ? buildingData.cost : 0;
    }
}
