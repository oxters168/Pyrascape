using UnityEngine;
using UnityHelpers;
using Rewired;

public class HumanControl : MonoBehaviour
{
    public int playerId;
    private Player player;

    public LayerMask surroundingsMask = ~0;

    [RequireInterface(typeof(IValueManager))]
    public GameObject controlledObject;
    // private GameObject prevControlledObject;
    private GameObject _prevControlledObject;
    private IValueManager InputDevice { get { if (controlledObject != _prevControlledObject) { _inputDevice = controlledObject.GetComponent<IValueManager>(); _prevControlledObject = controlledObject; } return _inputDevice; } }
    private IValueManager _inputDevice;

    [Space(10)]
    public LayerMask outdoorViewingLayers = ~0;
    public LayerMask indoorViewingLayers = ~0;
    // [Tooltip("How many tiles should be visible at once")]
    // public Vector2Int renderSize = new Vector2Int(16, 12);
    public RenderForMe WorldRender { get { if (_worldRender == null) _worldRender = GetComponentInChildren<RenderForMe>(); return _worldRender; } }
    private RenderForMe _worldRender;
    
    [Space(10)]
    // public OrbitCameraController cameraPrefab;
    // public MovementController2D characterPrefab;
    public Entity orbitCamera;
    public Entity spectator;
    // public Spectator spectatorPrefab;
    // public WorldGenerator terrainPrefab;
    public MovementController2D Movement { get { if (_movement == null) _movement = GetComponentInChildren<MovementController2D>(); return _movement; } }
    private MovementController2D _movement;
    private Spectator spawnedSpectator;
    // private MovementController2D spawnedCharacter;
    public OrbitCameraController spawnedCamera { get; private set; }
    // private WorldGenerator terrain;
    // private BackgroundLoop backgroundLoop;
    public bool isIndoors;
    public bool isSpecIndoors;
    public bool isSpectating;

    [Space(10)]
    public float exitVelocityUpwards = 0;

    private bool usedDoor;
    private bool usedVehicle;
    private bool usedSpectate;
    private GameObject preSpecControlledObj;
    private Vector2Int vehiclePrevRenderSize;
    //private bool preSpecIndoors;
    
    void Start()
    {
        player = ReInput.players.GetPlayer(playerId);
        Spawn();
        // prevControlledObject = controlledObject;
    }
    void Update()
    {
        EnterExitBuilding();

        EnterExitVehicle();

        EnterExitSpectate();

        BridgeInput();

        SetCameraColor();

        //Switch camera viewing layers and character layer
        spawnedCamera.GetComponent<Camera>().cullingMask = GetIsIndoors() ? indoorViewingLayers : outdoorViewingLayers;
        Movement.isIndoors = isIndoors;
    }
    void OnDestroy()
    {
        if (CharacterRegistry.HasCharacter(this))
            CharacterRegistry.RemoveCharacter(this);
    }

    public bool GetIsIndoors()
    {
        return (!isSpectating && isIndoors) || (isSpectating && isSpecIndoors);
    }
    private void SetCameraColor()
    {
        var currentBuilding = WorldGenerator.GetCurrentBuilding(WorldGenerator._instance.buildings, Vector3Int.RoundToInt(controlledObject.transform.position));
        Color buildingBg = Color.black;
        if (currentBuilding != null)
            buildingBg = currentBuilding.backgroundColor;
        Color backgroundColor = GetIsIndoors() ? buildingBg : WorldGenerator._instance.currentBiome.backgroundColor;
        spawnedCamera.GetComponentInChildren<Camera>().backgroundColor = backgroundColor;
    }

    private void BridgeInput()
    {
        if (InputDevice != null)
        {
            // string controllersDebug = "";
            // foreach (var controller in ReInput.controllers.Controllers)
            //     controllersDebug += controller.hardwareName + ", ";
            // DebugPanel.Log("Controllers: ", controllersDebug, 5);
            InputDevice.SetAxis("Horizontal", player.GetAxis("Horizontal"));
            InputDevice.SetAxis("Vertical", player.GetAxis("Vertical"));
            InputDevice.SetToggle("ButtonA", player.GetButton("ButtonA"));
            InputDevice.SetToggle("ButtonX", player.GetButton("ButtonX"));
        }
    }

    private void Spawn()
    {
        CharacterRegistry.AddCharacter(this);

        // spawnedCharacter = GameObject.Instantiate(characterPrefab) as MovementController2D;
        // Movement.transform.position = transform.position;
        // Movement.transform.SetParent(transform);
        controlledObject = Movement.gameObject;

        // spawnedCamera = GameObject.Instantiate(cameraPrefab) as OrbitCameraController;
        spawnedCamera = MegaPool.Spawn(orbitCamera) as OrbitCameraController;
        spawnedCamera.target = Movement.transform;
        spawnedCamera.transform.SetParent(transform);

        // spawnedSpectator = GameObject.Instantiate(spectatorPrefab) as Spectator;
        spawnedSpectator = MegaPool.Spawn(spectator) as Spectator;
        spawnedSpectator.transform.position = transform.position;
        spawnedSpectator.transform.SetParent(transform);
        spawnedSpectator.gameObject.SetActive(false);

        StartCoroutine(RenderFromCamera()); //This is so the camera has time to move to its correct position first

        // WorldRender.renderTerrain = true;
        // WorldRender.renderBackground = true;
    }
    private System.Collections.IEnumerator RenderFromCamera()
    {
        yield return new WaitForEndOfFrame();
        WorldRender.RenderingCamera = spawnedCamera.GetComponent<Camera>();
        spawnedSpectator.WorldRender.RenderingCamera = spawnedCamera.GetComponent<Camera>();
    }

