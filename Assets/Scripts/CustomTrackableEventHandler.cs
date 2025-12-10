using UnityEngine;
using Vuforia;

public class CustomTrackableEventHandler : DefaultObserverEventHandler
{
    public GameObject smoothFollowerObject;
    private Building buildingScript; // Add this
    
    protected override void Start()
    {
        base.Start();
        // Get the Building script if it exists
        buildingScript = GetComponent<Building>();
    }
    
    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();
        
        // Notify the Building script that tracking was found
        if (buildingScript != null)
        {
            buildingScript.OnTrackingFound();
        }
        
        if (smoothFollowerObject != null)
        {
            smoothFollowerObject.transform.position = transform.position;
            smoothFollowerObject.transform.rotation = transform.rotation;
            smoothFollowerObject.SetActive(true);
        }
    }
    
    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();
        
        // Notify the Building script that tracking was lost
        if (buildingScript != null)
        {
            buildingScript.OnTrackingLost();
        }
        
        if (smoothFollowerObject != null)
        {
            smoothFollowerObject.SetActive(false);
        }
    }
}
