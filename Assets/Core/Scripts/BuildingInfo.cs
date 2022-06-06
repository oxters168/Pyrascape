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
}
