using UnityEngine;
using UnityHelpers;
using System.Collections;
using System.Collections.Generic;

public class Digger : MonoBehaviour
{
    private PodPhysics2D _pod;
    private PodPhysics2D Pod { get { if (_pod == null) _pod = GetComponentInChildren<PodPhysics2D>(); return _pod; } }
    private WorldGenerator[] Terrains { get { if (_terrains == null) { _terrains = FindObjectsOfType<WorldGenerator>(); } return _terrains; } }
    private WorldGenerator[] _terrains;

    private Vector3Int currentCell;
    private Bounds podBounds;
    public bool isDigging { get; private set; }

    public List<OreData> collectedOres = new List<OreData>();

    /// <summary>
    /// How close the target tile needs to be before digging starts
    /// </summary>
    [Tooltip("How close the target tile needs to be before digging starts")]
    public float digReach = 0.05f;
    /// <summary>
    /// How fast to reach target tile in m/s
    /// </summary>
    [Tooltip("How fast to reach target tile in m/s")]
    public float digSpeed = 0.01f;
    public LayerMask digLayer = ~0;
    [Tooltip("The time since the press of left/right when the digger is allowed to dig")]
    public float digTimeWindow = 0.33f;

    private float horizontalDownTime;
    private float prevHorizontal;

    void Start()
    {
        GetComponentInChildren<PodPhysics2D>();
    }
    void Update()
    {
        if (isDigging || (prevHorizontal < float.Epsilon && prevHorizontal > -float.Epsilon && (Pod.horizontal > float.Epsilon || Pod.horizontal < -float.Epsilon)))
            horizontalDownTime = Time.time;
        prevHorizontal = Pod.horizontal;
    }
    void FixedUpdate()
    {
        podBounds = transform.GetTotalBounds(Space.World);

        if (!isDigging)
        {
            currentCell = WorldToCell(podBounds.center);
            bool leftCell = HasTile(currentCell + Vector3Int.left);
            bool rightCell = HasTile(currentCell + Vector3Int.right);
            bool botCell = HasTile(currentCell + Vector3Int.down);

            Ray2D downRay = new Ray2D(podBounds.center.xy() + Vector2.down * podBounds.extents.y, Vector2.down);
            Ray2D leftRay = new Ray2D(podBounds.center.xy() + Vector2.left * podBounds.extents.x, Vector2.left);
            Ray2D rightRay = new Ray2D(podBounds.center.xy() + Vector2.right * podBounds.extents.x, Vector2.right);
            RaycastHit2D downHit = Physics2D.Raycast(downRay.origin, downRay.direction, digReach, digLayer);
            RaycastHit2D leftHit = Physics2D.Raycast(leftRay.origin, leftRay.direction, digReach, digLayer);
            RaycastHit2D rightHit = Physics2D.Raycast(rightRay.origin, rightRay.direction, digReach, digLayer);
            RaycastHit2D groundHit = Physics2D.Raycast(downRay.origin, downRay.direction, Pod.minGroundDistance + digReach, Pod.groundMask);
            Debug.DrawRay(downRay.origin, downRay.direction * (Pod.minGroundDistance + digReach), groundHit ? Color.white : Color.black);
            Debug.DrawRay(downRay.origin, downRay.direction * digReach, downHit ? Color.green : Color.red);
            Debug.DrawRay(leftRay.origin, leftRay.direction * digReach, leftHit ? Color.green : Color.red);
            Debug.DrawRay(rightRay.origin, rightRay.direction * digReach, rightHit ? Color.green : Color.red);

            if (botCell && downHit && (Pod.fly < -float.Epsilon && Pod.horizontal > -float.Epsilon && Pod.horizontal < float.Epsilon))
            {
                StartCoroutine(DigTile(currentCell + Vector3Int.down));
            }
            else if (groundHit && (Time.time - horizontalDownTime) < digTimeWindow)
            {
                if (leftCell && leftHit && Pod.facingLeft && (Pod.horizontal < -float.Epsilon && Pod.fly > -float.Epsilon && Pod.fly < float.Epsilon))
                {
                    StartCoroutine(DigTile(currentCell + Vector3Int.left));
                }
                else if (rightCell && rightHit && !Pod.facingLeft && (Pod.horizontal > float.Epsilon && Pod.fly > -float.Epsilon && Pod.fly < float.Epsilon))
                {
                    StartCoroutine(DigTile(currentCell + Vector3Int.right));
                }
            }
        }
    }

