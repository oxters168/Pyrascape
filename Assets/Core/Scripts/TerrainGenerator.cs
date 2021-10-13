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

    /// <summary>
    /// The horizontal chunk the target is currently in
    /// </summary>
    private int chunkX;
    /// <summary>
    /// The vertical chunk the target is currently in
    /// </summary>
    private int chunkY;
    /// <summary>
    /// The previous horizontal chunk the target was in
    /// </summary>
    private int prevChunkX = int.MinValue;
    /// <summary>
    /// The previous vertical chunk the target was in
    /// </summary>
    private int prevChunkY = int.MinValue;

    void Update()
    {
        chunkWidth = (chunkWidth / 2) * 2; //Turns the width into an even number (since integer division is floored by default)
        chunkHeight = (chunkHeight / 2) * 2; //Turns the height into an even number (since integer division is floored by default)
        chunkX = Mathf.FloorToInt((target.position.x - (chunkWidth / 2)) / (chunkWidth / 1)) + 1;
        chunkY = Mathf.FloorToInt((target.position.y - (chunkHeight / 2)) / (chunkHeight / 1)) + 1;

        if (chunkX != prevChunkX || chunkY != prevChunkY)
        {
            // Debug.Log(chunkX + ", " + chunkY);

            var newTilePositions = GetChunkTilePositions(chunkX, chunkY, chunkWidth, chunkHeight, chunkRenderDistance); //Get all new tile indices
            Vector3Int[] tilesToBeDrawn;

            if (prevChunkX > int.MinValue && prevChunkY > int.MinValue)
            {
                var oldTilePositions = GetChunkTilePositions(prevChunkX, prevChunkY, chunkWidth, chunkHeight, chunkRenderDistance); //Get all old tile indices
                tilesToBeDrawn = newTilePositions.Except(oldTilePositions).ToArray(); //Get exclusive new tiles

                var tilesToBeCleared = oldTilePositions.Except(newTilePositions).ToArray(); //Get exclusive old tiles
                ClearTiles(tilesToBeCleared); //Clear exclusive old tiles from grid
            }
            else
                tilesToBeDrawn = newTilePositions;

            DrawTiles(tilesToBeDrawn); //Add exclusive new tiles to grid
            // PhysicsBounds.GenerateGeometry(); //Regenerate collider

            prevChunkX = chunkX;
            prevChunkY = chunkY;
        }
    }

    private void DrawTiles(params Vector3Int[] tilesToBeDrawn)
    {
        var addedTiles = new Tile[tilesToBeDrawn.Length];
        for (int i = 0; i < addedTiles.Length; i++)
            addedTiles[i] = currentBiome.undergroundTile;
        foreground.SetTiles(tilesToBeDrawn, addedTiles);
        physical.SetTiles(tilesToBeDrawn, addedTiles);
        background.SetTiles(tilesToBeDrawn, addedTiles);
    }
    private void ClearTiles(params Vector3Int[] tilesToBeCleared)
    {
        var nullTiles = new Tile[tilesToBeCleared.Length];
        foreground.SetTiles(tilesToBeCleared, nullTiles);
        physical.SetTiles(tilesToBeCleared, nullTiles);
        background.SetTiles(tilesToBeCleared, nullTiles);
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
