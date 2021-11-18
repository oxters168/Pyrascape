using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Data", menuName = "Pyrascape/OreInfo", order = 6)]
public class OreScriptableObject : ScriptableObject
{
    public OreData[] ores;
}
[System.Serializable]
public class OreData
{
    public string name;
    // public Sprite sprite;
    public TileBase ore;
    [Tooltip("The highest depth the ore can be found at [inclusive]")]
    public int minDepth = 0;
    [Tooltip("The lowest depth the ore can be found at [inclusive]")]
    public int maxDepth = 10;
    [Range(0, 1), Tooltip("0 = no chance and 1 = always")]
    /// <summary>
    /// 0 = no chance and 1 = always
    /// </summary>
    public float minPercentChance = 1;
    [Range(0, 1), Tooltip("0 = no chance and 1 = always")]
    /// <summary>
    /// 0 = no chance and 1 = always
    /// </summary>
    public float maxPercentChance = 1;
    /// <summary>
    /// The x axis represents the depth and the y axis represents the percent chance (normalized to the given values)
    /// </summary>
    [Tooltip("The x axis represents the depth and the y axis represents the percent chance (normalized to the given values)")]
    public AnimationCurve percentSpread = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

    public OreData Clone()
    {
        OreData clone = new OreData();
        clone.name = name;
        // clone.sprite = sprite;
        clone.ore = ore;
        clone.minDepth = minDepth;
        clone.maxDepth = maxDepth;
        clone.minPercentChance = minPercentChance;
        clone.maxPercentChance = maxPercentChance;
        clone.percentSpread = percentSpread;
        return clone;
    }
}
