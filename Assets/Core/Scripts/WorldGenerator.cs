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
    public Door trapDoorPrefab;
    private ObjectPool<Door> trapDoorPool;
    private List<Door> trapDoors = new List<Door>();

    [Space(10)]
    // public bool isIndoors;
    public Tilemap indoorForeground;
    public Tilemap indoorPhysical;
    public Tilemap indoorBackground;
    public Tilemap outdoorForeground;
    public Tilemap outdoorPhysical;
    public Tilemap outdoorBackground;

    [Space(10)]
    // public Transform target;
    private Dictionary<Transform, TargetData> targets = new Dictionary<Transform, TargetData>();

    [Space(10)]
    // public Ore orePrefab;
    public NoiseInfo oreNoise;
    public OreScriptableObject oreInfo;
    public BiomeInfo currentBiome;
    public BuildingScriptableObject buildingsInfo;
    public BuildingInfo currentBuilding;
    private BackgroundLoop currentOutdoorBackground;
    // private ObjectPool<Ore> oresPool;
    // private List<Ore> ores = new List<Ore>();
    private Dictionary<Vector3Int, OreData> oreMap = new Dictionary<Vector3Int, OreData>();

    private bool firstDraw = true;
    private FastNoise noise;

    void Start()
    {
        noise = new FastNoise(WorldData.seed);
        Transform doorsParent = new GameObject("Doors").transform;
        // Transform oresParent = new GameObject("Ores").transform;
        doorsPool = new ObjectPool<DoorController>(doorPrefab, 5, false, true, doorsParent);
        // oresPool = new ObjectPool<Ore>(orePrefab, 5, false, true, oresParent);
        trapDoorPool = new ObjectPool<Door>(trapDoorPrefab, 5, false, true, doorsParent);
    }
    void Update()
    {
        // indoorPhysical.transform.parent.gameObject.SetActive(isIndoors);
        // outdoorPhysical.transform.parent.gameObject.SetActive(!isIndoors);
        if (firstDraw)
        {
            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(outdoorPhysical.transform.parent);
            currentOutdoorBackground = backgroundObject.AddComponent<BackgroundLoop>();
            currentOutdoorBackground.background = currentBiome.background;
        }

        // if (currentOutdoorBackground != null)
        //     currentOutdoorBackground.target = targetDuo.Key;
        GenerateTerrain(targets, false);
    }

    public void AddTarget(Transform target, Vector2Int renderSize)
    {
        if (!targets.ContainsKey(target))
            targets.Add(target, new TargetData(renderSize));
        else
            Debug.LogError("Cannot add a target that already exists in the world generator dictionary");
    }
    public void RemoveTarget(Transform target)
    {
        if (targets.ContainsKey(target))
            targets.Remove(target);
        else
            Debug.LogError("Cannot remove a target that is not in the world generator dictionary");
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

    public void GenerateTerrain(Dictionary<Transform, TargetData> allTargets, bool forceAll)
    {        
        bool bigChange = forceAll || firstDraw || debugNoise != prevDebugNoise;
        bool smallChange = false;
        IEnumerable<Vector3Int> allNewTiles = new Vector3Int[0];
        IEnumerable<Vector3Int> allOldTiles = new Vector3Int[0];
        foreach (var target in allTargets)
        {
            target.Value.prevChunkStart = target.Value.chunkStart;
            target.Value.chunkStart = new Vector2Int(Mathf.FloorToInt(target.Key.position.x - (target.Value.renderSize.x / 2f)), Mathf.FloorToInt(target.Key.position.y - (target.Value.renderSize.y / 2f)));
            if (target.Value.chunkStart != target.Value.prevChunkStart)
                smallChange = true;
            
            var currentChunk = new RectInt(target.Value.chunkStart, target.Value.renderSize);
            allNewTiles = allNewTiles.Union(GetChunkTilePositions(currentChunk));
            var prevChunk = new RectInt(target.Value.prevChunkStart, target.Value.renderSize);
            allOldTiles = allOldTiles.Union(GetChunkTilePositions(prevChunk));
        }

        if (smallChange || bigChange)
        {
            Vector3Int[] tilesToBeDrawn = new Vector3Int[0];
            if (!bigChange)
            {
                tilesToBeDrawn = allNewTiles.Except(allOldTiles).ToArray(); //Get exclusive new tiles
                var tilesToBeCleared = allOldTiles.Except(allNewTiles).ToArray(); //Get exclusive old tiles
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
                tilesToBeDrawn = allNewTiles.ToArray();
                oreMap.Clear();
            }

            // RemoveObjectsOutsideOf(doors, doorsPool, currentChunk);
            // RemoveObjectsOutsideOf(trapDoors, trapDoorPool, currentChunk);
            DrawTiles(tilesToBeDrawn); //Add exclusive new tiles to grid //When redrawing an area, doors may double spawn
            firstDraw = false;
            prevDebugNoise = debugNoise;
        }
    }
    // public void GenerateTerrain(Transform target, TargetData targetData, bool forceAll)
    // {        
    //     targetData.prevChunkStart = targetData.chunkStart;
    //     targetData.chunkStart = new Vector2Int(Mathf.FloorToInt(target.position.x - (targetData.renderSize.x / 2f)), Mathf.FloorToInt(target.position.y - (targetData.renderSize.y / 2f)));
    //     var currentChunk = new RectInt(targetData.chunkStart, targetData.renderSize);
    //     var prevChunk = new RectInt(targetData.prevChunkStart, targetData.renderSize);
    //     DebugPanel.Log("Position", target.position.xy(), 5);

    //     bool smallChange = targetData.chunkStart != targetData.prevChunkStart;
    //     bool bigChange = forceAll || firstDraw || targetData.renderSize != targetData.prevRenderSize || debugNoise != prevDebugNoise;
    //     if (smallChange || bigChange)
    //     {
    //         // Debug.Log(chunkX + ", " + chunkY);

    //         var newTilePositions = GetChunkTilePositions(currentChunk); //Get all new tile indices
    //         Vector3Int[] tilesToBeDrawn = null;

    //         if (!bigChange)
    //         {
    //             //Check if you're overlapping other targets' chunks and remove their tiles from being drawn
    //             foreach (var otherTarget in targets)
    //             {
    //                 if (otherTarget.Key == target)
    //                     continue;
                    
    //                 RectInt otherChunk = new RectInt(otherTarget.Value.chunkStart, otherTarget.Value.renderSize);
    //                 if (otherChunk.Overlaps(currentChunk))
    //                     newTilePositions = newTilePositions.Except(GetChunkTilePositions(otherChunk)).ToArray();
    //             }

    //             if (newTilePositions.Length > 0)
    //             {
    //                 // drawnChunks.Add(currentChunk); //Not shadowed by other chunk so add to drawn chunks
    //                 var oldTilePositions = GetChunkTilePositions(prevChunk); //Get all old tile indices
    //                 tilesToBeDrawn = newTilePositions.Except(oldTilePositions).ToArray(); //Get exclusive new tiles
    //                 var tilesToBeCleared = oldTilePositions.Except(newTilePositions).ToArray(); //Get exclusive old tiles
    //                 ClearTiles(tilesToBeCleared); //Clear exclusive old tiles from grid
    //                 ClearOreMapOf(tilesToBeCleared);
    //             }
    //         }
    //         else
    //         {
    //             indoorForeground.ClearAllTiles();
    //             indoorPhysical.ClearAllTiles();
    //             indoorBackground.ClearAllTiles();
    //             outdoorForeground.ClearAllTiles();
    //             outdoorPhysical.ClearAllTiles();
    //             outdoorBackground.ClearAllTiles();
    //             tilesToBeDrawn = newTilePositions;
    //             oreMap.Clear();
    //         }

    //         if (tilesToBeDrawn != null)
    //         {
    //             RemoveObjectsOutsideOf(doors, doorsPool, currentChunk);
    //             RemoveObjectsOutsideOf(trapDoors, trapDoorPool, currentChunk);
    //             // RemoveObjectsOutsideOf(ores, oresPool, newTilePositions);
    //             DrawTiles(tilesToBeDrawn); //Add exclusive new tiles to grid //When redrawing an area, doors may double spawn
    //         }

    //         firstDraw = false;
    //         targetData.prevRenderSize = targetData.renderSize;
    //         prevDebugNoise = debugNoise;
    //     }
    // }
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
        DrawTiles(GetChunkTilePositions(new RectInt(tileIndex.x - 1, tileIndex.y - 1, 3, 3)).ToArray()); //When redrawing an area, doors may double spawn
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

                            float buildingTilePerlin = GetNoiseValueOf(currentTilePos, currentBuilding.noise);

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
                            else if (currentTilePos.y > buildingCorner.y + 1 && NoiseCheck(buildingTilePerlin, currentBuilding.noise) && !WorldData.IsDug(currentTilePos, true)) //Inner walls
                            {
                                if (currentTilePos.x == buildingCorner.x + 1) //Near left edge, so make sure has left border
                                    hasLeftTile = true;
                                if (currentTilePos.x == buildingCorner.x + buildingMaxWidth - 2) //Near right edge, so make sure has right border
                                    hasRightTile = true;
                                if (currentTilePos.y == buildingCorner.y + 2) //Near bottom edge, so make sure does not have bottom border since first tile is always empty
                                    hasBotTile = false;
                                if (currentTilePos.y == buildingCorner.y + buildingMaxHeight - 2) //Near top edge, so make sure has top border
                                    hasTopTile = true;

                                if (!hasBotTile && !hasTopTile && buildingTilePerlin >= currentBuilding.noise.threshold * 1.1f)
                                {
                                    // WorldData.SetDug(currentTilePos, true);
                                    trapDoors.Add(trapDoorPool.Get(trapDoor => { trapDoor.transform.position = outdoorPhysical.CellToWorld(currentTilePos); }));
                                }
                                else
                                {
                                    borderIndex = (hasLeftTile ? 1 : 0) + (hasTopTile ? 2 : 0) + (hasRightTile ? 4 : 0) + (hasBotTile ? 8 : 0);
                                    indoorPhysicalTiles[i] = currentBuilding.wallTile[borderIndex];
                                }
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

    private static IEnumerable<Vector3Int> GetChunkTilePositions(RectInt chunk)
    {
        int totalXTiles = chunk.size.x;
        int totalYTiles = chunk.size.y;
        int totalTiles = totalXTiles * totalYTiles;
        Vector3Int[] tilePositions = new Vector3Int[totalTiles];
        for (int tileIndex = 0; tileIndex < totalTiles; tileIndex++)
        {
            int tileX = tileIndex % totalXTiles;
            int tileY = tileIndex / totalXTiles;
            int tilePosX = chunk.xMin + tileX;
            int tilePosY = chunk.yMin + tileY;
            tilePositions[tileIndex] = new Vector3Int(tilePosX, tilePosY, 0);
        }
        return tilePositions;
    }

    private void RemoveObjectsOutsideOf<T>(List<T> objects, ObjectPool<T> pool, RectInt chunk) where T : MonoBehaviour
    {
        for (int i = objects.Count - 1; i >= 0; i--)
        {
            Vector3Int doorPos = outdoorPhysical.WorldToCell(objects[i].transform.position);
            // if (System.Array.IndexOf(tiles, doorPos) < 0)
            if (!(doorPos.x >= chunk.xMin && doorPos.x < chunk.xMax && doorPos.y >= chunk.yMin && doorPos.y < chunk.yMax))
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
        return NoiseCheck(perlin, noiseInfo);
    }
    private bool NoiseCheck(float perlinValue, NoiseInfo noiseInfo)
    {
        return noiseInfo.invert && perlinValue <= noiseInfo.threshold || !noiseInfo.invert && perlinValue >= noiseInfo.threshold;
    }

    public class TargetData
    {
        // public Transform target;
        /// <summary>
        /// The chunk the target is currently in represented by the bottom left corner tile index
        /// </summary>
        public Vector2Int chunkStart;
        /// <summary>
        /// The chunk the target was in in the previous frame
        /// </summary>
        public Vector2Int prevChunkStart;

        /// <summary>
        /// How many tiles should be visible at once
        /// </summary>
        public Vector2Int renderSize;
        public Vector2Int prevRenderSize;

        public TargetData(Vector2Int renderSize)
        {
            this.renderSize = renderSize;
        }
    }
}