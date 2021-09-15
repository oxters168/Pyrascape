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
                if (vehicleDetector != null)
                    nearbyVehicle = vehicleDetector.vehicle.gameObject;
            }

            if (!inVehicle && nearbyVehicle != null)
            {
                spawnedCharacter.gameObject.SetActive(false);
                controlledObject = nearbyVehicle;
                spawnedCamera.target = nearbyVehicle.transform;
            }
            else
            {
                controlledObject = spawnedCharacter.gameObject;
                spawnedCharacter.gameObject.SetActive(true);
                spawnedCamera.target = spawnedCharacter.transform;
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
