using UnityEngine;
using UnityHelpers;

public class Spectator : MonoBehaviour, IValueManager
{
    // [Tooltip("How many tiles should be visible at once")]
    // public Vector2Int renderSize = new Vector2Int(16, 12);

    // public bool isIndoors;

    // private Transform specTarget;
    // public OrbitCameraController cameraPrefab;
    // public OrbitCameraController spawnedCamera { get; private set; }

    // private WorldGenerator terrain;
    // private BackgroundLoop backgroundLoop;
    [Tooltip("In m/s")]
    public float speed = 10f;
    public ValuesVault controlValues;

    void Start()
    {
        // Spawn();
    }
    void Update()
    {
        // SetCameraColor();
        var horizontal = Mathf.Clamp(GetAxis("Horizontal"), -1, 1);
        var vertical = Mathf.Clamp(GetAxis("Vertical"), -1, 1);
        var jump = GetToggle("ButtonA");
        var sprint = GetToggle("ButtonX");

        transform.position = transform.position + ((Vector3)(new Vector2(horizontal, vertical)).ToCircle() * speed * Time.deltaTime);
    }
    void OnDestroy()
    {
        // Despawn();
    }

    public void SetAxis(string name, float value)
    {
        controlValues.GetValue(name).SetAxis(value);
    }
    public float GetAxis(string name)
    {
        return controlValues.GetValue(name).GetAxis();
    }
    public void SetToggle(string name, bool value)
    {
        controlValues.GetValue(name).SetToggle(value);
    }
    public bool GetToggle(string name)
    {
        return controlValues.GetValue(name).GetToggle();
    }
    public void SetDirection(string name, Vector3 value)
    {
        controlValues.GetValue(name).SetDirection(value);
    }
    public Vector3 GetDirection(string name)
    {
        return controlValues.GetValue(name).GetDirection();
    }
    public void SetPoint(string name, Vector3 value)
    {
        controlValues.GetValue(name).SetPoint(value);
    }
    public Vector3 GetPoint(string name)
    {
        return controlValues.GetValue(name).GetPoint();
    }
    public void SetOrientation(string name, Quaternion value)
    {
        controlValues.GetValue(name).SetOrientation(value);
    }
    public Quaternion GetOrientation(string name)
    {
        return controlValues.GetValue(name).GetOrientation();
    }
    // private void SetCameraColor()
    // {
    //     Color backgroundColor = isIndoors ? terrain.currentBuilding.backgroundColor : terrain.currentBiome.backgroundColor;
    //     spawnedCamera.GetComponentInChildren<Camera>().backgroundColor = backgroundColor;
    // }

    // private void Despawn()
    // {
    //     //terrain = FindObjectOfType<WorldGenerator>();
    //     terrain.RemoveTarget(specTarget);
    //     //backgroundLoop = FindObjectOfType<BackgroundLoop>();
    //     backgroundLoop.RemoveTarget(specTarget);
    // }

    // private void Spawn()
    // {
    //     specTarget = new GameObject("Target").transform;
    //     specTarget.SetParent(transform);

    //     spawnedCamera = GameObject.Instantiate(cameraPrefab) as OrbitCameraController;
    //     spawnedCamera.target = specTarget;
    //     spawnedCamera.transform.SetParent(transform);

    //     terrain = FindObjectOfType<WorldGenerator>();
    //     terrain.AddOrSetTarget(specTarget, renderSize);
    //     backgroundLoop = FindObjectOfType<BackgroundLoop>();
    //     backgroundLoop.AddTarget(specTarget);
    //     // terrain = GameObject.Instantiate(terrainPrefab) as WorldGenerator;
    //     // terrain.target = controlledObject.transform;
    // }
}
