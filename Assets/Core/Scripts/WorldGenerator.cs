using UnityEngine;
using UnityHelpers;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    private CompositeCollider2D _physicsBounds;
    private CompositeCollider2D PhysicsBounds { get { if (_physicsBounds == null) _physicsBounds = GetComponentInChildren<CompositeCollider2D>(); return _physicsBounds; } }

    public int surfaceHeight = -1;
    [Space(10)]
    public Tile noiseTile;
    public bool debugNoise;
    private bool prevDebugNoise;

    [Space(10)]
    public DoorController doorPrefab;
    private ObjectPool<DoorController> doorsPool;
    public int buildingMaxWidth = 8;
    public int buildingMaxHeight = 8;
    public int buildingCushionX = 8;
    public int buildingCushionY = 8;
    private List<DoorController> doors = new List<DoorController>();

    [Space(10)]
    public bool isIndoors;
    public Tilemap indoorForeground;
    public Tilemap indoorPhysical;
    public Tilemap indoorBackground;
    public Tilemap outdoorForeground;
    public Tilemap outdoorPhysical;
    public Tilemap outdoorBackground;

    [Space(10)]
    public Transform target;
    [Tooltip("How many tiles should be visible at once")]
    public Vector2Int renderSize = new Vector2Int(16, 12);
    private Vector2Int prevRenderSize;

    [Space(10)]
    // public Ore orePrefab;
    public NoiseInfo oreNoise;
    public OreScriptableObject oreInfo;
    public BiomeInfo currentBiome;
    public BuildingInfo currentBuilding;
    private BackgroundLoop currentOutdoorBackground;
    // private ObjectPool<Ore> oresPool;
    // private List<Ore> ores = new List<Ore>();
    private Dictionary<Vector3Int, OreData> oreMap = new Dictionary<Vector3Int, OreData>();

    /// <summary>
    /// The chunk the target is currently in represented by the bottom left corner tile index
    /// </summary>
    private Vector2Int chunk;
    /// <summary>
    /// The chunk the target was in in the previous frame
    /// </summary>
    private Vector2Int prevChunk;

    private bool firstDraw = true;
    private FastNoise noise;

    void Start()
    {
        noise = new FastNoise(WorldData.seed);
        Transform doorsParent = new GameObject("Doors").transform;
        // Transform oresParent = new GameObject("Ores").transform;
        doorsPool = new ObjectPool<DoorController>(doorPrefab, 5, false, true, doorsParent);
        // oresPool = new ObjectPool<Ore>(orePrefab, 5, false, true, oresParent);
    }
    void Update()
    {
        prevChunk = chunk;
        chunk = new Vector2Int(Mathf.FloorToInt(target.position.x - (renderSize.x / 2f)), Mathf.FloorToInt(target.position.y - (renderSize.y / 2f)));
        DebugPanel.Log("Position", target.position.xy(), 5);

        if (currentOutdoorBackground != null)
            currentOutdoorBackground.target = target;
        if (firstDraw)
        {
            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(outdoorPhysical.transform.parent);
            currentOutdoorBackground = backgroundObject.AddComponent<BackgroundLoop>();
            currentOutdoorBackground.background = currentBiome.background;
        }

        indoorPhysical.transform.parent.gameObject.SetActive(isIndoors);
        outdoorPhysical.transform.parent.gameObject.SetActive(!isIndoors);
        GenerateTerrain(false);
    }

    public Vector3Int WorldToCell(Vector3 position)
    {
        return outdoorPhysical.WorldToCell(position);
    }
    public bool HasTile(Vector3Int tilePosition)
    {
        return outdoorPhysical.HasTile(tilePosition);
    }
    public Vector3 GetCellCenterWorld(Vector3Int tilePosition)
    {
        return outdoorPhysical.GetCellCenterWorld(tilePosition);
    }

    public void GenerateTerrain(bool forceAll)
    {
        bool smallChange =  chunk != prevChunk;
        bool bigChange = forceAll || firstDraw || renderSize != prevRenderSize || debugNoise != prevDebugNoise;
        if (smallChange || bigChange)
        {
            // Debug.Log(chunkX + ", " + chunkY);

            var newTilePositions = GetChunkTilePositions(chunk.x, chunk.y, renderSize.x, renderSize.y); //Get all new tile indices
            Vector3Int[] tilesToBeDrawn;

            if (!bigChange)
            {
                var oldTilePositions = GetChunkTilePositions(prevChunk.x, prevChunk.y, renderSize.x, renderSize.y); //Get all old tile indices
                tilesToBeDrawn = newTilePositions.Except(oldTilePositions).ToArray(); //Get exclusive new tiles

                var tilesToBeCleared = oldTilePositions.Except(newTilePositions).ToArray(); //Get exclusive old tiles
                ClearTiles(tilesToBeCleared); //Clear exclusive old tiles from grid
                ClearOreMapOf(tilesToBeCleared);
            }
            else
            {
                indoorForeground.ClearAllTiles();
                indoorPhysical.ClearAllTiles();
                indoorBackground.ClearAllTiles();
                outdoorForeground.ClearAllTiles();
                outdoorPhysical.ClearAllTiles();
                outdoorBackground.ClearAllTiles();
                tilesToBeDrawn = newTilePositions;
                oreMap.Clear();
            }

            RemoveObjectsOutsideOf(doors, doorsPool, chunk.x, chunk.y, renderSize.x, renderSize.y);
            // RemoveObjectsOutsideOf(ores, oresPool, newTilePositions);
            DrawTiles(tilesToBeDrawn); //Add exclusive new tiles to grid //When redrawing an area, doors may double spawn

            firstDraw = false;
            prevRenderSize = renderSize;
            prevDebugNoise = debugNoise;
        }
    }
    public OreData GetOreData(Vector3Int tileIndex)
    {
        OreData oreData = null;
        if (oreMap.ContainsKey(tileIndex))
            oreData = oreMap[tileIndex];

        return oreData;
    }
    private void ClearOreMapOf(params Vector3Int[] tilePositions)
    {
        for (int i = 0; i < tilePositions.Length; i++)
            if (oreMap.ContainsKey(tilePositions[i]))
                oreMap.Remove(tilePositions[i]);
    }
    public void RefreshArea(Vector3Int tileIndex) //It's not exactly dig for tiles but just for ore, this is very messy
    {
        //Long handed way to get ore at position
        // for (int i = 0; i < ores.Count; i++)
        // {
        //     var currentOre = ores[i];
        //     if (tileIndex == outdoorPhysical.WorldToCell(currentOre.transform.position))
        //     {
        //         oreData = currentOre.oreData;
        //         oresPool.Return(currentOre);
        //         ores.RemoveAt(i);
        //         break;
        //     }
        // }
        DrawTiles(GetChunkTilePositions(tileIndex.x - 1, tileIndex.y - 1, 3, 3)); //When redrawing an area, doors may double spawn
    }
    private void DrawTiles(params Vector3Int[] tilesToBeDrawn)
    {
        var indoorForegroundTiles = new TileBase[tilesToBeDrawn.Length];
        var indoorPhysicalTiles = new TileBase[tilesToBeDrawn.Length];
        var indoorBackgroundTiles = new TileBase[tilesToBeDrawn.Length];
        var outdoorForegroundTiles = new TileBase[tilesToBeDrawn.Length];
        var outdoorPhysicalTiles = new TileBase[tilesToBeDrawn.Length];
        var outdoorBackgroundTiles = new TileBase[tilesToBeDrawn.Length];

        for (int i = 0; i < outdoorPhysicalTiles.Length; i++)
        {
            var currentTilePos = tilesToBeDrawn[i];

            //Generate terrain
            if (!debugNoise)
            {
                //Ornament tiles
                if (currentTilePos.y == 0 && !WorldData.IsDug(currentTilePos + Vector3Int.down, false))
                    outdoorForegroundTiles[i] = currentBiome.ornamentalTile;

                //Background and physical tiles
                if (currentTilePos.y <= surfaceHeight)
                {
                    outdoorBackgroundTiles[i] = currentBiome.backgroundTile; //Background tiles

                    Vector3Int leftTilePos = currentTilePos + Vector3Int.left;
                    Vector3Int upperTilePos = currentTilePos + Vector3Int.up;
                    Vector3Int rightTilePos = currentTilePos + Vector3Int.right;
                    Vector3Int lowerTilePos = currentTilePos + Vector3Int.down;
                    bool hasLeftTile = !WorldData.IsDug(leftTilePos, false);
                    bool hasTopTile = currentTilePos.y < surfaceHeight && !WorldData.IsDug(upperTilePos, false);
                    bool hasRightTile = !WorldData.IsDug(rightTilePos, false);
                    bool hasBotTile = !WorldData.IsDug(lowerTilePos, false) && NoiseCheck(lowerTilePos, currentBiome.noise);
                    if (currentTilePos.y < surfaceHeight)
                    {
                        hasLeftTile &= NoiseCheck(leftTilePos, currentBiome.noise);
                        hasRightTile &= NoiseCheck(rightTilePos, currentBiome.noise);
                    }
                    if (currentTilePos.y < surfaceHeight - 1)
                        hasTopTile &= NoiseCheck(upperTilePos, currentBiome.noise);
                    int borderIndex = (hasLeftTile ? 1 : 0) + (hasTopTile ? 2 : 0) + (hasRightTile ? 4 : 0) + (hasBotTile ? 8 : 0);

                    if (currentTilePos.y == surfaceHeight && !WorldData.IsDug(currentTilePos, false))
                        outdoorPhysicalTiles[i] = currentBiome.surfaceTile[borderIndex];

                    if (currentTilePos.y < surfaceHeight && NoiseCheck(currentTilePos, currentBiome.noise) && !WorldData.IsDug(currentTilePos, false))
                        outdoorPhysicalTiles[i] = currentBiome.undergroundTile[borderIndex];



                    //Place ore?
                    if (currentTilePos.y < surfaceHeight && outdoorPhysicalTiles[i] != null) //If we are currently underneath the surface and there currently exists
                    {
                        for (int j = oreInfo.ores.Length - 1; j >= 0; j--)
                        {
                            var currentOre = oreInfo.ores[j];
                            var currentDepth = Mathf.Abs(currentTilePos.y - (surfaceHeight - 1)); //(surfaceHeight - 1) because we don't want to consider the surface
                            if (currentDepth >= currentOre.minDepth && currentDepth <= currentOre.maxDepth) //If we are in the correct depth
                            {
                                float depthPercent = ((float)(currentDepth - currentOre.minDepth)) / (currentOre.maxDepth - currentOre.minDepth); //Get the t value to evaluate with based on depth
                                float currentDepthChance = (currentOre.percentSpread.Evaluate(depthPercent) * (currentOre.maxPercentChance - currentOre.minPercentChance)) + currentOre.minPercentChance; //Evaluate to retrieve the chance of spawning
                                if ((1 - currentDepthChance) < GetNoiseValueOf(currentTilePos, oreNoise))
                                {
                                    // ores.Add(oresPool.Get(ore => { ore.world = this; ore.oreData = currentOre.Clone(); ore.transform.position = outdoorPhysical.CellToWorld(currentTilePos) + (Vector3.right + Vector3.up) * 0.5f; }));
                                    outdoorForegroundTiles[i] = currentOre.ore;
                                    oreMap[currentTilePos] = currentOre.Clone();
                                    break;
                                }
                            }
                        }
                    }

                    if (currentTilePos.y < surfaceHeight - 1)
                    {
                        //Calculate building's start corner
                        Vector3Int buildingCorner = new Vector3Int(Mathf.FloorToInt(((float)currentTilePos.x) / (buildingMaxWidth + buildingCushionX)) * (buildingMaxWidth + buildingCushionX), Mathf.FloorToInt(((float)currentTilePos.y) / (buildingMaxHeight + buildingCushionY)) * (buildingMaxHeight + buildingCushionY), 0);
                        
                        //Add door to building
                        if (currentTilePos.x == buildingCorner.x + 1 && currentTilePos.y == buildingCorner.y + 1)
                            doors.Add(doorsPool.Get(door => { door.world = this; door.transform.position = outdoorPhysical.CellToWorld(currentTilePos) + Vector3.right * 0.5f; }));
                        
                        //Populate building
                        if (currentTilePos.x >= buildingCorner.x && currentTilePos.x < (buildingCorner.x + buildingMaxWidth) && currentTilePos.y >= buildingCorner.y && currentTilePos.y < (buildingCorner.y + buildingMaxHeight))
                        {
                            //Set background tiles through entire building
                            indoorBackgroundTiles[i] = currentBuilding.backgroundTile;

                            hasLeftTile = !WorldData.IsDug(leftTilePos, true) && NoiseCheck(leftTilePos, currentBuilding.noise);
                            hasTopTile = !WorldData.IsDug(upperTilePos, true) && NoiseCheck(upperTilePos, currentBuilding.noise);
                            hasRightTile = !WorldData.IsDug(rightTilePos, true) && NoiseCheck(rightTilePos, currentBuilding.noise);
                            hasBotTile = !WorldData.IsDug(lowerTilePos, true) && NoiseCheck(lowerTilePos, currentBuilding.noise);

                            //Set walls of building
                            if (currentTilePos.x == buildingCorner.x && currentTilePos.y == buildingCorner.y) //Bottom left corner
                                indoorPhysicalTiles[i] = currentBuilding.wallTile[6];
                            else if (currentTilePos.x == buildingCorner.x && currentTilePos.y == (buildingCorner.y + buildingMaxHeight - 1)) //Top left corner
                                indoorPhysicalTiles[i] = currentBuilding.wallTile[12];
                            else if (currentTilePos.x == (buildingCorner.x + buildingMaxWidth - 1) && currentTilePos.y == (buildingCorner.y + buildingMaxHeight - 1)) //Top right corner
                                indoorPhysicalTiles[i] = currentBuilding.wallTile[9];
                            else if (currentTilePos.x == (buildingCorner.x + buildingMaxWidth - 1) && currentTilePos.y == buildingCorner.y) //Bottom right corner
                                indoorPhysicalTiles[i] = currentBuilding.wallTile[3];
                            else if (currentTilePos.y == buildingCorner.y) //Bottom wall
                                indoorPhysicalTiles[i] = currentBuilding.wallTile[5]; //Don't add any border since first tile is always empty
                            else if (currentTilePos.y == (buildingCorner.y + buildingMaxHeight - 1)) //Top wall
                                indoorPhysicalTiles[i] = currentBuilding.wallTile[5 + (hasBotTile ? 8 : 0)];
                            else if (currentTilePos.x == buildingCorner.x) //Left wall
                                indoorPhysicalTiles[i] = currentBuilding.wallTile[10 + (hasRightTile ? 4 : 0)];
                            else if (currentTilePos.x == (buildingCorner.x + buildingMaxWidth - 1)) //Right wall
                                indoorPhysicalTiles[i] = currentBuilding.wallTile[10 + (hasLeftTile ? 1 : 0)];
                            else if (currentTilePos.y > buildingCorner.y + 1 && NoiseCheck(currentTilePos, currentBuilding.noise) && !WorldData.IsDug(currentTilePos, true)) //Inner walls
                            {
                                if (currentTilePos.x == buildingCorner.x + 1) //Near left edge, so make sure has left border
                                    hasLeftTile = true;
                                if (currentTilePos.x == buildingCorner.x + buildingMaxWidth - 2) //Near right edge, so make sure has right border
                                    hasRightTile = true;
                                if (currentTilePos.y == buildingCorner.y + 2) //Near bottom edge, so make sure does not have bottom border since first tile is always empty
                                    hasBotTile = false;
                                if (currentTilePos.y == buildingCorner.y + buildingMaxHeight - 2) //Near top edge, so make sure has top border
                                    hasTopTile = true;

                                borderIndex = (hasLeftTile ? 1 : 0) + (hasTopTile ? 2 : 0) + (hasRightTile ? 4 : 0) + (hasBotTile ? 8 : 0);
                                indoorPhysicalTiles[i] = currentBuilding.wallTile[borderIndex];
                            }
                        }
                    }
                }
            }
            else
            {
                float perlin = GetNoiseValueOf(currentTilePos, currentBiome.noise);
                outdoorPhysical.SetTile(currentTilePos, noiseTile);
                outdoorPhysical.SetTileFlags(currentTilePos, TileFlags.None);
                outdoorPhysical.SetColor(currentTilePos, new Color(perlin, perlin, perlin, 1));
                outdoorPhysical.RefreshTile(currentTilePos);

                perlin = GetNoiseValueOf(currentTilePos, currentBuilding.noise);
                indoorPhysical.SetTile(currentTilePos, noiseTile);
                indoorPhysical.SetTileFlags(currentTilePos, TileFlags.None);
                indoorPhysical.SetColor(currentTilePos, new Color(perlin, perlin, perlin, 1));
                indoorPhysical.RefreshTile(currentTilePos);
            }
        }
        
        if (!debugNoise)
        {
            indoorForeground.SetTiles(tilesToBeDrawn, indoorForegroundTiles);
            indoorPhysical.SetTiles(tilesToBeDrawn, indoorPhysicalTiles);
            indoorBackground.SetTiles(tilesToBeDrawn, indoorBackgroundTiles);
            outdoorForeground.SetTiles(tilesToBeDrawn, outdoorForegroundTiles);
            outdoorPhysical.SetTiles(tilesToBeDrawn, outdoorPhysicalTiles);
            outdoorBackground.SetTiles(tilesToBeDrawn, outdoorBackgroundTiles);
        }
    }

    private static Vector3Int[] GetChunkTilePositions(int chunkX, int chunkY, int chunkWidth, int chunkHeight)
    {
        int totalXTiles = chunkWidth;
        int totalYTiles = chunkHeight;
        int totalTiles = totalXTiles * totalYTiles;
        Vector3Int[] tilePositions = new Vector3Int[totalTiles];
        for (int tileIndex = 0; tileIndex < totalTiles; tileIndex++)
        {
            int tileX = tileIndex % totalXTiles;
            int tileY = tileIndex / totalXTiles;
            int tilePosX = chunkX + tileX;
            int tilePosY = chunkY + tileY;
            tilePositions[tileIndex] = new Vector3Int(tilePosX, tilePosY, 0);
        }
        return tilePositions;
    }

    private void RemoveObjectsOutsideOf<T>(List<T> objects, ObjectPool<T> pool, int chunkStartPosX, int chunkStartPosY, int chunkWidth, int chunkHeight) where T : MonoBehaviour
    {
        for (int i = objects.Count - 1; i >= 0; i--)
        {
            Vector3Int doorPos = outdoorPhysical.WorldToCell(objects[i].transform.position);
            // if (System.Array.IndexOf(tiles, doorPos) < 0)
            if (!(doorPos.x >= chunkStartPosX && doorPos.x < chunkStartPosX + chunkWidth && doorPos.y >= chunkStartPosY && doorPos.y < chunkStartPosY + chunkHeight))
            {
                pool.Return(objects[i]);
                objects.RemoveAt(i);
            }
        }
    }
    private void ClearTiles(params Vector3Int[] tilesToBeCleared)
    {
        var nullTiles = new Tile[tilesToBeCleared.Length];
        indoorForeground.SetTiles(tilesToBeCleared, nullTiles);
        indoorPhysical.SetTiles(tilesToBeCleared, nullTiles);
        indoorBackground.SetTiles(tilesToBeCleared, nullTiles);
        outdoorForeground.SetTiles(tilesToBeCleared, nullTiles);
        outdoorPhysical.SetTiles(tilesToBeCleared, nullTiles);
        outdoorBackground.SetTiles(tilesToBeCleared, nullTiles);
    }
    private float GetNoiseValueOf(Vector3Int tilePosition, NoiseInfo noiseInfo)
    {
        float rawNoise = 1;
        if (noise != null)
        {
            noise.SetNoiseType(noiseInfo.noiseType);
            noise.SetFractalOctaves(noiseInfo.octaves);
            noise.SetFrequency(noiseInfo.frequency);
            rawNoise = noise.GetNoise(tilePosition.x, tilePosition.y);
            rawNoise = Mathf.Pow((rawNoise + 1) / 2, noiseInfo.power);
        }
        return rawNoise;
    }
    private bool NoiseCheck(Vector3Int tilePosition, NoiseInfo noiseInfo)
    {
        float perlin = GetNoiseValueOf(tilePosition, noiseInfo);
        return noiseInfo.invert && perlin <= noiseInfo.threshold || !noiseInfo.invert && perlin >= noiseInfo.threshold;
    }
}
