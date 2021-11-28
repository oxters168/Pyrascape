using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    public GameObject entityPrefab;
    public GameObject spawnedEntity { get; private set; }
    private WorldGenerator terrain;

    [Tooltip("How many tiles should be visible at once")]
    public Vector2Int renderSize = new Vector2Int(4, 4);

    void Start()
    {
        Spawn();
    }

    private void Spawn()
    {
        spawnedEntity = GameObject.Instantiate(entityPrefab);
        spawnedEntity.transform.position = transform.position;
        // controlledObject = spawnedCharacter.gameObject;
        terrain = FindObjectOfType<WorldGenerator>();
        terrain.AddTarget(spawnedEntity.transform, renderSize);
    }
}
