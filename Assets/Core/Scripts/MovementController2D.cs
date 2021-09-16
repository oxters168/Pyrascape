using UnityEngine;
using UnityHelpers;

[System.Serializable]
public struct InputData
{
    [Range(-1, 1)]
    public float horizontal;
    [Range(-1, 1)]
    public float vertical;
    public bool jump;
}
public struct PhysicalData
{
    public Vector2 velocity;
    public Collider2D leftWall, rightWall, topWall, botWall;
}

public class MovementController2D : MonoBehaviour//, IValueManager
{
    public InputData currentInput;
    private InputData prevInput;

    [Space(10)]
    public float speed = 4;
    public float climbSpeed = 2;
    public float jumpSpeed = 5;
    public float addedFallAcceleration = 9.8f;
    public float wallDetectionDistance = 0.01f;
    public LayerMask groundMask = ~0;
    public LayerMask wallMask = ~0;
    public LayerMask ceilingMask = ~0;

    public float deadzone = 0.1f;

    public enum SpecificState { IdleLeft, IdleRight, RunLeft, RunRight, JumpFaceLeft, JumpFaceRight, JumpMoveLeft, JumpMoveRight, FallFaceLeft, FallFaceRight, FallMoveLeft, FallMoveRight, ClimbLeftIdle, ClimbLeftUp, ClimbLeftDown, ClimbRightIdle, ClimbRightUp, ClimbRightDown, ClimbTopIdleLeft, ClimbTopIdleRight, ClimbTopMoveLeft, ClimbTopMoveRight }
    public enum AnimeState { Idle, Run, Jump, AirFall, Land, TopClimb, TopClimbIdle, SideClimb, SideClimbIdle }

    // [Space(10)]
    // public ValuesVault controlValues;
    private SpriteRenderer Sprite7Up { get { if (_sprite7Up == null) _sprite7Up = GetComponent<SpriteRenderer>(); return _sprite7Up; } }
    private SpriteRenderer _sprite7Up;
    private Rigidbody2D AffectedBody { get { if (_affectedBody == null) _affectedBody = GetComponent<Rigidbody2D>(); return _affectedBody; } }
    private Rigidbody2D _affectedBody;
    private Animator SpriteAnim { get { if (_animator == null) _animator = GetComponent<Animator>(); return _animator; } }
    private Animator _animator;

    private SpecificState prevState;
    private SpecificState currentState;
    private PhysicalData currentPhysicals;

    [Space(10)]
    public float leftDetectOffset;
    public float rightDetectOffset;
    public float topDetectOffset;
    public float bottomDetectOffset;
    public float sideDetectVerticalOffset;
    public float verticalDetectSideOffset;

    [Space(10)]
    public bool debugWallRays = true;
    private Bounds colliderBounds;

    // private float otherObjectHorizontalVelocity;
    // private float otherObjectVerticalVelocity;
    private Vector2 otherObjectVelocity;
    private Vector2 otherObjectPrevVelocity;