    private IEnumerator DigTile(Vector3Int tilePosition)
    {
        isDigging = true;
        WorldData.SetDug(tilePosition, false);

        Rigidbody2D podBody = GetComponentInChildren<Rigidbody2D>();
        PodPhysics2D pod = GetComponentInChildren<PodPhysics2D>();
        UnifiedInputForPod playerInput = GetComponentInChildren<UnifiedInputForPod>();

        //Take away control from player
        if (playerInput != null)
            playerInput.enabled = false;
        //Turn off pod physics so we can move the pod freely
        if (pod != null)
            pod.enabled = false;
        
        //Let the pod be able to go through objects and not fall
        podBody.bodyType = RigidbodyType2D.Kinematic;
        podBody.velocity = Vector2.zero;
        podBody.angularVelocity = 0;

        //Stop pod input
        pod.horizontal = 0;
        pod.fly = 0;

        Vector2 targetPosition = GetCellCenterWorld(tilePosition).xy();
        Vector2 startPosition = transform.position;
        float startRotation = podBody.rotation;

        float distance = Vector2.Distance(targetPosition, podBounds.center);
        Vector2 direction = (targetPosition - podBounds.center.xy()).normalized;

        //Move towards dug tile within the timeframe based on digspeed
        int totalTimesteps = Mathf.CeilToInt(distance / digSpeed);
        float stepDistance = digSpeed;
        float rotStep = -startRotation / totalTimesteps;
        for (int i = 0; i < totalTimesteps; i++)
        {
            podBody.MovePosition(startPosition + direction * stepDistance * i);
            podBody.MoveRotation(startRotation + rotStep * i);

            yield return new WaitForFixedUpdate();
        }

        //Get ore
        var ore = CheckOre(tilePosition);
        if (ore != null)
            collectedOres.Add(ore);
        
        //Refresh terrain
        // RegenerateTerrain();
        RegenerateTerrainAt(tilePosition);

        //Turn colliders back on and the gravity
        podBody.bodyType = RigidbodyType2D.Dynamic;

        //Turn the pod physics back on
        if (pod != null)
            pod.enabled = true;
        //Give back control to player
        if (playerInput != null)
            playerInput.enabled = true;

        isDigging = false;
    }

    private Vector3Int WorldToCell(Vector3 position)
    {
        if (Terrains != null && Terrains.Length > 0)
            return Terrains[0].WorldToCell(position);
        else
            throw new System.NullReferenceException("No terrain found");
    }
    private bool HasTile(Vector3Int tilePosition)
    {
        if (Terrains != null && Terrains.Length > 0)
            return Terrains[0].HasTile(tilePosition);
        else
            throw new System.NullReferenceException("No terrain found");
    }
    private Vector3 GetCellCenterWorld(Vector3Int tilePosition)
    {
        if (Terrains != null && Terrains.Length > 0)
            return Terrains[0].GetCellCenterWorld(tilePosition);
        else
            throw new System.NullReferenceException("No terrain found");
    }
    private void RegenerateTerrainAt(Vector3Int tilePosition)
    {
        if (Terrains != null)
            foreach (var terrain in Terrains)
                terrain.RefreshArea(tilePosition);
    }
    private OreData CheckOre(Vector3Int tilePosition)
    {
        OreData oreData = null;
        if (Terrains != null && Terrains.Length > 0)
            oreData = Terrains[0].GetOreData(tilePosition);
        return oreData;
    }
}
