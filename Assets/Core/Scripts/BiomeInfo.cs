using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Data", menuName = "Pyrascape/BiomeInfo", order = 1)]
public class BiomeInfo : ScriptableObject
{
    public NoiseInfo noise;
    public Color backgroundColor;
    public BackgroundInfo background;
    public TileBase ornamentalTile;
    public TileBase[] surfaceTile;
    public TileBase[] undergroundTile;
    public TileBase backgroundTile;

    public TileBase GetOrnamentTileAt(Vector3Int currentTilePos, int surfaceHeight)
    {
        TileBase ornTile = null;
        if (currentTilePos.y == (surfaceHeight + 1) && !WorldData.IsDug(currentTilePos + Vector3Int.down, false))
            ornTile = ornamentalTile;
        return ornTile;
    }
    public TileBase GetBackgroundTileAt(Vector3Int currentTilePos, int surfaceHeight)
    {
        //Later will add more complexity using currentTilePos
        TileBase bgTile = null;
        if (currentTilePos.y <= surfaceHeight)
        {
            bgTile = backgroundTile;
        }
        return bgTile;
    }
    public TileBase GetPhysicalTileAt(Vector3Int currentTilePos, int surfaceHeight)
    {
        TileBase physicalTile = null;

        if (currentTilePos.y <= surfaceHeight)
        {
            Vector3Int leftTilePos = currentTilePos + Vector3Int.left;
            Vector3Int upperTilePos = currentTilePos + Vector3Int.up;
            Vector3Int rightTilePos = currentTilePos + Vector3Int.right;
            Vector3Int lowerTilePos = currentTilePos + Vector3Int.down;
            bool hasLeftTile = !WorldData.IsDug(leftTilePos, false);
            bool hasTopTile = currentTilePos.y < surfaceHeight && !WorldData.IsDug(upperTilePos, false);
            bool hasRightTile = !WorldData.IsDug(rightTilePos, false);
            bool hasBotTile = !WorldData.IsDug(lowerTilePos, false) && WorldGenerator.NoiseCheck(lowerTilePos, noise);
            if (currentTilePos.y < surfaceHeight)
            {
                hasLeftTile &= WorldGenerator.NoiseCheck(leftTilePos, noise);
                hasRightTile &= WorldGenerator.NoiseCheck(rightTilePos, noise);
            }
            if (currentTilePos.y < surfaceHeight - 1)
                hasTopTile &= WorldGenerator.NoiseCheck(upperTilePos, noise);
            int borderIndex = (hasLeftTile ? 1 : 0) + (hasTopTile ? 2 : 0) + (hasRightTile ? 4 : 0) + (hasBotTile ? 8 : 0);

            if (currentTilePos.y == surfaceHeight && !WorldData.IsDug(currentTilePos, false))
                physicalTile = surfaceTile[borderIndex];

            if (currentTilePos.y < surfaceHeight && WorldGenerator.NoiseCheck(currentTilePos, noise) && !WorldData.IsDug(currentTilePos, false))
                physicalTile = undergroundTile[borderIndex];
        }
        return physicalTile;
    }
}
