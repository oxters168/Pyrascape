using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Background/BackgroundInfo", order = 2)]
public class BackgroundInfo : ScriptableObject
{
    [Tooltip("The background layers")]
    public LoopInfo[] layers;
    [Tooltip("The y value the background parent will be set to")]
    public float height;
}
