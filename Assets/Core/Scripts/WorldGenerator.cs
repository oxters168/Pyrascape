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
    public float noiseThreshold = 0.42f;
    public bool invertNoise;
    private bool prevInvertNoise;
    [Space(10)]
    public Tile noiseTile;
    public FastNoise.NoiseType noiseType = FastNoise.NoiseType.Perlin;
    public float noiseFrequency = 0.01f;
    private float prevNoiseFrequency = 0.01f;
    public int noiseOctaves = 3;
    public float noisePower = 1;
    private float prevNoisePower = 1;
    public bool debugNoise;
    private bool prevDebugNoise;

    [Space(10)]
    public Door doorPrefab;
    private ObjectPool<Door> doorsPool;
    public int buildingMaxWidth = 8;
    public int buildingMaxHeight = 8;
    public int buildingCushionX = 8;
    public int buildingCushionY = 8;
    private List<Door> doors = new List<Door>();

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
    [Tooltip("The number of horizontal tiles per chunk. Should be even, if not even will be floor(half) x 2 of the odd number to make it even")]
    public int chunkWidth = 8;
    private int prevChunkWidth = 8;
    [Tooltip("The number of vertical tiles per chunk. Should be even, if not even will be floor(half) x 2 of the odd number to make it even")]
    public int chunkHeight = 8;
    private int prevChunkHeight = 8;
    [Tooltip("How many chunks beyond the current to render (0 renders 1 chunk, 1 renders 9 chunks, 2 renders 25 chunks...)")]
    public int chunkRenderDistance = 1;
    private int prevChunkRenderDistance = 1;

    public BiomeInfo currentBiome;
    private BackgroundLoop currentBackground;

    /// <summary>
    /// The chunk the target is currently in
    /// </summary>
    private Vector2Int chunk;
    /// <summary>
    /// The chunk the target was in
    /// </summary>
    private Vector2Int prevChunk;

    private bool firstDraw = true;
    private FastNoise noise;

    void Start()
    {
        noise = new FastNoise(WorldData.seed);
        noise.SetNoiseType(noiseType);

        doorsPool = new ObjectPool<Door>(doorPrefab, 5, false, true, transform);
    }
    void Update()
    {
        chunkWidth = (chunkWidth / 2) * 2; //Turns the width into an even number (since integer division is floored by default)
        chunkHeight = (chunkHeight / 2) * 2; //Turns the height into an even number (since integer division is floored by default)
        prevChunk = chunk;
        chunk = new Vector2Int(Mathf.FloorToInt(((target.position.x - (chunkWidth / 2f)) / chunkWidth) + 1),
            Mathf.FloorToInt(((target.position.y - (chunkHeight / 2f)) / chunkHeight) + 1));
        // chunk = GetChunkFromPosition(target.position);
        DebugPanel.Log("Position", target.position.xy(), 5);
        DebugPanel.Log("Chunk", chunk, 5);

        if (currentBackground != null)
            currentBackground.target = target;
        if (firstDraw)
        {
            GameObject backgroundObject = new GameObject("Background");
            currentBackground = backgroundObject.AddComponent<BackgroundLoop>();
            currentBackground.background = currentBiome.background;
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
        bool bigChange = forceAll || firstDraw || chunkWidth != prevChunkWidth || chunkHeight != prevChunkHeight || chunkRenderDistance != prevChunkRenderDistance || !Mathf.Approximately(noiseFrequency, prevNoiseFrequency) || !Mathf.Approximately(noisePower, prevNoisePower) || invertNoise != prevInvertNoise || debugNoise != prevDebugNoise;
        if (smallChange || bigChange)
        {
            // Debug.Log(chunkX + ", " + chunkY);

            var newTilePositions = GetChunkTilePositions(chunk.x, chunk.y, chunkWidth, chunkHeight, chunkRenderDistance); //Get all new tile indices
            Vector3Int[] tilesToBeDrawn;

            if (!bigChange)
            {
                var oldTilePositions = GetChunkTilePositions(prevChunk.x, prevChunk.y, chunkWidth, chunkHeight, chunkRenderDistance); //Get all old tile indices
                tilesToBeDrawn = newTilePositions.Except(oldTilePositions).ToArray(); //Get exclusive new tiles

                var tilesToBeCleared = oldTilePositions.Except(newTilePositions).ToArray(); //Get exclusive old tiles
                ClearTiles(tilesToBeCleared); //Clear exclusive old tiles from grid
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
            }

            RemoveDoorsOutsideOf(newTilePositions);
            DrawTiles(tilesToBeDrawn); //Add exclusive new tiles to grid

            firstDraw = false;
            prevChunkWidth = chunkWidth;
            prevChunkHeight = chunkHeight;
            prevChunkRenderDistance = chunkRenderDistance;
            prevNoiseFrequency = noiseFrequency;
            prevNoisePower = noisePower;
            prevInvertNoise = invertNoise;
            prevDebugNoise = debugNoise;
        }
    }
    private void DrawTiles(params Vector3Int[] tilesToBeDrawn)
    {
        var indoorForegroundTiles = new Tile[tilesToBeDrawn.Length];
        var indoorPhysicalTiles = new Tile[tilesToBeDrawn.Length];
        var indoorBackgroundTiles = new Tile[tilesToBeDrawn.Length];
        var outdoorForegroundTiles = new Tile[tilesToBeDrawn.Length];
        var outdoorPhysicalTiles = new Tile[tilesToBeDrawn.Length];
        var outdoorBackgroundTiles = new Tile[tilesToBeDrawn.Length];
        noise.SetNoiseType(noiseType);
        noise.SetFractalOctaves(noiseOctaves);
        noise.SetFrequency(noiseFrequency);

        for (int i = 0; i < outdoorPhysicalTiles.Length; i++)
        {
            var currentTilePos = tilesToBeDrawn[i];

            //Generate terrain
            if (!debugNoise)
            {
                //Ornament tiles
                if (currentTilePos.y == 0 && !WorldData.IsDug(currentTilePos + Vector3Int.down))
                    outdoorForegroundTiles[i] = currentBiome.ornamentalTile;

                //Background and physical tiles
                if (currentTilePos.y <= surfaceHeight)
                {
                    outdoorBackgroundTiles[i] = currentBiome.backgroundTile; //Background tiles

                    int borderIndex = 0;
                    Vector3Int leftTilePos = currentTilePos + Vector3Int.left;
                    Vector3Int upperTilePos = currentTilePos + Vector3Int.up;
                    Vector3Int rightTilePos = currentTilePos + Vector3Int.right;
                    Vector3Int lowerTilePos = currentTilePos + Vector3Int.down;
                    bool hasLeftTile = !WorldData.IsDug(leftTilePos);
                    bool hasTopTile = currentTilePos.y < surfaceHeight && !WorldData.IsDug(upperTilePos);
                    bool hasRightTile = !WorldData.IsDug(rightTilePos);
                    bool hasBotTile = !WorldData.IsDug(lowerTilePos) && PerlinCheck(lowerTilePos);
                    if (currentTilePos.y < surfaceHeight)
                    {
                        hasLeftTile &= PerlinCheck(leftTilePos);
                        hasRightTile &= PerlinCheck(rightTilePos);
                    }
                    if (currentTilePos.y < surfaceHeight - 1)
                        hasTopTile &= PerlinCheck(upperTilePos);
                    borderIndex = (hasLeftTile ? 1 : 0) + (hasTopTile ? 2 : 0) + (hasRightTile ? 4 : 0) + (hasBotTile ? 8 : 0);

                    if (currentTilePos.y == surfaceHeight && !WorldData.IsDug(currentTilePos))
                        outdoorPhysicalTiles[i] = currentBiome.surfaceTile[borderIndex];

                    if (currentTilePos.y < surfaceHeight && PerlinCheck(currentTilePos) && !WorldData.IsDug(currentTilePos))
                        outdoorPhysicalTiles[i] = currentBiome.undergroundTile[borderIndex];

                    if (currentTilePos.y < surfaceHeight - 1)
                    {
                        //Calculate building's start corner
                        Vector3Int buildingCorner = new Vector3Int(Mathf.FloorToInt(((float)currentTilePos.x) / (buildingMaxWidth + buildingCushionX)) * (buildingMaxWidth + buildingCushionX), Mathf.FloorToInt(((float)currentTilePos.y) / (buildingMaxHeight + buildingCushionY)) * (buildingMaxHeight + buildingCushionY), 0);
                        
                        //Add door to building
                        if (currentTilePos.x == buildingCorner.x + 1 && currentTilePos.y == buildingCorner.y + 1)
                            doors.Add(doorsPool.Get(door => door.transform.position = outdoorPhysical.CellToWorld(currentTilePos) + Vector3.right * 0.5f));
                        
                        //Populate building
                        if (currentTilePos.x >= buildingCorner.x && currentTilePos.x < (buildingCorner.x + buildingMaxWidth) && currentTilePos.y >= buildingCorner.y && currentTilePos.y < (buildingCorner.y + buildingMaxHeight))
                        {
                            //Place edges of the building
                            if (currentTilePos.x == buildingCorner.x || currentTilePos.x == (buildingCorner.x + buildingMaxWidth - 1) || currentTilePos.y == buildingCorner.y || currentTilePos.y == (buildingCorner.y + buildingMaxHeight - 1))
                                indoorPhysicalTiles[i] = currentBiome.undergroundTile[15];
                        }
                    }
                }
            }
            else
            {
                float perlin = GetPerlinOf(currentTilePos);
                outdoorPhysical.SetTile(currentTilePos, noiseTile);
                outdoorPhysical.SetTileFlags(currentTilePos, TileFlags.None);
                outdoorPhysical.SetColor(currentTilePos, new Color(perlin, perlin, perlin, 1));
                outdoorPhysical.RefreshTile(currentTilePos);
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

    private static Vector3Int[] GetChunkTilePositions(int chunkX, int chunkY, int chunkWidth, int chunkHeight, int chunkRenderDistance)
    {
        int totalXTiles = (chunkWidth * chunkRenderDistance * 2) + chunkWidth;
        int totalYTiles = (chunkHeight * chunkRenderDistance * 2) + chunkHeight;
        int totalTiles = totalXTiles * totalYTiles;
        Vector3Int[] tilePositions = new Vector3Int[totalTiles];
        for (int tileIndex = 0; tileIndex < totalTiles; tileIndex++)
        {
            int tileX = tileIndex % totalXTiles;
            int tileY = tileIndex / totalXTiles;
            int tilePosX = (tileX + (chunkX * chunkWidth)) - (totalXTiles / 2);
            int tilePosY = (tileY + (chunkY * chunkHeight)) - (totalYTiles / 2);
            tilePositions[tileIndex] = new Vector3Int(tilePosX, tilePosY, 0);
        }
        return tilePositions;
    }

    private void RemoveDoorsOutsideOf(params Vector3Int[] tiles)
    {
        for (int i = doors.Count - 1; i >= 0; i--)
        {
            Vector3Int doorPos = outdoorPhysical.WorldToCell(doors[i].transform.position);
            if (System.Array.IndexOf(tiles, doorPos) < 0)
            {
                doorsPool.Return(doors[i]);
                doors.RemoveAt(i);
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
    private float GetPerlinOf(Vector3Int tilePosition)
    {
        float rawNoise = noise.GetNoise(tilePosition.x, tilePosition.y);
        return Mathf.Pow((rawNoise + 1) / 2, noisePower);
    }
    private bool PerlinCheck(Vector3Int tilePosition)
    {
        float perlin = GetPerlinOf(tilePosition);
        return invertNoise && perlin <= noiseThreshold || !invertNoise && perlin >= noiseThreshold;
    }
}
