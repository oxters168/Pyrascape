using UnityEngine;
using UnityHelpers;
using UnityEngine.Tilemaps;
using System.Linq;

public class TerrainGenerator : MonoBehaviour
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
    public Tilemap foreground;
    public Tilemap physical;
    public Tilemap background;

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

        GenerateTerrain();

        firstDraw = false;
    }

    private void DrawTiles(params Vector3Int[] tilesToBeDrawn)
    {
        var foregroundTiles = new Tile[tilesToBeDrawn.Length];
        var physicalTiles = new Tile[tilesToBeDrawn.Length];
        var backgroundTiles = new Tile[tilesToBeDrawn.Length];
        noise.SetNoiseType(noiseType);
        noise.SetFractalOctaves(noiseOctaves);
        noise.SetFrequency(noiseFrequency);

        for (int i = 0; i < physicalTiles.Length; i++)
        {
            var currentTilePos = tilesToBeDrawn[i];

            //Generate terrain
            if (!debugNoise)
            {
                //Ornament tiles
                if (currentTilePos.y == 0 && !WorldData.IsDug(currentTilePos + Vector3Int.down))
                    foregroundTiles[i] = currentBiome.ornamentalTile;

                //Background and physical tiles
                if (currentTilePos.y <= surfaceHeight)
                {
                    backgroundTiles[i] = currentBiome.backgroundTile; //Background tiles

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
                        physicalTiles[i] = currentBiome.surfaceTile[borderIndex];

                    if (currentTilePos.y < surfaceHeight && PerlinCheck(currentTilePos) && !WorldData.IsDug(currentTilePos))
                        physicalTiles[i] = currentBiome.undergroundTile[borderIndex];
                }
            }
            else
            {
                float perlin = GetPerlinOf(currentTilePos);
                physical.SetTile(currentTilePos, noiseTile);
                physical.SetTileFlags(currentTilePos, TileFlags.None);
                physical.SetColor(currentTilePos, new Color(perlin, perlin, perlin, 1));
                physical.RefreshTile(currentTilePos);
            }
        }
        
        if (!debugNoise)
        {
            foreground.SetTiles(tilesToBeDrawn, foregroundTiles);
            physical.SetTiles(tilesToBeDrawn, physicalTiles);
            background.SetTiles(tilesToBeDrawn, backgroundTiles);
        }
    }
    private void ClearTiles(params Vector3Int[] tilesToBeCleared)
    {
        var nullTiles = new Tile[tilesToBeCleared.Length];
        foreground.SetTiles(tilesToBeCleared, nullTiles);
        physical.SetTiles(tilesToBeCleared, nullTiles);
        background.SetTiles(tilesToBeCleared, nullTiles);
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

    private void GenerateTerrain()
    {
        bool changeHappens = firstDraw || chunk != prevChunk || chunkWidth != prevChunkWidth || chunkHeight != prevChunkHeight || chunkRenderDistance != prevChunkRenderDistance || !Mathf.Approximately(noiseFrequency, prevNoiseFrequency) || !Mathf.Approximately(noisePower, prevNoisePower) || invertNoise != prevInvertNoise || debugNoise != prevDebugNoise;
        if (changeHappens)
        {
            // Debug.Log(chunkX + ", " + chunkY);

            var newTilePositions = GetChunkTilePositions(chunk.x, chunk.y, chunkWidth, chunkHeight, chunkRenderDistance); //Get all new tile indices
            Vector3Int[] tilesToBeDrawn;

            if (!changeHappens)
            {
                var oldTilePositions = GetChunkTilePositions(prevChunk.x, prevChunk.y, chunkWidth, chunkHeight, chunkRenderDistance); //Get all old tile indices
                tilesToBeDrawn = newTilePositions.Except(oldTilePositions).ToArray(); //Get exclusive new tiles

                var tilesToBeCleared = oldTilePositions.Except(newTilePositions).ToArray(); //Get exclusive old tiles
                ClearTiles(tilesToBeCleared); //Clear exclusive old tiles from grid
            }
            else
            {
                foreground.ClearAllTiles();
                physical.ClearAllTiles();
                background.ClearAllTiles();
                tilesToBeDrawn = newTilePositions;
            }

            DrawTiles(tilesToBeDrawn); //Add exclusive new tiles to grid

            // firstDraw = false;
            prevNoiseFrequency = noiseFrequency;
            prevNoisePower = noisePower;
            prevInvertNoise = invertNoise;
            prevDebugNoise = debugNoise;
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
}
