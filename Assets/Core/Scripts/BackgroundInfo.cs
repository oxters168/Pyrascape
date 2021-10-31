using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Pyrascape/BackgroundInfo", order = 3)]
public class BackgroundInfo : ScriptableObject
{
    [Tooltip("The background layers")]
    public LoopInfo[] layers;
    [Tooltip("The y value the background parent will be set to")]
    public float height;
}
