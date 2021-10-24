using UnityEngine.Tilemaps;
using UnityEngine;
 
 [CreateAssetMenu(fileName = "NoiseTile", menuName = "Background/NoiseTile", order = 1)]
public class NoiseTile : Tile
{
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
        // tileData.color = Color.blue;
    }
 }