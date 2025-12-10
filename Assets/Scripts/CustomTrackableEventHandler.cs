using UnityEngine;
using Vuforia;

public class CustomTrackableEventHandler : DefaultObserverEventHandler
{
    public GameObject smoothFollowerObject; // Your interpolated object
    
    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();
        
        if (smoothFollowerObject != null)
        {
            // IMPORTANT: Set position BEFORE activating
            smoothFollowerObject.transform.position = transform.position;
            smoothFollowerObject.transform.rotation = transform.rotation;
            
            // Now activate it
            smoothFollowerObject.SetActive(true);
            
            Debug.Log($"Activated {smoothFollowerObject.name} at position: {transform.position}");
        }
    }
    
    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();
        
        if (smoothFollowerObject != null)
        {
            smoothFollowerObject.SetActive(false);
        }
    }
}
