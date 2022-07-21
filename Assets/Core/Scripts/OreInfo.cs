using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Data", menuName = "Pyrascape/OreInfo", order = 6)]
public class OreInfo : ScriptableObject
{
    public OreData[] ores;

    public OreData GetOreAt(Vector3Int currentTilePos, bool hasPhysicalTile)
    {
        OreData outputOre = null;
        //if (currentTilePos.y < surfaceHeight && hasPhysicalTile) //If we are currently underneath the surface and there currently exists
        if (hasPhysicalTile) //If there's a physical tile in the current position
        {
            for (int j = ores.Length - 1; j >= 0; j--)
            {
                var currentOre = ores[j];
                var currentHor = currentTilePos.x;
                var currentDepth = currentTilePos.y;
                //var currentDepth = Mathf.Abs(currentTilePos.y - (surfaceHeight - 1)); //(surfaceHeight - 1) because we don't want to consider the surface
                if (currentDepth >= currentOre.lowerMostPos && currentDepth <= currentOre.upperMostPos && currentHor >= currentOre.leftMostPos && currentHor <= currentOre.rightMostPos) //If we are in the correct depth
                {
                    float depthPercent = Mathf.Abs(((float)(currentDepth - currentOre.upperMostPos)) / (currentOre.upperMostPos - currentOre.lowerMostPos)); //Get the t value to evaluate with based on depth
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
    [Tooltip("The left most position the ore can be found [inclusive]")]
    public int leftMostPos = -16;
    [Tooltip("The right most position the ore can be found [inclusive]")]
    public int rightMostPos = 16;
    [Tooltip("The highest depth the ore can be found at [inclusive]")]
    public int upperMostPos = 0;
    [Tooltip("The lowest depth the ore can be found at [inclusive]")]
    public int lowerMostPos = -10;
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
        clone.upperMostPos = upperMostPos;
        clone.lowerMostPos = lowerMostPos;
        clone.minPercentChance = minPercentChance;
        clone.maxPercentChance = maxPercentChance;
        clone.percentSpread = percentSpread;
        return clone;
    }
}
