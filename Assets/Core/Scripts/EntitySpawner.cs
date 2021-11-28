using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    public GameObject entityPrefab;
    public GameObject spawnedEntity { get; private set; }
    private WorldGenerator terrain;

    [Tooltip("How many tiles should be visible at once")]
    public Vector2Int renderSize = new Vector2Int(4, 4);

    private PodPhysics2D entityDigger;
    private bool prevDiggerIsOccupied;

    void Start()
    {
        Spawn();
    }
    void Update()
    {
        if (entityDigger)
        {
            if (!entityDigger.isOccupied)
                terrain.AddOrSetTarget(spawnedEntity.transform, renderSize);
            prevDiggerIsOccupied = entityDigger.isOccupied;
        }
    }

    private void Spawn()
    {
        spawnedEntity = GameObject.Instantiate(entityPrefab);
        spawnedEntity.transform.position = transform.position;
        entityDigger = spawnedEntity.GetComponentInChildren<PodPhysics2D>();
        // controlledObject = spawnedCharacter.gameObject;
        terrain = FindObjectOfType<WorldGenerator>();
        terrain.AddOrSetTarget(spawnedEntity.transform, renderSize);
    }
}
