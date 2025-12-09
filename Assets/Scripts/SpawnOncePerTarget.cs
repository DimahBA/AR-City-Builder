using UnityEngine;
using Vuforia;

public class CityPieceObserverHandler : DefaultObserverEventHandler
{
    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();
        // Enable only THIS observer's children
        SetChildrenActive(true);
    }

    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();
        SetChildrenActive(false);
    }

    private void SetChildrenActive(bool active)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(active);
        }
    }
}
