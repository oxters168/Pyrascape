using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Data", menuName = "Background/BiomeInfo", order = 1)]
public class BiomeInfo : ScriptableObject
{
    public Tile ornamentalTile;
    public Tile surfaceTile;
    public Tile undergroundTile;
    public Tile backgroundTile;
}
