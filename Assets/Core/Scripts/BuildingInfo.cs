using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Data", menuName = "Pyrascape/BuildingInfo", order = 2)]
public class BuildingInfo : ScriptableObject
{
    [Tooltip("The bottom left corner of the building")]
    public Vector2Int corner;
    [Tooltip("The number of tiles along the horizontal and vertical axes of the building")]
    public Vector2Int size;
    [Tooltip("The building's door relative to the corner position")]
    public Vector2Int doorPos;
    public Entity door;
    public Entity trapDoor;
    [Tooltip("How the building's tiles should be placed")]
    public NoiseInfo noise;
    public NoiseInfo entityNoise;
    [Tooltip("When the player is in this building, what should the camera's solid color be set to")]
    public Color backgroundColor;
    [Tooltip("The tiles that make up the walls of the building, they must be placed in a specific order")]
    public TileBase[] wallTile;
    [Tooltip("The tile that will be repeated as a background")]
    public TileBase backgroundTile;

    public TileBase GetBackgroundTileAt(Vector3Int currentTilePos)
    {
        //Will use currentTilePos later for more complicated bgs
        return backgroundTile;
    }
    public TileBase GetPhysicalTileAt(Vector3Int currentTilePos)
    {
        TileBase physicalTile = null;

        bool hasLeftTile = HasTileAt(currentTilePos, Vector3Int.left);
        bool hasTopTile = HasTileAt(currentTilePos, Vector3Int.up);
        bool hasRightTile = HasTileAt(currentTilePos, Vector3Int.right);
        bool hasBotTile = HasTileAt(currentTilePos, Vector3Int.down);

        // float buildingTilePerlin = WorldGenerator.GetNoiseValueOf(currentTilePos, noise);

        //Set walls of building
        if (IsBottomLeftCorner(currentTilePos))
            physicalTile = wallTile[6];
        else if (IsTopLeftCorner(currentTilePos)) //Top left corner
            physicalTile = wallTile[12];
        else if (IsTopRightCorner(currentTilePos)) //Top right corner
            physicalTile = wallTile[9];
        else if (IsBottomRightCorner(currentTilePos)) //Bottom right corner
            physicalTile = wallTile[3];
        else if (IsBottomWall(currentTilePos)) //Bottom wall
            physicalTile = wallTile[5]; //Don't add any border since first tile is always empty
        else if (IsTopWall(currentTilePos)) //Top wall
            physicalTile = wallTile[5 + (hasBotTile ? 8 : 0)];
        else if (IsLeftWall(currentTilePos)) //Left wall
            physicalTile = wallTile[10 + (hasRightTile ? 4 : 0)];
        else if (IsRightWall(currentTilePos)) //Right wall
            physicalTile = wallTile[10 + (hasLeftTile ? 1 : 0)];
        else if (currentTilePos.y > corner.y + 1 && HasTileAt(currentTilePos)) //Inner walls
        {
            if (currentTilePos.x == corner.x + 1) //Near left edge, so make sure has left border
                hasLeftTile = true;
            if (currentTilePos.x == corner.x + size.x - 2) //Near right edge, so make sure has right border
                hasRightTile = true;
            if (currentTilePos.y == corner.y + 2) //Near bottom edge, so make sure does not have bottom border since first tile is always empty
                hasBotTile = false;
            if (currentTilePos.y == corner.y + size.y - 2) //Near top edge, so make sure has top border
                hasTopTile = true;

            // if (!hasBotTile && !hasTopTile && buildingTilePerlin >= currentBuilding.noise.threshold * 1.1f)
            // {
            //     // WorldData.SetDug(currentTilePos, true);
            //     trapDoors.Add(trapDoorPool.Get(trapDoor => { trapDoor.transform.position = outdoorPhysical.CellToWorld(currentTilePos); }));
            // }
            // else
            // {
            int borderIndex = (hasLeftTile ? 1 : 0) + (hasTopTile ? 2 : 0) + (hasRightTile ? 4 : 0) + (hasBotTile ? 8 : 0);
            physicalTile = wallTile[borderIndex];
            // }
        }

        return physicalTile;
    }
    public Entity GetEntityAt(Vector3Int currentTilePos)
    {
        bool hasEntity = WorldGenerator.NoiseCheck(currentTilePos, entityNoise);
        if (currentTilePos == (Vector3Int)(corner + doorPos))
            return door;
        else if (hasEntity && !HasTileAt(currentTilePos) && !HasTileAt(currentTilePos, Vector3Int.down) && !HasTileAt(currentTilePos, Vector3Int.up))
            return trapDoor;
        return null;
    }
    public bool HasTileAt(Vector3Int currentTilePos, Vector3Int offset)
    {
        return HasTileAt(currentTilePos + offset);
    }
    public bool HasTileAt(Vector3Int currentTilePos)
    {
        return IsInnerWall(currentTilePos) || IsOuterWall(currentTilePos);
    }
    public bool IsInnerWall(Vector3Int currentTilePos)
    {
        return !WorldData.IsDug(currentTilePos, true) && WorldGenerator.NoiseCheck(currentTilePos, noise);
    }
    
    public bool IsBottomLeftCorner(Vector3Int currentTilePos)
    {
        return currentTilePos.x == corner.x && currentTilePos.y == corner.y;
    }
    public bool IsTopLeftCorner(Vector3Int currentTilePos)
    {
        return currentTilePos.x == corner.x && currentTilePos.y == (corner.y + size.y - 1);
    }
    public bool IsTopRightCorner(Vector3Int currentTilePos)
    {
        return currentTilePos.x == (corner.x + size.x - 1) && currentTilePos.y == (corner.y + size.y - 1);
    }
    public bool IsBottomRightCorner(Vector3Int currentTilePos)
    {
        return currentTilePos.x == (corner.x + size.x - 1) && currentTilePos.y == corner.y;
    }
    public bool IsBottomWall(Vector3Int currentTilePos)
    {
        return currentTilePos.y == corner.y;
    }
    public bool IsTopWall(Vector3Int currentTilePos)
    {
        return currentTilePos.y == (corner.y + size.y - 1);
    }
    public bool IsLeftWall(Vector3Int currentTilePos)
    {
        return currentTilePos.x == corner.x;
    }
    public bool IsRightWall(Vector3Int currentTilePos)
    {
        return currentTilePos.x == (corner.x + size.x - 1);
    }
    public bool IsOuterWall(Vector3Int currentTilePos)
    {
        return IsBottomWall(currentTilePos) || IsTopWall(currentTilePos) || IsLeftWall(currentTilePos) || IsRightWall(currentTilePos) || IsBottomLeftCorner(currentTilePos) || IsBottomRightCorner(currentTilePos) || IsTopLeftCorner(currentTilePos) || IsBottomRightCorner(currentTilePos);
    }
}
