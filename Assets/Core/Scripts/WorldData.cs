using System.Collections.Generic;
using UnityEngine;

public class WorldData
{
    public static int chunkWidth = 8; //Is the global setting and will be used to set the terrain generator values
    public static int chunkHeight = 8; //Is the global setting and will be used to set the terrain generator values
    public static int chunkRenderDistance = 1; //Is the global setting and will be used to set the terrain generator values

    private static Dictionary<Vector2Int, List<Vector2Int>> dug = new Dictionary<Vector2Int, List<Vector2Int>>(); //The tiles that have been dug around the world

    public static void SetDug(Vector2Int tilePosition)
    {
        var chunk = GetChunkFromTile(tilePosition);
        if (!dug.ContainsKey(chunk)) //If the chunk list doesn't exits, add it
            dug[chunk] = new List<Vector2Int>();
        if (!dug[chunk].Contains(tilePosition)) //If the tile isn't already set as dug, set it
            dug[chunk].Add(tilePosition);
    }
    public static void SetDug(Vector3Int tilePosition)
    {
        SetDug(new Vector2Int(tilePosition.x, tilePosition.y));
    }
    public static bool IsDug(Vector2Int tilePosition)
    {
        var chunk = GetChunkFromTile(tilePosition);
        bool isDug = false;
        if (dug.ContainsKey(chunk))
            isDug = dug[chunk].Contains(tilePosition);
        return isDug;
    }
    public static bool IsDug(Vector3Int tilePosition)
    {
        return IsDug(new Vector2Int(tilePosition.x, tilePosition.y));
    }

    public static Vector2Int GetChunkFromTile(Vector2Int tilePosition)
    {
        int chunkX = ((tilePosition.x - (chunkWidth / 2)) / chunkWidth) + 1;
        int chunkY = ((tilePosition.y - (chunkHeight / 2)) / chunkHeight) + 1;
        return new Vector2Int(chunkX, chunkY);
    }
    public static Vector2Int GetChunkFromTile(Vector3Int tilePosition)
    {
        return GetChunkFromTile(new Vector2Int(tilePosition.x, tilePosition.y));
    }
    public static Vector2Int GetChunkFromPosition(Vector2 position)
    {
        int chunkX = Mathf.FloorToInt((position.x - (chunkWidth / 2)) / chunkWidth) + 1;
        int chunkY = Mathf.FloorToInt((position.y - (chunkHeight / 2)) / chunkHeight) + 1;
        return new Vector2Int(chunkX, chunkY);
    }
    public static Vector2Int GetChunkFromPosition(Vector3 position)
    {
        return GetChunkFromPosition(new Vector2(position.x, position.y));
    }
}
