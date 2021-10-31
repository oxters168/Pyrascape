using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Data", menuName = "Pyrascape/BuildingInfo", order = 2)]
public class BuildingInfo : ScriptableObject
{
    public Color backgroundColor;
    public TileBase[] wallTile;
    public TileBase backgroundTile;
}
