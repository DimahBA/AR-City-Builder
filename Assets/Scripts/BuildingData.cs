using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "AR City/Building Data")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public int cost;
    [TextArea(3, 5)]
    public string description;
}