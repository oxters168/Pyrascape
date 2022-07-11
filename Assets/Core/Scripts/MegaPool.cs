using System.Collections.Generic;
using UnityEngine;
using UnityHelpers;

public class MegaPool : MonoBehaviour
{
    public static MegaPool _instance;
    private static Dictionary<string, ObjectPool<MonoBehaviour>> maga = new Dictionary<string, ObjectPool<MonoBehaviour>>();

    void Awake()
    {
        _instance = this;
    }

    public static MonoBehaviour Spawn(Entity entity)
    {
        ObjectPool<MonoBehaviour> entityPool;
        if (maga.ContainsKey(entity.prefabName))
            entityPool = maga[entity.prefabName];
        else
        {
            GameObject parent = new GameObject();
            parent.name = entity.prefabName + "_Pool";
            if (_instance != null)
                parent.transform.SetParent(_instance.transform);
            entityPool = new ObjectPool<MonoBehaviour>(entity.prefab, entity.poolSize, entity.reuseObjectsInUse, entity.dynamicSize, parent.transform);
            maga[entity.prefabName] = entityPool;
        }
        return entityPool.Get();
    }
}
