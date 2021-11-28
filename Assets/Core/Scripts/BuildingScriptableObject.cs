using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Pyrascape/BuildingsInfo", order = 7)]
public class BuildingScriptableObject : ScriptableObject
{
    public BuildingData[] buildings;
}
[System.Serializable]
public class BuildingData
{
    public string name;
    [Tooltip("The index based on the buildings' height")]
    public int heightIndex = 0;

    [Tooltip("The furthest left the building can spawn")]
    public int horizontalStartIndex = -500;
    [Tooltip("The furthest right the building can spawn")]
    public int horizontalEndIndex = 500;

    public BuildingData Clone()
    {
        BuildingData clone = new BuildingData();
        clone.name = name;
        clone.heightIndex = heightIndex;
        clone.horizontalStartIndex = horizontalStartIndex;
        clone.horizontalEndIndex = horizontalEndIndex;
        return clone;
    }
}
