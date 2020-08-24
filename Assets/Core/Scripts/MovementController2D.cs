using UnityEngine;
using UnityHelpers;

public class MovementController2D : MonoBehaviour, IValueManager
{
    public float speed = 4;
    public float climbSpeed = 2;
    public float jumpSpeed = 5;
    public float wallDetectionDistance = 0.01f;
    public LayerMask wallMask = ~0;

    [Space(10)]
    public ValuesVault controlValues;
    private SpriteRenderer Sprite7Up { get { if (_sprite7Up == null) _sprite7Up = GetComponent<SpriteRenderer>(); return _sprite7Up; } }
    private SpriteRenderer _sprite7Up;
    private Rigidbody2D AffectedBody { get { if (_affectedBody == null) _affectedBody = GetComponent<Rigidbody2D>(); return _affectedBody; } }
    private Rigidbody2D _affectedBody;

    private float horizontal;
    private float vertical;
    private bool buttonA;

    private bool isFacingRight;
    [Space(10)]
    public bool debugWallRays = true;
    private bool leftWall, rightWall, topWall, botWall;
    private Bounds colliderBounds;

    void Update()
    {
        ReadInput();
        CheckDirection();

        Sprite7Up.flipX = isFacingRight;
    }
    void FixedUpdate()
    {
        DetectWall();

        if ((leftWall && horizontal < -float.Epsilon) || (rightWall && horizontal > float.Epsilon))
        {
            var verticalForce = PhysicsHelpers.CalculateRequiredForceForSpeed(AffectedBody.mass, AffectedBody.velocity.y, vertical * climbSpeed, Time.fixedDeltaTime, true);
            AffectedBody.AddForce(Vector2.up * verticalForce);
        }
        else if (topWall && vertical > float.Epsilon)
        {
            var horizontalForce = PhysicsHelpers.CalculateRequiredForceForSpeed(AffectedBody.mass, AffectedBody.velocity.x, horizontal * climbSpeed);
            var verticalForce = PhysicsHelpers.CalculateRequiredForceForSpeed(AffectedBody.mass, AffectedBody.velocity.y, 0, Time.fixedDeltaTime, true);
            AffectedBody.AddForce(Vector2.right * horizontalForce + Vector2.up * verticalForce);
        }
        else
        {
            float verticalForce = 0;
            if (buttonA && botWall && AffectedBody.velocity.y > -float.Epsilon && AffectedBody.velocity.y < float.Epsilon)
                verticalForce = PhysicsHelpers.CalculateRequiredForceForSpeed(AffectedBody.mass, AffectedBody.velocity.y, jumpSpeed, Time.fixedDeltaTime, true);
                
            var horizontalForce = PhysicsHelpers.CalculateRequiredForceForSpeed(AffectedBody.mass, AffectedBody.velocity.x, horizontal * speed);
            AffectedBody.AddForce(Vector2.right * horizontalForce + Vector2.up * verticalForce);
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(colliderBounds.center, colliderBounds.size);
    }

    private bool WallCast(bool debug, params Ray2D[] rays)
    {
        bool wallHit = false;
        foreach (var rightRay in rays)
        {
            var rightHitInfo = Physics2D.Raycast(rightRay.origin, rightRay.direction, wallDetectionDistance, wallMask);
            if (debug)
                Debug.DrawRay(rightRay.origin, rightRay.direction * wallDetectionDistance, rightHitInfo.transform != null ? Color.green : Color.red);
            if (rightHitInfo.transform != null)
            {
                wallHit = true;
                break;
            }
        }
        return wallHit;
    }
    private void DetectWall()
    {
        colliderBounds = transform.GetTotalBounds(Space.Self, true);
        var rightRayBot = new Ray2D(transform.position + transform.right * colliderBounds.size.x / 2, transform.right);
        var rightRayTop = new Ray2D(transform.position + transform.up * colliderBounds.size.y + transform.right * colliderBounds.size.x / 2, transform.right);
        var leftRayBot = new Ray2D(transform.position + -transform.right * colliderBounds.size.x / 2, -transform.right);
        var leftRayTop = new Ray2D(transform.position + transform.up * colliderBounds.size.y + -transform.right * colliderBounds.size.x / 2, -transform.right);
        
        rightWall = WallCast(debugWallRays, rightRayTop, rightRayBot);
        leftWall = WallCast(debugWallRays, leftRayTop, leftRayBot);

        var topRightRay = new Ray2D(transform.position + transform.up * colliderBounds.size.y + transform.right * colliderBounds.size.x / 2, transform.up);
        var topLeftRay = new Ray2D(transform.position + transform.up * colliderBounds.size.y + -transform.right * colliderBounds.size.x / 2, transform.up);
        topWall = WallCast(debugWallRays, topLeftRay, topRightRay);

        var botRightRay = new Ray2D(transform.position + transform.right * colliderBounds.size.x / 2, -transform.up);
        var botLeftRay = new Ray2D(transform.position + -transform.right * colliderBounds.size.x / 2, -transform.up);
        botWall = WallCast(debugWallRays, botLeftRay, botRightRay);
    }
    private void ReadInput()
    {
        horizontal = Mathf.Clamp(GetAxis("Horizontal"), -1, 1);
        vertical = Mathf.Clamp(GetAxis("Vertical"), -1, 1);
        buttonA = GetToggle("ButtonA");
    }
    private void CheckDirection()
    {
        if (AffectedBody.velocity.x < -float.Epsilon)
            isFacingRight = false;
        else if (AffectedBody.velocity.x > float.Epsilon)
            isFacingRight = true;
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
}
