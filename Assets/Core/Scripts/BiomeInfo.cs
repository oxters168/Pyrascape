using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Data", menuName = "Pyrascape/BiomeInfo", order = 1)]
public class BiomeInfo : ScriptableObject
{
    [Tooltip("Bottom left corner")]
    public Vector2Int corner;
    [Tooltip("How far the biome reaches horizontally and vertically from the corner")]
    public Vector2Int size;
    [Tooltip("How the biome's tiles should be placed")]
    public NoiseInfo noise;
    [Tooltip("When the player is in this biome, what should the camera's solid color be set to")]
    public Color backgroundColor;
    [Tooltip("When the player is in this biome, what looped background should be shown")]
    public BackgroundInfo background;
    [Tooltip("The tile that is placed directly on top of the surface")]
    public TileBase ornamentalTile;
    [Tooltip("The tiles that make up the surface, they must be placed in a specific order")]
    public TileBase[] surfaceTile;
    [Tooltip("The tiles that make up the area under the surface, they must be placed in a specific order")]
    public TileBase[] undergroundTile;
    [Tooltip("The tile that will be repeated as a background")]
    public TileBase backgroundTile;

    public int TopPos { get { return corner.y + size.y; } }
    public int BotPos { get { return corner.y; } }
    public int LeftPos { get { return corner.x; } }
    public int RightPos { get { return corner.x + size.x; } }

    public TileBase GetOrnamentTileAt(Vector3Int currentTilePos)
    {
        TileBase ornTile = null;
        if (currentTilePos.y == (TopPos + 1) && currentTilePos.x >= LeftPos && currentTilePos.x <= RightPos && !WorldData.IsDug(currentTilePos + Vector3Int.down, false))
            ornTile = ornamentalTile;
        return ornTile;
    }
    public TileBase GetBackgroundTileAt(Vector3Int currentTilePos)
    {
        //Later will add more complexity using currentTilePos
        TileBase bgTile = null;
        if (currentTilePos.y >= BotPos && currentTilePos.y <= TopPos && currentTilePos.x >= LeftPos && currentTilePos.x <= RightPos)
        {
            bgTile = backgroundTile;
        }
        return bgTile;
    }
    public TileBase GetPhysicalTileAt(Vector3Int currentTilePos)
    {
        TileBase physicalTile = null;

        if (currentTilePos.y >= BotPos && currentTilePos.y <= TopPos && currentTilePos.x >= LeftPos && currentTilePos.x <= RightPos)
        {
            Vector3Int leftTilePos = currentTilePos + Vector3Int.left;
            Vector3Int upperTilePos = currentTilePos + Vector3Int.up;
            Vector3Int rightTilePos = currentTilePos + Vector3Int.right;
            Vector3Int lowerTilePos = currentTilePos + Vector3Int.down;
            bool hasLeftTile = currentTilePos.x > LeftPos &&!WorldData.IsDug(leftTilePos, false);
            bool hasTopTile = currentTilePos.y < TopPos && !WorldData.IsDug(upperTilePos, false);
            bool hasRightTile = currentTilePos.x < RightPos && !WorldData.IsDug(rightTilePos, false);
            bool hasBotTile = currentTilePos.y > BotPos && !WorldData.IsDug(lowerTilePos, false) && WorldGenerator.NoiseCheck(lowerTilePos, noise);
            if (currentTilePos.y < TopPos)
            {
                hasLeftTile &= WorldGenerator.NoiseCheck(leftTilePos, noise);
                hasRightTile &= WorldGenerator.NoiseCheck(rightTilePos, noise);
            }
            if (currentTilePos.y < TopPos - 1)
                hasTopTile &= WorldGenerator.NoiseCheck(upperTilePos, noise);
            int borderIndex = (hasLeftTile ? 1 : 0) + (hasTopTile ? 2 : 0) + (hasRightTile ? 4 : 0) + (hasBotTile ? 8 : 0);

            if (currentTilePos.y == TopPos && !WorldData.IsDug(currentTilePos, false))
                physicalTile = surfaceTile[borderIndex];

            if (currentTilePos.y < TopPos && WorldGenerator.NoiseCheck(currentTilePos, noise) && !WorldData.IsDug(currentTilePos, false))
                physicalTile = undergroundTile[borderIndex];
        }
        return physicalTile;
    }
}
