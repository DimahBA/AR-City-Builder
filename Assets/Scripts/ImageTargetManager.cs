using UnityEngine;
using Vuforia;
using System.Collections.Generic;
using System.Linq;

public class MultiTargetManager : MonoBehaviour
{
    public static MultiTargetManager Instance;
    
    [Header("Settings")]
    [SerializeField] private float duplicateCheckRadius = 0.05f; // How close is "same position"
    [SerializeField] private bool debugMode = true;
    [SerializeField] private int maxTargetsPerPosition = 1; // Strictly one per position
    
    // Track active representations by their world position
    private Dictionary<Vector3, ActiveTarget> positionMap = new Dictionary<Vector3, ActiveTarget>();
    private Dictionary<string, ActiveTarget> targetMap = new Dictionary<string, ActiveTarget>();
    
    private class ActiveTarget
    {
        public string targetId;
        public GameObject imageTarget;
        public GameObject representation;
        public Vector3 position;
        public float activationTime;
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SetupAllTargets();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void SetupAllTargets()
    {
        ObserverBehaviour[] observers = FindObjectsOfType<ObserverBehaviour>();
        
        foreach (ObserverBehaviour observer in observers)
        {
            // Ensure all children start disabled
            foreach (Transform child in observer.transform)
            {
                child.gameObject.SetActive(false);
            }
            
            // Remove or disable default handler if exists
            DefaultObserverEventHandler defaultHandler = observer.GetComponent<DefaultObserverEventHandler>();
            if (defaultHandler != null)
            {
                defaultHandler.enabled = false;
            }
            
            // Add our custom handler
            TargetHandler handler = observer.gameObject.AddComponent<TargetHandler>();
        }
        
        Debug.Log($"Setup complete for {observers.Length} targets");
    }
    
    public bool RequestActivation(string targetId, GameObject imageTarget, GameObject representation)
    {
        Vector3 worldPos = imageTarget.transform.position;
        
        // Check if there's already something at this position
        Vector3 existingPos = GetExistingPositionNearby(worldPos);
        
        if (existingPos != Vector3.zero)
        {
            // Something already exists here
            if (positionMap.ContainsKey(existingPos))
            {
                var existing = positionMap[existingPos];
                
                // If it's the same target, allow it (just updating)
                if (existing.targetId == targetId)
                {
                    return true;
                }
                
                // Different target trying to spawn at same position - BLOCK IT
                if (debugMode)
                {
                    Debug.LogWarning($"BLOCKED: {imageTarget.name} tried to spawn at position of {existing.imageTarget.name}");
                }
                return false;
            }
        }
        
        // Check if this specific representation is already active elsewhere
        foreach (var kvp in targetMap)
        {
            if (kvp.Value.representation == representation && kvp.Key != targetId)
            {
                // This representation is already showing somewhere else - disable it there first
                DeactivateTarget(kvp.Key);
                break;
            }
        }
        
        // Safe to activate
        ActivateTarget(targetId, imageTarget, representation, worldPos);
        return true;
    }
    
    private void ActivateTarget(string targetId, GameObject imageTarget, GameObject representation, Vector3 position)
    {
        // Clean up any existing entry for this target
        if (targetMap.ContainsKey(targetId))
        {
            DeactivateTarget(targetId);
        }
        
        var newTarget = new ActiveTarget
        {
            targetId = targetId,
            imageTarget = imageTarget,
            representation = representation,
            position = position,
            activationTime = Time.time
        };
        
        // Round position to avoid floating point issues
        Vector3 roundedPos = RoundPosition(position);
        
        positionMap[roundedPos] = newTarget;
        targetMap[targetId] = newTarget;
        
        // Activate only the first child (the correct representation)
        for (int i = 0; i < imageTarget.transform.childCount; i++)
        {
            imageTarget.transform.GetChild(i).gameObject.SetActive(i == 0);
        }
        
        if (debugMode)
        {
            Debug.Log($"✓ Activated: {imageTarget.name} at position {roundedPos}");
        }
    }
    
    public void RequestDeactivation(string targetId)
    {
        DeactivateTarget(targetId);
    }
    
    private void DeactivateTarget(string targetId)
    {
        if (targetMap.ContainsKey(targetId))
        {
            var target = targetMap[targetId];
            
            // Disable the representation
            if (target.representation != null)
            {
                target.representation.SetActive(false);
            }
            
            // Remove from position map
            Vector3 roundedPos = RoundPosition(target.position);
            if (positionMap.ContainsKey(roundedPos))
            {
                positionMap.Remove(roundedPos);
            }
            
            // Remove from target map
            targetMap.Remove(targetId);
            
            if (debugMode)
            {
                Debug.Log($"✗ Deactivated: {targetId}");
            }
        }
    }
    
    private Vector3 GetExistingPositionNearby(Vector3 checkPos)
    {
        foreach (var kvp in positionMap)
        {
            if (Vector3.Distance(kvp.Key, checkPos) < duplicateCheckRadius)
            {
                return kvp.Key;
            }
        }
        return Vector3.zero;
    }
    
    private Vector3 RoundPosition(Vector3 pos)
    {
        // Round to avoid floating point comparison issues
        return new Vector3(
            Mathf.Round(pos.x * 100f) / 100f,
            Mathf.Round(pos.y * 100f) / 100f,
            Mathf.Round(pos.z * 100f) / 100f
        );
    }
    
    // Emergency cleanup
    [ContextMenu("Force Clean All")]
    public void ForceCleanAll()
    {
        foreach (var kvp in targetMap)
        {
            if (kvp.Value.representation != null)
            {
                kvp.Value.representation.SetActive(false);
            }
        }
        
        positionMap.Clear();
        targetMap.Clear();
        
        Debug.Log("Forced cleanup of all targets!");
    }
    
    // Show active targets in inspector
    public void DebugShowActive()
    {
        Debug.Log($"Active targets: {targetMap.Count}");
        foreach (var kvp in targetMap)
        {
            Debug.Log($"- {kvp.Value.imageTarget.name} at {kvp.Value.position}");
        }
    }
}
