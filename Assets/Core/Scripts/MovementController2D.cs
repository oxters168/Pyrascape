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
    public bool leftWall, rightWall, topWall, botWall;
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

    void Update()
    {
        // ReadInput();
        DetectWall();
        currentPhysicals.velocity = AffectedBody.velocity;
        TickState();
        ApplyAnimation();
    }
    void FixedUpdate()
    {
        MoveCharacter();
        prevInput = currentInput;
    }
    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(colliderBounds.center, colliderBounds.size);
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
        
        float horizontalVelocity = 0;
        if (currentAnimeState == AnimeState.Idle || currentAnimeState == AnimeState.Run || currentAnimeState == AnimeState.SideClimb || currentAnimeState == AnimeState.SideClimbIdle || currentAnimeState == AnimeState.TopClimb || currentAnimeState == AnimeState.TopClimbIdle || currentState == SpecificState.FallMoveLeft || currentState == SpecificState.FallMoveRight || currentState == SpecificState.JumpMoveLeft || currentState == SpecificState.JumpMoveRight)
        {
            if (currentAnimeState == AnimeState.TopClimb)
                horizontalVelocity = (isFacingRight ? 1 : -1) * climbSpeed;
            else if (currentAnimeState != AnimeState.Idle && currentAnimeState != AnimeState.TopClimbIdle && currentAnimeState != AnimeState.SideClimb && currentAnimeState != AnimeState.SideClimbIdle)
                horizontalVelocity = (isFacingRight ? 1 : -1) * speed;
        }
        horizontalForce = PhysicsHelpers.CalculateRequiredForceForSpeed(AffectedBody.mass, AffectedBody.velocity.x, horizontalVelocity, Time.fixedDeltaTime);

        if (currentAnimeState == AnimeState.SideClimb || currentAnimeState == AnimeState.SideClimbIdle || currentAnimeState == AnimeState.TopClimb || currentAnimeState == AnimeState.TopClimbIdle || (currentAnimeState == AnimeState.Jump && prevAnimeState != AnimeState.Jump))
        {
            float verticalSpeed = 0;
            if (currentState == SpecificState.ClimbRightUp || currentState == SpecificState.ClimbLeftUp)
                verticalSpeed = climbSpeed;
            else if (currentState == SpecificState.ClimbLeftDown || currentState == SpecificState.ClimbRightDown)
                verticalSpeed = -climbSpeed;
            else if (currentAnimeState != AnimeState.SideClimbIdle && currentAnimeState != AnimeState.TopClimb && currentAnimeState != AnimeState.TopClimbIdle)
                verticalSpeed = jumpSpeed;

            verticalForce = PhysicsHelpers.CalculateRequiredForceForSpeed(AffectedBody.mass, AffectedBody.velocity.y, verticalSpeed, Time.fixedDeltaTime, true);
        }

        //Added weight when falling for better feel
        if (currentAnimeState == AnimeState.AirFall && currentPhysicals.velocity.y < -float.Epsilon)
            verticalForce -= AffectedBody.mass * addedFallAcceleration;

        //When the jump button stops being held then stop ascending
        if (currentAnimeState == AnimeState.Jump && currentPhysicals.velocity.y > 0 && prevInput.jump && !currentInput.jump)
            verticalForce = PhysicsHelpers.CalculateRequiredForceForSpeed(AffectedBody.mass, AffectedBody.velocity.y, 0, Time.fixedDeltaTime);
        
        if (Mathf.Abs(horizontalVelocity) > float.Epsilon || (Mathf.Abs(currentInput.horizontal) < float.Epsilon && Mathf.Abs(prevInput.horizontal) > float.Epsilon) || Mathf.Abs(verticalForce) > float.Epsilon)
            AffectedBody.AddForce(Vector2.right * horizontalForce + Vector2.up * verticalForce);
    }

    private void TickState()
    {
        prevState = currentState;
        currentState = GetNextState(currentState, currentInput, currentPhysicals, deadzone);
    }
    private static SpecificState GetNextState(SpecificState currentState, InputData currentInput, PhysicalData currentPhysicals, float deadzone = float.Epsilon)
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
                break;
            case SpecificState.ClimbRightIdle:
                if (currentInput.horizontal < deadzone)
                    nextState = SpecificState.FallFaceRight;
                else if (currentInput.vertical > deadzone)
                    nextState = SpecificState.ClimbRightUp;
                else if (currentInput.vertical < -deadzone && !currentPhysicals.botWall)
                    nextState = SpecificState.ClimbRightDown;
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
                break;
            case SpecificState.ClimbTopIdleLeft:
                if (currentInput.vertical < deadzone)
                    nextState = SpecificState.FallFaceLeft;
                else if (currentInput.horizontal < -deadzone && !currentPhysicals.leftWall)
                    nextState = SpecificState.ClimbTopMoveLeft;
                else if (currentInput.horizontal > deadzone)
                    nextState = SpecificState.ClimbTopIdleRight;
                break;
            case SpecificState.ClimbTopIdleRight:
                if (currentInput.vertical < deadzone)
                    nextState = SpecificState.FallFaceRight;
                else if (currentInput.horizontal > deadzone && !currentPhysicals.rightWall)
                    nextState = SpecificState.ClimbTopMoveRight;
                else if (currentInput.horizontal < -deadzone)
                    nextState = SpecificState.ClimbTopIdleLeft;
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

    private bool WallCast(bool debug, LayerMask mask, params Ray2D[] rays)
    {
        bool wallHit = false;
        foreach (var rightRay in rays)
        {
            var rightHitInfo = Physics2D.Raycast(rightRay.origin, rightRay.direction, wallDetectionDistance, mask);
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
        var rightRayBot = new Ray2D(transform.position + transform.right * (colliderBounds.size.x / 2 + rightDetectOffset) + -transform.up * (bottomDetectOffset  + sideDetectVerticalOffset), transform.right);
        var rightRayTop = new Ray2D(transform.position + transform.up * (colliderBounds.size.y + topDetectOffset + sideDetectVerticalOffset) + transform.right * (colliderBounds.size.x / 2 + rightDetectOffset), transform.right);
        var leftRayBot = new Ray2D(transform.position + -transform.right * (colliderBounds.size.x / 2 + leftDetectOffset) + -transform.up * (bottomDetectOffset + sideDetectVerticalOffset), -transform.right);
        var leftRayTop = new Ray2D(transform.position + transform.up * (colliderBounds.size.y + topDetectOffset + sideDetectVerticalOffset) + -transform.right * (colliderBounds.size.x / 2 + leftDetectOffset), -transform.right);
        
        currentPhysicals.rightWall = WallCast(debugWallRays, wallMask, rightRayTop, rightRayBot);
        currentPhysicals.leftWall = WallCast(debugWallRays, wallMask, leftRayTop, leftRayBot);

        var topRightRay = new Ray2D(transform.position + transform.up * (colliderBounds.size.y + topDetectOffset) + transform.right * (colliderBounds.size.x / 2 + rightDetectOffset + verticalDetectSideOffset), transform.up);
        var topLeftRay = new Ray2D(transform.position + transform.up * (colliderBounds.size.y + topDetectOffset) + -transform.right * (colliderBounds.size.x / 2 + leftDetectOffset + verticalDetectSideOffset), transform.up);
        currentPhysicals.topWall = WallCast(debugWallRays, ceilingMask, topLeftRay, topRightRay);

        var botRightRay = new Ray2D(transform.position + transform.right * (colliderBounds.size.x / 2 + rightDetectOffset + verticalDetectSideOffset) + -transform.up * (bottomDetectOffset), -transform.up);
        var botLeftRay = new Ray2D(transform.position + -transform.right * (colliderBounds.size.x / 2 + leftDetectOffset + verticalDetectSideOffset) + -transform.up * (bottomDetectOffset), -transform.up);
        currentPhysicals.botWall = WallCast(debugWallRays, groundMask, botLeftRay, botRightRay);
    }
    // private void ReadInput()
    // {
    //     currentInput.horizontal = Mathf.Clamp(GetAxis("Horizontal"), -1, 1);
    //     currentInput.vertical = Mathf.Clamp(GetAxis("Vertical"), -1, 1);
    //     currentInput.jump = GetToggle("ButtonA");
    // }

    // public void SetAxis(string name, float value)
    // {
    //     controlValues.GetValue(name).SetAxis(value);
    // }
    // public float GetAxis(string name)
    // {
    //     return controlValues.GetValue(name).GetAxis();
    // }
    // public void SetToggle(string name, bool value)
    // {
    //     controlValues.GetValue(name).SetToggle(value);
    // }
    // public bool GetToggle(string name)
    // {
    //     return controlValues.GetValue(name).GetToggle();
    // }
    // public void SetDirection(string name, Vector3 value)
    // {
    //     controlValues.GetValue(name).SetDirection(value);
    // }
    // public Vector3 GetDirection(string name)
    // {
    //     return controlValues.GetValue(name).GetDirection();
    // }
    // public void SetPoint(string name, Vector3 value)
    // {
    //     controlValues.GetValue(name).SetPoint(value);
    // }
    // public Vector3 GetPoint(string name)
    // {
    //     return controlValues.GetValue(name).GetPoint();
    // }
    // public void SetOrientation(string name, Quaternion value)
    // {
    //     controlValues.GetValue(name).SetOrientation(value);
    // }
    // public Quaternion GetOrientation(string name)
    // {
    //     return controlValues.GetValue(name).GetOrientation();
    // }
}
