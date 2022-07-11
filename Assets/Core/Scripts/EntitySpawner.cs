using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    public Entity entity;

    void Start()
    {
        MegaPool.Spawn(entity).transform.position = transform.position;
    }
}