    private void EnterExitSpectate()
    {
        if (player.GetButton("ButtonL3"))
        {
            if (!usedSpectate)
            {
                usedSpectate = true;
                isSpectating = !isSpectating;

                if (isSpectating)
                {
                    spawnedSpectator.gameObject.SetActive(true);
                    spawnedSpectator.transform.position = controlledObject.transform.position;
                    preSpecControlledObj = controlledObject;
                    isSpecIndoors = isIndoors;
                    controlledObject = spawnedSpectator.gameObject;
                }
                else
                {
                    spawnedSpectator.gameObject.SetActive(false);
                    controlledObject = preSpecControlledObj;
                    controlledObject.transform.position = spawnedSpectator.transform.position;
                    //isIndoors = preSpecIndoors;
                    //spawnedSpectator.transform.localPosition = Vector3.zero;
                }

                spawnedCamera.target = controlledObject.transform;
            }
        }
        else
            usedSpectate = false;
    }
    private void EnterExitBuilding()
    {
        if (player.GetButton("ButtonY"))
        {
            if (!usedDoor && !usedVehicle)
            {
                if (!isSpectating)
                {
                    var doorDetector = Movement.GetComponentInChildren<DoorDetector>();
                    if (doorDetector != null && doorDetector.door != null && doorDetector.door.GetComponentInParent<Door>().isOpen)
                    {
                        usedDoor = true;
                        isIndoors = !isIndoors;
                    }
                }
                else
                {
                    usedDoor = true;
                    isSpecIndoors = !isSpecIndoors;
                }

                WorldRender.SetRenderBackground(!GetIsIndoors());
                // WorldRender.renderBackground = GetIsIndoors();
            }
        }
        else
            usedDoor = false;
    }
    private void EnterExitVehicle()
    {
        if (player.GetButton("ButtonY"))
        {
            if (!isSpectating && !usedVehicle && !usedDoor)
            {
                // usedVehicle = true;
                bool inVehicle = controlledObject != Movement.gameObject;
                GameObject nearbyVehicle = null;
                if (!inVehicle) //If not currently in vehicle then check what vehicle is nearby
                {
                    var vehicleDetector = Movement.GetComponentInChildren<VehicleDetector>();
                    if (vehicleDetector != null && vehicleDetector.vehicle != null)
                        nearbyVehicle = vehicleDetector.vehicle.gameObject;
                }
                
                    
                if (!isIndoors && !inVehicle && nearbyVehicle != null) //If not in vehicle and there is a vehicle nearby then enter
                {
                    bool enteredVehicle = nearbyVehicle?.GetComponentInChildren<Digger>()?.Enter(this) ?? false;
                    if (enteredVehicle) //If the vehicle is not a digger or it is not occupied, if I add other vehicles later I should unify them
                    {
                        usedVehicle = true;

                        // var vehicleSurroundingsRender = nearbyVehicle.GetComponent<RenderForMe>();
                        // vehiclePrevRenderSize = vehicleSurroundingsRender.renderSize;
                        // vehicleSurroundingsRender.renderSize = WorldRender.renderSize;
                        // vehicleSurroundingsRender.renderBackground = true;
                        
                        Movement.gameObject.SetActive(false);
                        // WorldRender.renderTerrain = false;
                        // WorldRender.renderBackground = false;
                        controlledObject = nearbyVehicle;
                        spawnedCamera.target = nearbyVehicle.transform;
                    }
                }
                else if (inVehicle) //If in vehicle then exit
                {
                    usedVehicle = true;
                    var vehicleBounds = controlledObject.transform.GetTotalBounds(Space.World);
                    var characterBounds = Movement.transform.GetTotalBounds(Space.World, false, true);

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
                        var characterPhysics = Movement.GetComponentInChildren<Rigidbody2D>();

                        var diggerVehicle = controlledObject.GetComponentInChildren<Digger>();
                        if (diggerVehicle)
                            diggerVehicle.Exit();

                        // spawnedCharacter.transform.position = aboveVehiclePosition;
                        // WorldRender.renderTerrain = true;
                        // WorldRender.renderBackground = true;
                        // var vehicleSurroundingsRender = controlledObject.GetComponent<RenderForMe>();
                        // vehicleSurroundingsRender.renderSize = vehiclePrevRenderSize;
                        // vehicleSurroundingsRender.renderBackground = false;

                        Movement.transform.position = exitPosition;
                        controlledObject = Movement.gameObject;
                        // terrain.AddOrSetTarget(spawnedCharacter.transform, renderSize); //Track character again in terrain generation
                        // backgroundLoop.AddTarget(spawnedCharacter.transform);
                        Movement.gameObject.SetActive(true);
                        spawnedCamera.target = Movement.transform;

                        if (characterPhysics != null && vehiclePhysics != null)
                            characterPhysics.velocity = vehiclePhysics.velocity + Vector2.up * exitVelocityUpwards;
                    }
                    else
                        Debug.Log("Exit blocked"); //Seems to be affected by indoor/outdoor regardless of indoor state
                }
            }
        }
        else
            usedVehicle = false;
    }
}