    void Update()
    {
        currentPhysicals.velocity = AffectedBody.velocity;
        DetectWall();
        TickState();
        ApplyAnimation();
        // Debug.Log(currentState);
    }
    void FixedUpdate()
    {
        RetrieveSurroundingVelocity();
        MoveCharacter();
        prevInput = currentInput;
    }
    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(colliderBounds.center, colliderBounds.size);
    }

    private void RetrieveSurroundingVelocity()
    {
        otherObjectPrevVelocity = otherObjectVelocity;
        //Get other object's velocity if climbing or standing on it to keep up with it
        if (currentPhysicals.rightWall != null && (currentState == SpecificState.ClimbRightIdle || currentState == SpecificState.ClimbRightUp || currentState == SpecificState.ClimbRightDown))
        {
            var rightWallPhysics = currentPhysicals.rightWall.GetComponentInChildren<Rigidbody2D>();
            if (rightWallPhysics != null)
            {
                otherObjectVelocity = rightWallPhysics.velocity;
            }
            
        }
        else if (currentPhysicals.leftWall != null && (currentState == SpecificState.ClimbLeftIdle || currentState == SpecificState.ClimbLeftUp || currentState == SpecificState.ClimbLeftDown))
        {
            var leftWallPhysics = currentPhysicals.leftWall.GetComponentInChildren<Rigidbody2D>();
            if (leftWallPhysics != null)
            {
                otherObjectVelocity = leftWallPhysics.velocity;
            }
        }
        else if (currentPhysicals.topWall != null && (currentState == SpecificState.ClimbTopIdleLeft || currentState == SpecificState.ClimbTopIdleRight || currentState == SpecificState.ClimbTopMoveLeft || currentState == SpecificState.ClimbTopMoveRight))
        {
            var topWallPhysics = currentPhysicals.topWall.GetComponentInChildren<Rigidbody2D>();
            if (topWallPhysics != null)
            {
                otherObjectVelocity = topWallPhysics.velocity;
            }
        }
        else if (currentPhysicals.botWall != null)
        {
            var botWallPhysics = currentPhysicals.botWall.GetComponentInChildren<Rigidbody2D>();
            if (botWallPhysics != null)
            {
                otherObjectVelocity = botWallPhysics.velocity; //Might want to remove y value because object is below us and we are only standing on it, not grabbing on to it
            }
        }
    }
    private void ApplyAnimation()
    {
        Sprite7Up.flipX = IsFacingRight(currentState);
        var prevAnimeState = GetAnimeFromState(prevState);
        var currentAnimeState = GetAnimeFromState(currentState);
        if (prevAnimeState != currentAnimeState)
            SpriteAnim.SetTrigger(currentAnimeState.ToString());
    }
    private void MoveCharacter()
    {
        float horizontalForce = 0;
        float verticalForce = 0;

        var prevAnimeState = GetAnimeFromState(prevState);
        var currentAnimeState = GetAnimeFromState(currentState);
        var isFacingRight = IsFacingRight(currentState);

        Vector2 otherObjectPredictedVelocity = (otherObjectVelocity - otherObjectPrevVelocity);
        
        float horizontalVelocity = 0;
        if (currentAnimeState == AnimeState.Idle || currentAnimeState == AnimeState.Run || currentAnimeState == AnimeState.SideClimb || currentAnimeState == AnimeState.SideClimbIdle || currentAnimeState == AnimeState.TopClimb || currentAnimeState == AnimeState.TopClimbIdle || currentState == SpecificState.FallMoveLeft || currentState == SpecificState.FallMoveRight || currentState == SpecificState.JumpMoveLeft || currentState == SpecificState.JumpMoveRight)
        {
            if (currentAnimeState == AnimeState.TopClimb)
                horizontalVelocity = (isFacingRight ? 1 : -1) * climbSpeed;
            else if (currentAnimeState != AnimeState.Idle && currentAnimeState != AnimeState.TopClimbIdle && currentAnimeState != AnimeState.SideClimb && currentAnimeState != AnimeState.SideClimbIdle)
                horizontalVelocity = (isFacingRight ? 1 : -1) * speed;
        }
        // horizontalForce = PhysicsHelpers.CalculateRequiredForceForSpeed(AffectedBody.mass, AffectedBody.velocity.x, horizontalVelocity, Time.fixedDeltaTime);
        horizontalForce = PhysicsHelpers.CalculateRequiredForceForSpeed(AffectedBody.mass, AffectedBody.velocity.x, (otherObjectVelocity.x + otherObjectPredictedVelocity.x) + horizontalVelocity, Time.fixedDeltaTime);

        // Debug.Log(currentState + " from " + prevState);
        if (currentAnimeState == AnimeState.SideClimb || currentAnimeState == AnimeState.SideClimbIdle || currentAnimeState == AnimeState.TopClimb || currentAnimeState == AnimeState.TopClimbIdle || (currentAnimeState == AnimeState.Jump && prevAnimeState != AnimeState.Jump))
        {
            float verticalSpeed = 0;
            if (currentState == SpecificState.ClimbRightUp || currentState == SpecificState.ClimbLeftUp)
                verticalSpeed = climbSpeed;
            else if (currentState == SpecificState.ClimbLeftDown || currentState == SpecificState.ClimbRightDown)
                verticalSpeed = -climbSpeed;
            else if (currentAnimeState != AnimeState.SideClimbIdle && currentAnimeState != AnimeState.TopClimb && currentAnimeState != AnimeState.TopClimbIdle)
                verticalSpeed = jumpSpeed;

            // verticalForce = PhysicsHelpers.CalculateRequiredForceForSpeed(AffectedBody.mass, AffectedBody.velocity.y, verticalSpeed, Time.fixedDeltaTime, true);
            verticalForce = PhysicsHelpers.CalculateRequiredForceForSpeed(AffectedBody.mass, AffectedBody.velocity.y, (otherObjectVelocity.y + otherObjectPredictedVelocity.y) + verticalSpeed, Time.fixedDeltaTime, true);
        }

        //Added weight when falling for better feel
        if (currentAnimeState == AnimeState.AirFall && currentPhysicals.velocity.y < -float.Epsilon)
            verticalForce -= AffectedBody.mass * addedFallAcceleration;

        //When the jump button stops being held then stop ascending
        if (currentAnimeState == AnimeState.Jump && currentPhysicals.velocity.y > 0 && prevInput.jump && !currentInput.jump)
            verticalForce = PhysicsHelpers.CalculateRequiredForceForSpeed(AffectedBody.mass, AffectedBody.velocity.y, 0, Time.fixedDeltaTime);
        
        // if (Mathf.Abs(currentInput.horizontal) > float.Epsilon || (Mathf.Abs(currentInput.horizontal) < float.Epsilon && Mathf.Abs(prevInput.horizontal) > float.Epsilon))
        if (Mathf.Abs(horizontalForce) > float.Epsilon)
            AffectedBody.AddForce(Vector2.right * horizontalForce);
        if (Mathf.Abs(verticalForce) > float.Epsilon)
            AffectedBody.AddForce(Vector2.up * verticalForce);
    }

    private void TickState()
    {
        var nextState = GetNextState(currentState, prevState, currentInput, currentPhysicals, deadzone);
        prevState = currentState;
        currentState = nextState;
    }
    private static SpecificState GetNextState(SpecificState currentState, SpecificState prevState, InputData currentInput, PhysicalData currentPhysicals, float deadzone = float.Epsilon)
    {
        var nextState = currentState;

        switch (currentState)
        {
            case SpecificState.IdleLeft:
                if (currentInput.horizontal > deadzone)
                    nextState = SpecificState.IdleRight;
                else if (currentInput.horizontal < -deadzone)
                    nextState = SpecificState.RunLeft;
                else if (currentPhysicals.velocity.y < -deadzone)
                    nextState = SpecificState.FallFaceLeft;
                else if (currentInput.jump)
                    nextState = SpecificState.JumpFaceLeft;
                else if (currentPhysicals.topWall && currentInput.vertical > deadzone)
                    nextState = SpecificState.ClimbTopIdleLeft;
                break;
            case SpecificState.IdleRight:
                if (currentInput.horizontal > deadzone)
                    nextState = SpecificState.RunRight;
                else if (currentInput.horizontal < -deadzone)
                    nextState = SpecificState.IdleLeft;
                else if (currentPhysicals.velocity.y < -deadzone)
                    nextState = SpecificState.FallFaceLeft;
                else if (currentInput.jump)
                    nextState = SpecificState.JumpFaceRight;
                else if (currentPhysicals.topWall && currentInput.vertical > deadzone)
                    nextState = SpecificState.ClimbTopIdleRight;
                break;
            case SpecificState.RunLeft:
                if (currentInput.horizontal > -deadzone)
                    nextState = SpecificState.IdleLeft;
                else if (currentPhysicals.velocity.y < -deadzone)
                    nextState = SpecificState.FallMoveLeft;
                else if (currentInput.jump)
                    nextState = SpecificState.JumpMoveLeft;
                else if (currentPhysicals.leftWall)
                    nextState = SpecificState.ClimbLeftIdle;
                else if (currentPhysicals.topWall && currentInput.vertical > deadzone)
                    nextState = SpecificState.ClimbTopMoveLeft;
                break;
            case SpecificState.RunRight:
                if (currentInput.horizontal < deadzone)
                    nextState = SpecificState.IdleRight;
                else if (currentPhysicals.velocity.y < -deadzone)
                    nextState = SpecificState.FallMoveRight;
                else if (currentInput.jump)
                    nextState = SpecificState.JumpMoveRight;
                else if (currentPhysicals.rightWall)
                    nextState = SpecificState.ClimbRightIdle;
                else if (currentPhysicals.topWall && currentInput.vertical > deadzone)
                    nextState = SpecificState.ClimbTopMoveRight;
                break;
            case SpecificState.JumpFaceLeft:
                if (currentPhysicals.velocity.y < -deadzone)
                    nextState = SpecificState.FallFaceLeft;
                else if (currentInput.horizontal < -deadzone)
                    nextState = SpecificState.JumpMoveLeft;
                else if (currentInput.horizontal > deadzone)
                    nextState = SpecificState.JumpFaceRight;
                else if (currentPhysicals.botWall)
                    nextState = SpecificState.IdleLeft;
                else if (currentPhysicals.topWall && currentInput.vertical > deadzone)
                    nextState = SpecificState.ClimbTopIdleLeft;
                break;
            case SpecificState.JumpFaceRight:
                if (currentPhysicals.velocity.y < -deadzone)
                    nextState = SpecificState.FallFaceRight;
                else if (currentInput.horizontal > deadzone)
                    nextState = SpecificState.JumpMoveRight;
                else if (currentInput.horizontal < -deadzone)
                    nextState = SpecificState.JumpFaceLeft;
                else if (currentPhysicals.botWall)
                    nextState = SpecificState.IdleRight;
                else if (currentPhysicals.topWall && currentInput.vertical > deadzone)
                    nextState = SpecificState.ClimbTopIdleRight;
                break;
            case SpecificState.JumpMoveLeft:
                if (currentPhysicals.velocity.y < -deadzone)
                    nextState = SpecificState.FallMoveLeft;
                else if (currentInput.horizontal > -deadzone)
                    nextState = SpecificState.JumpFaceLeft;
                else if (currentPhysicals.botWall)
                    nextState = SpecificState.RunLeft;
                else if (currentPhysicals.leftWall)
                    nextState = SpecificState.ClimbLeftIdle;
                else if (currentPhysicals.topWall && currentInput.vertical > deadzone)
                    nextState = SpecificState.ClimbTopMoveLeft;
                break;
            case SpecificState.JumpMoveRight:
                if (currentPhysicals.velocity.y < -deadzone)
                    nextState = SpecificState.FallMoveRight;
                else if (currentInput.horizontal < deadzone)
                    nextState = SpecificState.JumpFaceRight;
                else if (currentPhysicals.botWall)
                    nextState = SpecificState.RunRight;
                else if (currentPhysicals.rightWall)
                    nextState = SpecificState.ClimbRightIdle;
                else if (currentPhysicals.topWall && currentInput.vertical > deadzone)
                    nextState = SpecificState.ClimbTopMoveRight;
                break;
            case SpecificState.FallFaceLeft:
                if (currentPhysicals.botWall)
                    nextState = SpecificState.IdleLeft;
                else if (currentInput.horizontal < -deadzone)
                    nextState = SpecificState.FallMoveLeft;
                else if (currentInput.horizontal > deadzone)
                    nextState = SpecificState.FallFaceRight;
                break;
            case SpecificState.FallFaceRight:
                if (currentPhysicals.botWall)
                    nextState = SpecificState.IdleRight;
                else if (currentInput.horizontal > deadzone)
                    nextState = SpecificState.FallMoveRight;
                else if (currentInput.horizontal < -deadzone)
                    nextState = SpecificState.FallFaceLeft;
                break;
            case SpecificState.FallMoveLeft:
                if (currentPhysicals.botWall)
                    nextState = SpecificState.RunLeft;
                else if (currentInput.horizontal > -deadzone)
                    nextState = SpecificState.FallFaceLeft;
                else if (currentPhysicals.leftWall)
                    nextState = SpecificState.ClimbLeftIdle;
                break;
            case SpecificState.FallMoveRight:
                if (currentPhysicals.botWall)
                    nextState = SpecificState.RunRight;
                else if (currentInput.horizontal < deadzone)
                    nextState = SpecificState.FallFaceRight;
                else if (currentPhysicals.rightWall)
                    nextState = SpecificState.ClimbRightIdle;
                break;
            case SpecificState.ClimbLeftIdle:
                if (currentInput.horizontal > -deadzone)
                    nextState = SpecificState.FallFaceLeft;
                else if (currentInput.vertical > deadzone)
                    nextState = SpecificState.ClimbLeftUp;
                else if (currentInput.vertical < -deadzone && !currentPhysicals.botWall)
                    nextState = SpecificState.ClimbLeftDown;
                else if (!currentPhysicals.leftWall)
                    nextState = SpecificState.FallFaceLeft;
                break;
            case SpecificState.ClimbRightIdle:
                if (currentInput.horizontal < deadzone)
                    nextState = SpecificState.FallFaceRight;
                else if (currentInput.vertical > deadzone)
                    nextState = SpecificState.ClimbRightUp;
                else if (currentInput.vertical < -deadzone && !currentPhysicals.botWall)
                    nextState = SpecificState.ClimbRightDown;
                else if (!currentPhysicals.rightWall)
                    nextState = SpecificState.FallFaceRight;
                break;
            case SpecificState.ClimbLeftUp:
                if (currentInput.horizontal > -deadzone)
                    nextState = SpecificState.FallFaceLeft;
                else if (currentInput.vertical < deadzone)
                    nextState = SpecificState.ClimbLeftIdle;
                else if (!currentPhysicals.leftWall)
                    nextState = SpecificState.FallMoveLeft;
                else if (currentPhysicals.topWall && currentInput.vertical > deadzone)
                    nextState = SpecificState.ClimbTopIdleLeft;
                else if (!currentPhysicals.leftWall)
                    nextState = SpecificState.FallFaceLeft;
                break;
            case SpecificState.ClimbRightUp:
                if (currentInput.horizontal < deadzone)
                    nextState = SpecificState.FallFaceRight;
                else if (currentInput.vertical < deadzone)
                    nextState = SpecificState.ClimbRightIdle;
                else if (!currentPhysicals.rightWall)
                    nextState = SpecificState.FallMoveRight;
                else if (currentPhysicals.topWall && currentInput.vertical > deadzone)
                    nextState = SpecificState.ClimbTopIdleRight;
                else if (!currentPhysicals.rightWall)
                    nextState = SpecificState.FallFaceRight;
                break;
            case SpecificState.ClimbLeftDown:
                if (currentInput.horizontal > -deadzone)
                    nextState = SpecificState.FallFaceLeft;
                else if (currentInput.vertical > -deadzone)
                    nextState = SpecificState.ClimbLeftIdle;
                else if (!currentPhysicals.leftWall)
                    nextState = SpecificState.FallMoveLeft;
                else if (currentPhysicals.botWall)
                    nextState = SpecificState.ClimbLeftIdle;
                else if (!currentPhysicals.leftWall)
                    nextState = SpecificState.FallFaceLeft;
                break;
            case SpecificState.ClimbRightDown:
                if (currentInput.horizontal < deadzone)
                    nextState = SpecificState.FallFaceRight;
                else if (currentInput.vertical > -deadzone)
                    nextState = SpecificState.ClimbRightIdle;
                else if (!currentPhysicals.rightWall)
                    nextState = SpecificState.FallMoveRight;
                else if (currentPhysicals.botWall)
                    nextState = SpecificState.ClimbRightIdle;
                else if (!currentPhysicals.rightWall)
                    nextState = SpecificState.FallFaceRight;
                break;
            case SpecificState.ClimbTopIdleLeft:
                if (currentInput.vertical < deadzone)
                    nextState = SpecificState.FallFaceLeft;
                else if (currentInput.horizontal < -deadzone && !currentPhysicals.leftWall)
                    nextState = SpecificState.ClimbTopMoveLeft;
                else if (currentInput.horizontal > deadzone)
                    nextState = SpecificState.ClimbTopIdleRight;
                else if (!currentPhysicals.topWall)
                    nextState = SpecificState.FallFaceLeft;
                break;
            case SpecificState.ClimbTopIdleRight:
                if (currentInput.vertical < deadzone)
                    nextState = SpecificState.FallFaceRight;
                else if (currentInput.horizontal > deadzone && !currentPhysicals.rightWall)
                    nextState = SpecificState.ClimbTopMoveRight;
                else if (currentInput.horizontal < -deadzone)
                    nextState = SpecificState.ClimbTopIdleLeft;
                else if (!currentPhysicals.topWall)
                    nextState = SpecificState.FallFaceRight;
                break;
            case SpecificState.ClimbTopMoveLeft:
                if (currentInput.vertical < deadzone)
                    nextState = SpecificState.FallMoveLeft;
                else if (currentInput.horizontal > -deadzone)
                    nextState = SpecificState.ClimbTopIdleLeft;
                else if (!currentPhysicals.topWall)
                    nextState = SpecificState.FallMoveLeft;
                else if (currentPhysicals.leftWall)
                    nextState = SpecificState.ClimbTopIdleLeft;
                else if (!currentPhysicals.topWall)
                    nextState = SpecificState.FallFaceLeft;
                break;
            case SpecificState.ClimbTopMoveRight:
                if (currentInput.vertical < deadzone)
                    nextState = SpecificState.FallMoveRight;
                else if (currentInput.horizontal < deadzone)
                    nextState = SpecificState.ClimbTopIdleRight;
                else if (!currentPhysicals.topWall)
                    nextState = SpecificState.FallMoveRight;
                else if (currentPhysicals.rightWall)
                    nextState = SpecificState.ClimbTopIdleRight;
                else if (!currentPhysicals.topWall)
                    nextState = SpecificState.FallFaceRight;
                break;
        }
        return nextState;
    }
    private static bool IsFacingRight(SpecificState state)
    {
        bool isFacingRight;

        switch (state)
        {
            case SpecificState.IdleRight:
            case SpecificState.RunRight:
            case SpecificState.JumpFaceRight:
            case SpecificState.JumpMoveRight:
            case SpecificState.FallFaceRight:
            case SpecificState.FallMoveRight:
            case SpecificState.ClimbRightIdle:
            case SpecificState.ClimbRightUp:
            case SpecificState.ClimbRightDown:
            case SpecificState.ClimbTopIdleRight:
            case SpecificState.ClimbTopMoveRight:
                isFacingRight = true;
                break;
            default:
                isFacingRight = false;
                break;
        }

        return isFacingRight;
    }
    private static AnimeState GetAnimeFromState(SpecificState state)
    {
        var animeState = AnimeState.Idle;

        switch (state)
        {
            case SpecificState.IdleLeft:
            case SpecificState.IdleRight:
                animeState = AnimeState.Idle;
                break;
            case SpecificState.RunLeft:
            case SpecificState.RunRight:
                animeState = AnimeState.Run;
                break;
            case SpecificState.JumpFaceLeft:
            case SpecificState.JumpFaceRight:
            case SpecificState.JumpMoveLeft:
            case SpecificState.JumpMoveRight:
                animeState = AnimeState.Jump;
                break;
            case SpecificState.FallFaceLeft:
            case SpecificState.FallFaceRight:
            case SpecificState.FallMoveLeft:
            case SpecificState.FallMoveRight:
                animeState = AnimeState.AirFall;
                break;
            case SpecificState.ClimbLeftIdle:
            case SpecificState.ClimbRightIdle:
                animeState = AnimeState.SideClimbIdle;
                break;
            case SpecificState.ClimbLeftUp:
            case SpecificState.ClimbLeftDown:
            case SpecificState.ClimbRightUp:
            case SpecificState.ClimbRightDown:
                animeState = AnimeState.SideClimb;
                break;
            case SpecificState.ClimbTopIdleLeft:
            case SpecificState.ClimbTopIdleRight:
                animeState = AnimeState.TopClimbIdle;
                break;
            case SpecificState.ClimbTopMoveLeft:
            case SpecificState.ClimbTopMoveRight:
                animeState = AnimeState.TopClimb;
                break;
        }

        return animeState;
    }

    private Collider2D WallCast(bool debug, LayerMask mask, params Ray2D[] rays)
    {
        Collider2D wallHit = null;
        foreach (var rightRay in rays)
        {
            var rightHitInfo = Physics2D.Raycast(rightRay.origin, rightRay.direction, wallDetectionDistance, mask);
            if (debug)
                Debug.DrawRay(rightRay.origin, rightRay.direction * wallDetectionDistance, rightHitInfo.transform != null ? Color.green : Color.red);
            if (rightHitInfo)
            {
                wallHit = rightHitInfo.collider;
                break;
            }
        }
        return wallHit;
    }
    private void DetectWall()
    {
        colliderBounds = transform.GetTotalBounds(Space.Self, true);

        var rightRayBot = new Ray2D(transform.position + transform.right * (colliderBounds.size.x / 2 + rightDetectOffset) + -transform.up * (bottomDetectOffset  + sideDetectVerticalOffset), transform.right);
        var rightRayCenter = new Ray2D(transform.position + transform.up * (colliderBounds.extents.y) + transform.right * (colliderBounds.extents.x + rightDetectOffset), transform.right);
        var rightRayTop = new Ray2D(transform.position + transform.up * (colliderBounds.size.y + topDetectOffset + sideDetectVerticalOffset) + transform.right * (colliderBounds.size.x / 2 + rightDetectOffset), transform.right);
        currentPhysicals.rightWall = WallCast(debugWallRays, wallMask, rightRayTop, rightRayCenter, rightRayBot);

        var leftRayBot = new Ray2D(transform.position + -transform.right * (colliderBounds.size.x / 2 + leftDetectOffset) + -transform.up * (bottomDetectOffset + sideDetectVerticalOffset), -transform.right);
        var leftRayCenter = new Ray2D(transform.position + transform.up * (colliderBounds.extents.y) + -transform.right * (colliderBounds.extents.x + leftDetectOffset), -transform.right);
        var leftRayTop = new Ray2D(transform.position + transform.up * (colliderBounds.size.y + topDetectOffset + sideDetectVerticalOffset) + -transform.right * (colliderBounds.size.x / 2 + leftDetectOffset), -transform.right);
        currentPhysicals.leftWall = WallCast(debugWallRays, wallMask, leftRayTop, leftRayCenter, leftRayBot);

        var topRightRay = new Ray2D(transform.position + transform.up * (colliderBounds.size.y + topDetectOffset) + transform.right * (colliderBounds.extents.x + rightDetectOffset + verticalDetectSideOffset), transform.up);
        var topCenterRay = new Ray2D(transform.position + transform.up * (colliderBounds.size.y + topDetectOffset), transform.up);
        var topLeftRay = new Ray2D(transform.position + transform.up * (colliderBounds.size.y + topDetectOffset) + -transform.right * (colliderBounds.extents.x + leftDetectOffset + verticalDetectSideOffset), transform.up);
        currentPhysicals.topWall = WallCast(debugWallRays, ceilingMask, topLeftRay, topCenterRay, topRightRay);

        var botRightRay = new Ray2D(transform.position + transform.right * (colliderBounds.extents.x + rightDetectOffset + verticalDetectSideOffset) + -transform.up * (bottomDetectOffset), -transform.up);
        var botCenterRay = new Ray2D(transform.position + -transform.up * (bottomDetectOffset), -transform.up);
        var botLeftRay = new Ray2D(transform.position + -transform.right * (colliderBounds.extents.x + leftDetectOffset + verticalDetectSideOffset) + -transform.up * (bottomDetectOffset), -transform.up);
        currentPhysicals.botWall = WallCast(debugWallRays, groundMask, botLeftRay, botCenterRay, botRightRay);
    }
}
