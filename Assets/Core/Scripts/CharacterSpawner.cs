using UnityEngine;
using UnityHelpers;
using Rewired;

public class CharacterSpawner : MonoBehaviour
{
    public int playerId;
    private Player player;

    public LayerMask surroundingsMask = ~0;

    [RequireInterface(typeof(IValueManager))]
    public GameObject controlledObject;
    private GameObject _prevControlledObject;
    private IValueManager InputDevice { get { if (controlledObject != _prevControlledObject) { _inputDevice = controlledObject.GetComponent<IValueManager>(); _prevControlledObject = controlledObject; } return _inputDevice; } }
    private IValueManager _inputDevice;

    [Space(10)]
    public OrbitCameraController cameraPrefab;
    public MovementController2D characterPrefab;
    public WorldGenerator terrainPrefab;
    private MovementController2D spawnedCharacter;
    public OrbitCameraController spawnedCamera { get; private set; }
    private WorldGenerator terrain;
    public bool IsIndoors { get { return terrain.isIndoors; } }

    [Space(10)]
    public float exitVelocityUpwards = 0;

    private bool usedDoor;
    
    void Start()
    {
        player = ReInput.players.GetPlayer(playerId);
        Spawn();
    }
    
    void Update()
    {
        EnterExitBuilding();

        EnterExitVehicle();

        BridgeInput();

        terrain.target = controlledObject.transform;
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
        spawnedCharacter.transform.position = transform.position;
        controlledObject = spawnedCharacter.gameObject;

        spawnedCamera = GameObject.Instantiate(cameraPrefab) as OrbitCameraController;
        spawnedCamera.target = spawnedCharacter.transform;

        terrain = GameObject.Instantiate(terrainPrefab) as WorldGenerator;
        terrain.target = controlledObject.transform;
    }

    private void EnterExitBuilding()
    {
        if (player.GetButton("ButtonY"))
        {
            if (!usedDoor)
            {
                var doorDetector = spawnedCharacter.GetComponentInChildren<DoorDetector>();
                if (doorDetector != null && doorDetector.door != null)
                {
                    terrain.isIndoors = !terrain.isIndoors;
                }
                usedDoor = true;
            }
        }
        else
            usedDoor = false;
    }
    private void EnterExitVehicle()
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

            if (!inVehicle && nearbyVehicle != null) //If not in vehicle and there is a vehicle nearby then enter
            {
                spawnedCharacter.gameObject.SetActive(false);
                controlledObject = nearbyVehicle;
                spawnedCamera.target = nearbyVehicle.transform;
            }
            else if (inVehicle) //If in vehicle then exit
            {
                var vehicleBounds = controlledObject.transform.GetTotalBounds(Space.World);
                var characterBounds = spawnedCharacter.transform.GetTotalBounds(Space.World, false, true);

                //Get all possible exit positions
                var sprite7Up = controlledObject.GetComponentInChildren<SpriteRenderer>();
                bool onLeft = false;
                if (sprite7Up != null)
                    onLeft = sprite7Up.flipX;
                Vector2 behindVehiclePosition = vehicleBounds.center + (onLeft ? 1 : -1) * Vector3.right * vehicleBounds.extents.x;
                Vector2 frontVehiclePosition = vehicleBounds.center + (onLeft ? -1 : 1) * Vector3.right * vehicleBounds.extents.x;
                Vector2 aboveVehiclePosition = vehicleBounds.center + Vector3.up * vehicleBounds.extents.y;
                Vector2 belowVehiclePosition = vehicleBounds.center - Vector3.up * vehicleBounds.extents.y;

                //Make sure exit position is not blocked
                RaycastHit2D aboveHit = Physics2D.BoxCast(aboveVehiclePosition + Vector2.up * characterBounds.extents.y, characterBounds.extents, 0, Vector2.zero, 0, surroundingsMask);
                RaycastHit2D bottomHit = Physics2D.BoxCast(belowVehiclePosition - Vector2.up * characterBounds.extents.y, characterBounds.extents, 0, Vector2.zero, 0, surroundingsMask);
                RaycastHit2D behindHit = Physics2D.BoxCast(behindVehiclePosition + (onLeft ? 1 : -1) * Vector2.right * characterBounds.extents.x, characterBounds.extents, 0, Vector2.zero, 0, surroundingsMask);
                RaycastHit2D frontHit = Physics2D.BoxCast(frontVehiclePosition + (onLeft ? -1 : 1) * Vector2.right * characterBounds.extents.x, characterBounds.extents, 0, Vector2.zero, 0, surroundingsMask);
                // Debug.DrawRay(aboveVehiclePosition + Vector2.up * characterBounds.extents.y, Vector3.up, Color.green, 10);
                // Debug.DrawRay(belowVehiclePosition - Vector2.up * characterBounds.extents.y, Vector3.up, Color.green, 10);
                // Debug.DrawRay(behindVehiclePosition + (onLeft ? 1 : -1) * Vector2.right * characterBounds.extents.x, Vector3.up, Color.green, 10);
                // Debug.DrawRay(frontVehiclePosition + (onLeft ? -1 : 1) * Vector2.right * characterBounds.extents.x, Vector3.up, Color.green, 10);

                // Debug.Log((aboveHit == true) + ", " + (behindHit == true) + ", " + (frontHit == true) + ", " + (bottomHit == true));
                Vector2 exitPosition = Vector2.zero;
                if (aboveHit == false)
                    exitPosition = aboveVehiclePosition;
                else if (behindHit == false)
                    exitPosition = behindVehiclePosition + (onLeft ? 1 : -1) * Vector2.right * characterBounds.extents.x;
                else if (frontHit == false)
                    exitPosition = frontVehiclePosition + (onLeft ? -1 : 1) * Vector2.right * characterBounds.extents.x;
                else if (bottomHit == false)
                    exitPosition = belowVehiclePosition - Vector2.up * characterBounds.size.y;

                if (aboveHit == false || bottomHit == false || behindHit == false || frontHit == false) //If no obstacles then exit
                {
                    var vehiclePhysics = controlledObject.GetComponentInChildren<Rigidbody2D>();
                    var characterPhysics = spawnedCharacter.GetComponentInChildren<Rigidbody2D>();

                    // spawnedCharacter.transform.position = aboveVehiclePosition;
                    spawnedCharacter.transform.position = exitPosition;
                    controlledObject = spawnedCharacter.gameObject;
                    spawnedCharacter.gameObject.SetActive(true);
                    spawnedCamera.target = spawnedCharacter.transform;

                    if (characterPhysics != null && vehiclePhysics != null)
                        characterPhysics.velocity = vehiclePhysics.velocity + Vector2.up * exitVelocityUpwards;
                }
                else
                    Debug.Log("Exit blocked");
            }
        }
    }
}
