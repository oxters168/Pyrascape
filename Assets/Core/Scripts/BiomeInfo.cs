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
}
