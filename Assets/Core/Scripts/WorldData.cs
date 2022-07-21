using System.Collections.Generic;
using UnityEngine;

public class WorldData
{
    public static int seed = 1337;

    private static List<Vector2Int> indoorDug = new List<Vector2Int>();
    private static List<Vector2Int> outdoorDug = new List<Vector2Int>();

    public static void SetDug(Vector2Int tilePosition, bool indoor)
    {
        if (indoor && !indoorDug.Contains(tilePosition))
            indoorDug.Add(tilePosition);
        else if (!indoor && !outdoorDug.Contains(tilePosition))
            outdoorDug.Add(tilePosition);
    }
    public static void SetDug(Vector3Int tilePosition, bool indoor)
    {
        SetDug(new Vector2Int(tilePosition.x, tilePosition.y), indoor);
    }
    public static bool IsDug(Vector2Int tilePosition, bool indoor)
    {
        return indoor ? indoorDug.Contains(tilePosition) : outdoorDug.Contains(tilePosition);
    }
    public static bool IsDug(Vector3Int tilePosition, bool indoor)
    {
        return IsDug(new Vector2Int(tilePosition.x, tilePosition.y), indoor);
    }
    public static string GetUniqueId(Vector2Int position, bool isIndoors)
    {
        return $"{position.x}:{position.y}:{isIndoors}";
    }

    // public static Vector2Int GetChunkFromTile(Vector2Int tilePosition)
    // {
    //     int chunkX = ((tilePosition.x - (chunkWidth / 2)) / chunkWidth) + 1;
    //     int chunkY = ((tilePosition.y - (chunkHeight / 2)) / chunkHeight) + 1;
    //     return new Vector2Int(chunkX, chunkY);
    // }
    // public static Vector2Int GetChunkFromTile(Vector3Int tilePosition)
    // {
    //     return GetChunkFromTile(new Vector2Int(tilePosition.x, tilePosition.y));
    // }
    // public static Vector2Int GetChunkFromPosition(Vector2 position)
    // {
    //     int chunkX = Mathf.FloorToInt(((position.x - (chunkWidth / 2f)) / chunkWidth) + 1);
    //     int chunkY = Mathf.FloorToInt(((position.y - (chunkHeight / 2f)) / chunkHeight) + 1);
    //     return new Vector2Int(chunkX, chunkY);
    // }
    // public static Vector2Int GetChunkFromPosition(Vector3 position)
    // {
    //     return GetChunkFromPosition(new Vector2(position.x, position.y));
    // }
}
