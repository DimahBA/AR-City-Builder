using UnityEngine;

public class SmoothFollower : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform imageTarget; // Assign your ImageTarget here
    
    [Header("Smoothing Settings")]
    [Range(1f, 20f)]
    public float positionSmoothness = 8f;
    [Range(1f, 20f)]
    public float rotationSmoothness = 8f;
    
    [Header("Enable/Disable")]
    public bool enableSmoothing = true;
    
    void OnEnable()
    {
        // Immediately snap to target position when enabled
        if (imageTarget != null)
        {
            transform.position = imageTarget.position;
            transform.rotation = imageTarget.rotation;
            Debug.Log($"{gameObject.name} snapped to {imageTarget.name} at position: {transform.position}");
        }
    }
    
    void LateUpdate()
    {
        if (imageTarget == null) 
        {
            Debug.LogWarning($"ImageTarget is null on {gameObject.name}!");
            return;
        }
        
        if (enableSmoothing)
        {
            // Smooth position interpolation
            transform.position = Vector3.Lerp(
                transform.position, 
                imageTarget.position, 
                Time.deltaTime * positionSmoothness
            );
            
            // Smooth rotation interpolation
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                imageTarget.rotation, 
                Time.deltaTime * rotationSmoothness
            );
        }
        else
        {
            // Direct following (no smoothing)
            transform.position = imageTarget.position;
            transform.rotation = imageTarget.rotation;
        }
    }
}
