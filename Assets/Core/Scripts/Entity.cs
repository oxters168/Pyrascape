using UnityEngine;

[CreateAssetMenu(fileName = "Entity", menuName = "Pyrascape/Entity", order = 8)]
public class Entity : ScriptableObject
{
    [Tooltip("This name works as a way of identifying each entity, in some cases as a key")]
    public string prefabName;
    [Tooltip("The prefab that will be duplicated")]
    public MonoBehaviour prefab;
    [Tooltip("How large the object pool will be when first created")]
    public int poolSize = 5;
    [Tooltip("Whether the object pool should cycle through enabled and disabled instances")]
    public bool reuseObjectsInUse = false;
    [Tooltip("Should the object pool increase in size when requesting to spawn an entity and no disabled instances exist in the pool (only applicable if reuseObjectsInUse is set to false)")]
    public bool dynamicSize = true;
}