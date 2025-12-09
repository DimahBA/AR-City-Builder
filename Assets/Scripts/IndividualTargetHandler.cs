using UnityEngine;
using Vuforia;
using System.Collections;

public class TargetHandler : MonoBehaviour
{
    private ObserverBehaviour observerBehaviour;
    private string targetId;
    private GameObject myChild;
    private bool isCurrentlyTracked = false;
    
    // Stabilization
    private int trackingConfirmationFrames = 3;
    private int currentConfirmationCount = 0;
    private Coroutine stabilizationCoroutine;
    
    void Start()
    {
        observerBehaviour = GetComponent<ObserverBehaviour>();
        targetId = gameObject.name + "_" + GetInstanceID();
        
        // Cache the child
        if (transform.childCount > 0)
        {
            myChild = transform.GetChild(0).gameObject;
            myChild.SetActive(false);
        }
        
        if (observerBehaviour != null)
        {
            observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }
    
    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        bool shouldTrack = (status.Status == Status.TRACKED || 
                           status.Status == Status.EXTENDED_TRACKED);
        
        if (shouldTrack)
        {
            currentConfirmationCount++;
            
            if (currentConfirmationCount >= trackingConfirmationFrames && !isCurrentlyTracked)
            {
                AttemptActivation();
            }
        }
        else
        {
            currentConfirmationCount = 0;
            
            if (isCurrentlyTracked)
            {
                Deactivate();
            }
        }
    }
    
    private void AttemptActivation()
    {
        if (MultiTargetManager.Instance == null || myChild == null)
            return;
        
        // Request activation from manager (will be denied if duplicate position)
        bool allowed = MultiTargetManager.Instance.RequestActivation(targetId, gameObject, myChild);
        
        if (allowed)
        {
            isCurrentlyTracked = true;
            
            // Extra safety: ensure ONLY first child is active
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(i == 0);
            }
            
            // Start position monitoring
            if (stabilizationCoroutine != null)
                StopCoroutine(stabilizationCoroutine);
            stabilizationCoroutine = StartCoroutine(MonitorPosition());
        }
        else
        {
            // Activation denied - ensure everything is off
            myChild.SetActive(false);
            isCurrentlyTracked = false;
        }
    }
    
    private void Deactivate()
    {
        if (MultiTargetManager.Instance != null)
        {
            MultiTargetManager.Instance.RequestDeactivation(targetId);
        }
        
        if (myChild != null)
        {
            myChild.SetActive(false);
        }
        
        isCurrentlyTracked = false;
        
        if (stabilizationCoroutine != null)
        {
            StopCoroutine(stabilizationCoroutine);
            stabilizationCoroutine = null;
        }
    }
    
    // Monitor for position changes (in case of tracking jumps)
    private IEnumerator MonitorPosition()
    {
        Vector3 lastPosition = transform.position;
        
        while (isCurrentlyTracked)
        {
            yield return new WaitForSeconds(0.1f);
            
            // If position jumped significantly, re-validate
            if (Vector3.Distance(transform.position, lastPosition) > 0.5f)
            {
                if (MultiTargetManager.Instance != null)
                {
                    bool stillAllowed = MultiTargetManager.Instance.RequestActivation(targetId, gameObject, myChild);
                    if (!stillAllowed)
                    {
                        Deactivate();
                        yield break;
                    }
                }
                lastPosition = transform.position;
            }
        }
    }
    
    void OnDestroy()
    {
        if (observerBehaviour != null)
        {
            observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
        
        Deactivate();
    }
}
