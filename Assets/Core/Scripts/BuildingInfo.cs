using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Data", menuName = "Pyrascape/BuildingInfo", order = 2)]
public class BuildingInfo : ScriptableObject
{
    [Tooltip("The bottom left corner of the building")]
    public Vector3Int corner;
    [Tooltip("The number of tiles along the horizontal and vertical axes of the building")]
    public Vector2Int size;
    [Tooltip("How the building's tiles should be placed")]
    public NoiseInfo noise;
    [Tooltip("When the player is indoors, what should the camera's solid color be set to")]
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

        Vector3Int leftTilePos = currentTilePos + Vector3Int.left;
        Vector3Int upperTilePos = currentTilePos + Vector3Int.up;
        Vector3Int rightTilePos = currentTilePos + Vector3Int.right;
        Vector3Int lowerTilePos = currentTilePos + Vector3Int.down;

        bool hasLeftTile = !WorldData.IsDug(leftTilePos, true) && WorldGenerator.NoiseCheck(leftTilePos, noise);
        bool hasTopTile = !WorldData.IsDug(upperTilePos, true) && WorldGenerator.NoiseCheck(upperTilePos, noise);
        bool hasRightTile = !WorldData.IsDug(rightTilePos, true) && WorldGenerator.NoiseCheck(rightTilePos, noise);
        bool hasBotTile = !WorldData.IsDug(lowerTilePos, true) && WorldGenerator.NoiseCheck(lowerTilePos, noise);

        float buildingTilePerlin = WorldGenerator.GetNoiseValueOf(currentTilePos, noise);

        //Set walls of building
        if (currentTilePos.x == corner.x && currentTilePos.y == corner.y) //Bottom left corner
            physicalTile = wallTile[6];
        else if (currentTilePos.x == corner.x && currentTilePos.y == (corner.y + size.y - 1)) //Top left corner
            physicalTile = wallTile[12];
        else if (currentTilePos.x == (corner.x + size.x - 1) && currentTilePos.y == (corner.y + size.y - 1)) //Top right corner
            physicalTile = wallTile[9];
        else if (currentTilePos.x == (corner.x + size.x - 1) && currentTilePos.y == corner.y) //Bottom right corner
            physicalTile = wallTile[3];
        else if (currentTilePos.y == corner.y) //Bottom wall
            physicalTile = wallTile[5]; //Don't add any border since first tile is always empty
        else if (currentTilePos.y == (corner.y + size.y - 1)) //Top wall
            physicalTile = wallTile[5 + (hasBotTile ? 8 : 0)];
        else if (currentTilePos.x == corner.x) //Left wall
            physicalTile = wallTile[10 + (hasRightTile ? 4 : 0)];
        else if (currentTilePos.x == (corner.x + size.x - 1)) //Right wall
            physicalTile = wallTile[10 + (hasLeftTile ? 1 : 0)];
        else if (currentTilePos.y > corner.y + 1 && WorldGenerator.NoiseCheck(buildingTilePerlin, noise) && !WorldData.IsDug(currentTilePos, true)) //Inner walls
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
}
