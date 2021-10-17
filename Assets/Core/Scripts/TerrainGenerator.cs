using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class TerrainGenerator : MonoBehaviour
{
    private CompositeCollider2D _physicsBounds;
    private CompositeCollider2D PhysicsBounds { get { if (_physicsBounds == null) _physicsBounds = GetComponentInChildren<CompositeCollider2D>(); return _physicsBounds; } }

    public Tilemap foreground, physical, background;

    [Space(10)]
    public Transform target;
    [Tooltip("The number of horizontal tiles per chunk. Should be even, if not even will be floor(half) x 2 of the odd number to make it even")]
    public int chunkWidth = 8;
    [Tooltip("The number of vertical tiles per chunk. Should be even, if not even will be floor(half) x 2 of the odd number to make it even")]
    public int chunkHeight = 8;
    [Tooltip("How many chunks beyond the current to render (0 renders 1 chunk, 1 renders 9 chunks, 2 renders 25 chunks...)")]
    public int chunkRenderDistance = 1;

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

    void Update()
    {
        chunkWidth = (chunkWidth / 2) * 2; //Turns the width into an even number (since integer division is floored by default)
        chunkHeight = (chunkHeight / 2) * 2; //Turns the height into an even number (since integer division is floored by default)
        prevChunk = chunk;
        chunk = WorldData.GetChunkFromPosition(target.position);

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

        for (int i = 0; i < physicalTiles.Length; i++)
        {
            var currentTilePos = tilesToBeDrawn[i];
            if (currentTilePos.y == 1)
            {
                if (!WorldData.IsDug(currentTilePos + Vector3Int.down))
                    foregroundTiles[i] = currentBiome.ornamentalTile;
            }
            
            if (currentTilePos.y == 0)
            {
                if (!WorldData.IsDug(currentTilePos))
                    physicalTiles[i] = currentBiome.surfaceTile;
                backgroundTiles[i] = currentBiome.backgroundTile;
            }

            if (currentTilePos.y < 0)
            {
                if (!WorldData.IsDug(currentTilePos))
                    physicalTiles[i] = currentBiome.undergroundTile;
                backgroundTiles[i] = currentBiome.backgroundTile;
            }
        }

        foreground.SetTiles(tilesToBeDrawn, foregroundTiles);
        physical.SetTiles(tilesToBeDrawn, physicalTiles);
        background.SetTiles(tilesToBeDrawn, backgroundTiles);
    }
    private void ClearTiles(params Vector3Int[] tilesToBeCleared)
    {
        var nullTiles = new Tile[tilesToBeCleared.Length];
        foreground.SetTiles(tilesToBeCleared, nullTiles);
        physical.SetTiles(tilesToBeCleared, nullTiles);
        background.SetTiles(tilesToBeCleared, nullTiles);
    }

    private void GenerateTerrain()
    {
        if (firstDraw || chunk != prevChunk)
        {
            // Debug.Log(chunkX + ", " + chunkY);

            var newTilePositions = GetChunkTilePositions(chunk.x, chunk.y, chunkWidth, chunkHeight, chunkRenderDistance); //Get all new tile indices
            Vector3Int[] tilesToBeDrawn;

            if (!firstDraw)
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
            // PhysicsBounds.GenerateGeometry(); //Regenerate collider

            // firstDraw = false;
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
