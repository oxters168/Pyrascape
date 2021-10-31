using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    public GameObject entityPrefab;
    public GameObject spawnedEntity { get; private set; }

    void Start()
    {
        Spawn();
    }

    private void Spawn()
    {
        spawnedEntity = GameObject.Instantiate(entityPrefab);
        spawnedEntity.transform.position = transform.position;
        // controlledObject = spawnedCharacter.gameObject;
    }
}
