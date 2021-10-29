using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Background/LoopInfo", order = 3)]
public class LoopInfo : ScriptableObject
{
    [Tooltip("The sprite that will be displayed and looped")]
    public Sprite image;
    [Space(10), Tooltip("How many of the sprite image to display at once (will be rounded down to the nearest odd number)")]
    public int loopCount = 3;

    [Tooltip("How far the image should be in the z axis")]
    public float distance;

    [Tooltip("The draw order")]
    public int layerOrder;
    
    [Tooltip("The height of this layer in the background loop")]
    public float relativeHeight;
}