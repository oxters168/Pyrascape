using UnityEngine;
using UnityHelpers;
using Rewired;

public class CharacterSpawner : MonoBehaviour
{
    public int playerId;
    private Player player;

    [RequireInterface(typeof(IValueManager))]
    public GameObject controlledObject;
    private GameObject _prevControlledObject;
    private IValueManager InputDevice { get { if (controlledObject != _prevControlledObject) { _inputDevice = controlledObject.GetComponent<IValueManager>(); _prevControlledObject = controlledObject; } return _inputDevice; } }
    private IValueManager _inputDevice;

    public OrbitCameraController cameraPrefab;
    public MovementController2D characterPrefab;
    private MovementController2D spawnedCharacter;
    private OrbitCameraController spawnedCamera;

    [Space(10)]
    public float exitVelocityUpwards = 0;
    
    void Start()
    {
        player = ReInput.players.GetPlayer(playerId);
        Spawn();
    }
    
    void Update()
    {
        if (player.GetButtonUp("ButtonY"))
        {
            bool inVehicle = controlledObject != spawnedCharacter.gameObject;
            GameObject nearbyVehicle = null;
            if (!inVehicle)
            {
                var vehicleDetector = spawnedCharacter.GetComponentInChildren<VehicleDetector>();
                if (vehicleDetector != null && vehicleDetector.vehicle != null)
                    nearbyVehicle = vehicleDetector.vehicle.gameObject;
            }

            if (!inVehicle && nearbyVehicle != null)
            {
                spawnedCharacter.gameObject.SetActive(false);
                controlledObject = nearbyVehicle;
                spawnedCamera.target = nearbyVehicle.transform;
            }
            else if (inVehicle)
            {
                // var sprite7Up = controlledObject.GetComponentInChildren<SpriteRenderer>();
                // bool onLeft = false;
                // if (sprite7Up != null)
                //     onLeft = sprite7Up.flipX;

                var vehiclePhysics = controlledObject.GetComponentInChildren<Rigidbody2D>();
                var characterPhysics = spawnedCharacter.GetComponentInChildren<Rigidbody2D>();

                var vehicleBounds = controlledObject.transform.GetTotalBounds(Space.World);
                // spawnedCharacter.transform.position = vehicleBounds.center + (onLeft ? 1 : -1) * Vector3.right * vehicleBounds.extents.x * 1.5f;
                spawnedCharacter.transform.position = vehicleBounds.center + Vector3.up * vehicleBounds.extents.y;
                controlledObject = spawnedCharacter.gameObject;
                spawnedCharacter.gameObject.SetActive(true);
                spawnedCamera.target = spawnedCharacter.transform;

                if (characterPhysics != null && vehiclePhysics != null)
                    characterPhysics.velocity = vehiclePhysics.velocity + Vector2.up * exitVelocityUpwards;
            }
        }

        BridgeInput();
    }

    private void BridgeInput()
    {
        if (InputDevice != null)
        {
            InputDevice.SetAxis("Horizontal", player.GetAxis("Horizontal"));
            InputDevice.SetAxis("Vertical", player.GetAxis("Vertical"));
            InputDevice.SetToggle("ButtonA", player.GetButton("ButtonA"));
        }
    }

    private void Spawn()
    {
        spawnedCharacter = GameObject.Instantiate(characterPrefab) as MovementController2D;
        controlledObject = spawnedCharacter.gameObject;

        spawnedCamera = GameObject.Instantiate(cameraPrefab) as OrbitCameraController;
        spawnedCamera.target = spawnedCharacter.transform;
    }
}
