using UnityEngine;
using UnityEngine.Tilemaps;
using UnityHelpers;
using System.Collections;

public class Digger : MonoBehaviour
{
    private PodPhysics2D _pod;
    private PodPhysics2D Pod { get { if (_pod == null) _pod = GetComponentInChildren<PodPhysics2D>(); return _pod; } }
    private Tilemap physical, foreground;

    private Vector3Int currentCell;
    private Bounds podBounds;
    public bool isDigging { get; private set; }

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

    void Start()
    {
        FindTilemaps();
        GetComponentInChildren<PodPhysics2D>();
    }

    void FixedUpdate()
    {
        podBounds = transform.GetTotalBounds(Space.World);

        if (!isDigging)
        {
            currentCell = physical.WorldToCell(podBounds.center);
            bool leftCell = physical.HasTile(currentCell + Vector3Int.left);
            bool rightCell = physical.HasTile(currentCell + Vector3Int.right);
            bool botCell = physical.HasTile(currentCell + Vector3Int.down);
            // Debug.Log(currentCell + " => " + physical.CellToWorld(currentCell));

            Ray2D downRay = new Ray2D(podBounds.center.xy() + Vector2.down * podBounds.extents.y, Vector2.down);
            Ray2D leftRay = new Ray2D(podBounds.center.xy() + Vector2.left * podBounds.extents.x, Vector2.left);
            Ray2D rightRay = new Ray2D(podBounds.center.xy() + Vector2.right * podBounds.extents.x, Vector2.right);
            RaycastHit2D downHit = Physics2D.Raycast(downRay.origin, downRay.direction, digReach, digLayer);
            RaycastHit2D leftHit = Physics2D.Raycast(leftRay.origin, leftRay.direction, digReach, digLayer);
            RaycastHit2D rightHit = Physics2D.Raycast(rightRay.origin, rightRay.direction, digReach, digLayer);
            Debug.DrawRay(downRay.origin, downRay.direction * digReach, downHit ? Color.green : Color.red);
            Debug.DrawRay(leftRay.origin, leftRay.direction * digReach, leftHit ? Color.green : Color.red);
            Debug.DrawRay(rightRay.origin, rightRay.direction * digReach, rightHit ? Color.green : Color.red);

            if (botCell && downHit && (Pod.fly < -float.Epsilon && Pod.horizontal > -float.Epsilon && Pod.horizontal < float.Epsilon))
            {
                StartCoroutine(DigTile(currentCell + Vector3Int.down));
            }
            else if (botCell)
            {
                if (leftCell && leftHit && (Pod.horizontal < -float.Epsilon && Pod.fly > -float.Epsilon && Pod.fly < float.Epsilon))
                {
                    StartCoroutine(DigTile(currentCell + Vector3Int.left));
                }
                else if (rightCell && rightHit && (Pod.horizontal > float.Epsilon && Pod.fly > -float.Epsilon && Pod.fly < float.Epsilon))
                {
                    StartCoroutine(DigTile(currentCell + Vector3Int.right));
                }
            }
        }
    }

    private IEnumerator DigTile(Vector3Int tilePosition)
    {
        isDigging = true;

        Rigidbody2D podBody = GetComponentInChildren<Rigidbody2D>();
        PodPhysics2D pod = GetComponentInChildren<PodPhysics2D>();
        UnifiedInputForPod playerInput = GetComponentInChildren<UnifiedInputForPod>();
        // Collider2D[] podColliders = GetComponentsInChildren<Collider2D>();

        //Take away control from player
        if (playerInput != null)
            playerInput.enabled = false;
        //Turn off pod physics so we can move the pod freely
        if (pod != null)
            pod.enabled = false;
        
        //Let the pod be able to go through objects and not fall
        // podBody.gravityScale = 0;
        podBody.bodyType = RigidbodyType2D.Kinematic;
        // for (int i = 0; i < podColliders.Length; i++)
        //     podColliders[i].enabled = false;

        //Stop pod input
        // pod.horizontal = 0;
        // pod.fly = 0;

        Vector2 targetPosition = physical.GetCellCenterWorld(tilePosition).xy();
        Vector2 startPosition = transform.position;
        float startRotation = podBody.rotation;
        // Debug.Log(startPosition + " going to " + targetPosition);
        // var removedTile = physical.GetTile(tilePosition);
        // foreground.SetTile(tilePosition, removedTile);
        // physical.SetTile(tilePosition, null);

        float distance = Vector2.Distance(targetPosition, podBounds.center);
        Vector2 direction = (targetPosition - podBounds.center.xy()).normalized;

        //Move towards dug tile within the timeframe based on digspeed
        int totalTimesteps = Mathf.CeilToInt(distance / digSpeed);
        float stepDistance = digSpeed;
        float rotStep = -startRotation / totalTimesteps;
        for (int i = 0; i < totalTimesteps; i++)
        {
            // Vector2 digForce;
            // if (i == (totalTimesteps - 1))
            //     digForce = podBody.CalculateRequiredForceForPosition(targetPosition);
            // else
                // digForce = podBody.CalculateRequiredForceForSpeed(direction * digSpeed, 0.02f, true);
            // podBody.AddForce(digForce, ForceMode2D.Force);
            podBody.MovePosition(startPosition + direction * stepDistance * i);
            podBody.MoveRotation(startRotation + rotStep * i);

            yield return new WaitForFixedUpdate();
        }

        //Remove any grass on top of tile
        if (foreground.HasTile(tilePosition + Vector3Int.up))
            foreground.SetTile(tilePosition + Vector3Int.up, null);
        physical.SetTile(tilePosition, null); //Remove the dug tile
        // foreground.SetTile(tilePosition, null); //Remove temporary foreground tile

        //Turn colliders back on and the gravity
        podBody.bodyType = RigidbodyType2D.Dynamic;
        // podBody.gravityScale = 1;
        // for (int i = 0; i < podColliders.Length; i++)
        //     podColliders[i].enabled = true;

        //Turn the pod physics back on
        if (pod != null)
            pod.enabled = true;
        //Give back control to player
        if (playerInput != null)
            playerInput.enabled = true;

        isDigging = false;
    }

    private void FindTilemaps()
    {
        Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
        for (int i = 0; i < tilemaps.Length; i++)
        {
            var tileMapRenderer = tilemaps[i].GetComponentInChildren<TilemapRenderer>();
            if (tileMapRenderer != null)
            {
                if (tileMapRenderer.sortingOrder == 1)
                    physical = tilemaps[i];
                else if (tileMapRenderer.sortingOrder == 2)
                    foreground = tilemaps[i];
            }
        }
    }
}
