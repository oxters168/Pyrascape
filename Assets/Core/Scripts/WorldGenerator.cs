using UnityEngine;
using UnityHelpers;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    public int surfaceHeight = -1;
    [Space(10)]
    public Tile noiseTile;
    public bool debugNoise;
    private bool prevDebugNoise;

    [Space(10)]
    public DoorController doorPrefab;
    private ObjectPool<DoorController> doorsPool;
    // public int buildingMaxWidth = 8;
    // public int buildingMaxHeight = 8;
    // public int buildingCushionX = 8;
    // public int buildingCushionY = 8;
    private List<DoorController> doors = new List<DoorController>();
    // public Door trapDoorPrefab;
    // private ObjectPool<Door> trapDoorPool;
    // private List<Door> trapDoors = new List<Door>();

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
    //public NoiseInfo oreNoise;
    public OreScriptableObject oreInfo;
    public BiomeInfo currentBiome;
    //public BuildingScriptableObject buildingsInfo;
    // public BuildingInfo currentBuilding;
    public BuildingInfo[] buildings;
    // private BackgroundLoop currentOutdoorBackground;
    private Dictionary<Vector3Int, OreData> oreMap = new Dictionary<Vector3Int, OreData>();

    private bool firstDraw = true;
    private static FastNoise noise;

    void Start()
    {
        Application.targetFrameRate = 60;
        noise = new FastNoise(WorldData.seed);
        Transform doorsParent = new GameObject("Doors").transform;
        // Transform oresParent = new GameObject("Ores").transform;
        doorsPool = new ObjectPool<DoorController>(doorPrefab, 5, false, true, doorsParent);
        // oresPool = new ObjectPool<Ore>(orePrefab, 5, false, true, oresParent);
        // trapDoorPool = new ObjectPool<Door>(trapDoorPrefab, 5, false, true, doorsParent);
    }
    void Update()
    {
        // indoorPhysical.transform.parent.gameObject.SetActive(isIndoors);
        // outdoorPhysical.transform.parent.gameObject.SetActive(!isIndoors);
        // if (firstDraw)
        // {
        //     GameObject backgroundObject = new GameObject("Background");
        //     backgroundObject.transform.SetParent(outdoorPhysical.transform.parent);
        //     currentOutdoorBackground = backgroundObject.AddComponent<BackgroundLoop>();
        //     // currentOutdoorBackground.background = currentBiome.background;
        // }

        // if (currentOutdoorBackground != null)
        //     currentOutdoorBackground.target = targetDuo.Key;
        GenerateTerrain(targets, false);
    }

    public void AddOrSetTarget(Transform target, Vector2Int renderSize)
    {
        if (!targets.ContainsKey(target))
            targets.Add(target, new TargetData(renderSize));
        else
            targets[target].renderSize = renderSize;
    }
    public void RemoveTarget(Transform target)
    {
        if (targets.ContainsKey(target))
            targets.Remove(target);
        else
            Debug.LogError("Cannot remove a target that is not in the world generator targets collection");
    }
    // public void SetTargetBGIndex(Transform target, int bgIndex = -1)
    // {
    //     if (targets.Contains(target))
    //         targets[target].SetBackground(bgIndex);
    //     else
    //         Debug.LogError("Cannot set background of a target that is not in the world generator targets collection");
    // }

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
            if (target.Value.chunkStart != target.Value.prevChunkStart || target.Value.renderSize != target.Value.prevRenderSize)
                smallChange = true;
            target.Value.prevRenderSize = target.Value.renderSize;
            
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
            //Debug.Log(tilesToBeDrawn == null);
            DrawTiles(tilesToBeDrawn); //Add exclusive new tiles to grid //When redrawing an area, doors may double spawn
            firstDraw = false;
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
                outdoorForegroundTiles[i] = currentBiome.GetOrnamentTileAt(currentTilePos, surfaceHeight);
                outdoorBackgroundTiles[i] = currentBiome.GetBackgroundTileAt(currentTilePos, surfaceHeight);
                outdoorPhysicalTiles[i] = currentBiome.GetPhysicalTileAt(currentTilePos, surfaceHeight);

                var currentOre = oreInfo.GetOreAt(currentTilePos, surfaceHeight, outdoorPhysicalTiles[i] != null);
                if (currentOre != null)
                {
                    outdoorForegroundTiles[i] = currentOre.ore;
                    oreMap[currentTilePos] = currentOre.Clone();
                }

                //Add door to building
                // if (currentTilePos.x == buildingCorner.x + 1 && currentTilePos.y == buildingCorner.y + 1)
                //     doors.Add(doorsPool.Get(door => { door.world = this; door.transform.position = outdoorPhysical.CellToWorld(currentTilePos) + Vector3.right * 0.5f; }));
                var currentBuilding = GetCurrentBuilding(buildings, currentTilePos);
                if (currentBuilding != null)
                {
                    indoorBackgroundTiles[i] = currentBuilding.GetBackgroundTileAt(currentTilePos);
                    indoorPhysicalTiles[i] = currentBuilding.GetPhysicalTileAt(currentTilePos);
                }
            }
            else
            {
                float perlin = GetNoiseValueOf(currentTilePos, currentBiome.noise);
                outdoorPhysical.SetTile(currentTilePos, noiseTile);
                outdoorPhysical.SetTileFlags(currentTilePos, TileFlags.None);
                outdoorPhysical.SetColor(currentTilePos, new Color(perlin, perlin, perlin, 1));
                outdoorPhysical.RefreshTile(currentTilePos);

                var currentBuilding = GetCurrentBuilding(buildings, currentTilePos);
                if (currentBuilding != null)
                {
                    perlin = GetNoiseValueOf(currentTilePos, currentBuilding.noise);
                    indoorPhysical.SetTile(currentTilePos, noiseTile);
                    indoorPhysical.SetTileFlags(currentTilePos, TileFlags.None);
                    indoorPhysical.SetColor(currentTilePos, new Color(perlin, perlin, perlin, 1));
                    indoorPhysical.RefreshTile(currentTilePos);
                }
                break;
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

    public static BuildingInfo GetCurrentBuilding(BuildingInfo[] buildings, Vector3Int currentTilePos)
    {
        BuildingInfo building = null;
        for (int i = 0; i < buildings.Length; i++)
        {
            if (IsInsideBuilding(buildings[i], currentTilePos))
            {
                building = buildings[i];
                break;
            }
        }
        return building;
    }
    private static bool IsInsideBuilding(BuildingInfo building, Vector3Int currentTilePos)
    {
        //Will later have psuedo randomized x position here
        return currentTilePos.x >= building.corner.x && currentTilePos.x < (building.corner.x + building.size.x) && currentTilePos.y >= building.corner.y && currentTilePos.y < (building.corner.y + building.size.y);
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
    public static float GetNoiseValueOf(Vector3Int tilePosition, NoiseInfo noiseInfo)
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
    public static bool NoiseCheck(Vector3Int tilePosition, NoiseInfo noiseInfo)
    {
        float perlin = GetNoiseValueOf(tilePosition, noiseInfo);
        return NoiseCheck(perlin, noiseInfo);
    }
    public static bool NoiseCheck(float perlinValue, NoiseInfo noiseInfo)
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

        private BackgroundLoop currentOutdoorBackground;

        public TargetData(Vector2Int renderSize)
        {
            this.renderSize = renderSize;
        }

        // public void SetBackground(int bgIndex = -1)
        // {
        //     if (bgIndex >= 0)
        //     {
        //         if (!currentOutdoorBackground)
        //         {
        //             GameObject backgroundObject = new GameObject("Background");
        //             backgroundObject.transform.SetParent(outdoorPhysical.transform.parent);
        //             currentOutdoorBackground = backgroundObject.AddComponent<BackgroundLoop>();
        //             currentOutdoorBackground.background = currentBiome.background;
        //         }
        //     }
        //     else
        //         Destroy(currentOutdoorBackground.gameObject);
        // }
    }
}