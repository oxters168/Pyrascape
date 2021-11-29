using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Pyrascape/BackgroundInfo", order = 3)]
public class BackgroundInfo : ScriptableObject
{
    public static readonly float[] DISTANCE = { 4, 8, 16, 32 };
    [Tooltip("The background layers, must not exceed 4")]
    public LoopInfo[] layers;

    [Tooltip("The y value the background parent will be set to")]
    public float height;
}
