using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    public GameObject entityPrefab;
    public GameObject spawnedEntity { get; private set; }
    private WorldGenerator terrain;
    private BackgroundLoop backgroundLoop;

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
                terrain.AddOrSetTarget(spawnedEntity.transform, renderSize); //Revert digger terrain render size

            if (!entityDigger.isOccupied && prevDiggerIsOccupied)
                backgroundLoop.RemoveTarget(spawnedEntity.transform); //Remove digger from background loop targets
            if (entityDigger.isOccupied && !prevDiggerIsOccupied)
                backgroundLoop.AddTarget(spawnedEntity.transform); //Add digger to background loop targets
                
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
        backgroundLoop = FindObjectOfType<BackgroundLoop>();
        terrain.AddOrSetTarget(spawnedEntity.transform, renderSize);
    }
}
