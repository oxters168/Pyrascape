using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Data", menuName = "Pyrascape/OreInfo", order = 6)]
public class OreScriptableObject : ScriptableObject
{
    public OreData[] ores;

    public OreData GetOreAt(Vector3Int currentTilePos, int surfaceHeight, bool hasPhysicalTile)
    {
        OreData outputOre = null;
        if (currentTilePos.y < surfaceHeight && hasPhysicalTile) //If we are currently underneath the surface and there currently exists
        {
            for (int j = ores.Length - 1; j >= 0; j--)
            {
                var currentOre = ores[j];
                var currentDepth = Mathf.Abs(currentTilePos.y - (surfaceHeight - 1)); //(surfaceHeight - 1) because we don't want to consider the surface
                if (currentDepth >= currentOre.minDepth && currentDepth <= currentOre.maxDepth) //If we are in the correct depth
                {
                    float depthPercent = ((float)(currentDepth - currentOre.minDepth)) / (currentOre.maxDepth - currentOre.minDepth); //Get the t value to evaluate with based on depth
                    float currentDepthChance = (currentOre.percentSpread.Evaluate(depthPercent) * (currentOre.maxPercentChance - currentOre.minPercentChance)) + currentOre.minPercentChance; //Evaluate to retrieve the chance of spawning
                    currentDepthChance = 1 - currentDepthChance; //Flip 0-1 to 1-0
                    if (currentDepthChance < WorldGenerator.GetNoiseValueOf(currentTilePos, currentOre.noise))
                    {
                        // ores.Add(oresPool.Get(ore => { ore.world = this; ore.oreData = currentOre.Clone(); ore.transform.position = outdoorPhysical.CellToWorld(currentTilePos) + (Vector3.right + Vector3.up) * 0.5f; }));
                        // outdoorForegroundTiles[i] = currentOre.ore;
                        // oreMap[currentTilePos] = currentOre.Clone();
                        outputOre = currentOre;
                        break;
                    }
                }
            }
        }
        return outputOre;
    }
}
[System.Serializable]
public class OreData
{
    public string name;
    // public Sprite sprite;
    public TileBase ore;
    public NoiseInfo noise;
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
