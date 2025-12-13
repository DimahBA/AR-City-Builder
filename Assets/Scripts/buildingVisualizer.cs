using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BuildingVisualizer : MonoBehaviour
{
    public BuildingData data;

    [Header("Runtime Radius Zone")]
    public Color zoneColor = new Color(0f, 1f, 0f, 0.3f);
    public int circleSegments = 64;
    public float yOffset = 0.05f;   // Slightly above ground

    LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.loop = true;
        line.useWorldSpace = true;
        line.positionCount = circleSegments;
        line.material = new Material(Shader.Find("Sprites/Default"));
    }

    void Update()
    {
        if (!Application.isPlaying || data == null)
        {
            if (line != null) line.enabled = false;
            return;
        }

        float radius = GetCurrentRadius();
        if (radius <= 0f)
        {
            line.enabled = false;
            return;
        }

        line.enabled = true;
        line.startColor = zoneColor;
        line.endColor = zoneColor;
        line.widthMultiplier = 0.05f;

        Vector3 center = transform.position + Vector3.up * yOffset;
        float angleStep = 360f / circleSegments;

        for (int i = 0; i < circleSegments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleStep);
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            line.SetPosition(i, center + new Vector3(x, 0f, z));
        }
    }

    float GetCurrentRadius()
    {
        switch (data.buildingType)
        {
            case BuildingType.Service:
                return data.serviceRadius;
            case BuildingType.Factory:
                return data.pollutionRadius;
            case BuildingType.Commercial:
                return data.commercialRadius;
            default:
                return 0f;
        }
    }
}
