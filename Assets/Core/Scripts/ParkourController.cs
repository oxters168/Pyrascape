using UnityEngine;
using UnityHelpers;

public class ParkourController : MonoBehaviour
{
    public Animator characterAnimator;
    public Rigidbody characterBody;

    [Space(10), Tooltip("The name of the boolean value related to falling in the animator")]
    public string fallingAnimatorName = "Falling";
    [Tooltip("The name of the float value related to jump in the animator")]
    public string jumpAnimatorName = "Jump";
    [Tooltip("The name of the boolean value related to landing on a surface in the animator")]
    public string landAnimationName = "Land";
    [Tooltip("The name of the float value related to peak falling height in the animator")]
    public string peakHeightAnimationName = "PeakHeight";

    [Space(10), Tooltip("The highest the character can jump (in meters)")]
    public float maxJumpLength = 3;
    [Tooltip("The lowest the character can jump (in meters)")]
    public float minJumpLength = 1.5f;
    [Tooltip("How long jump input needs to be true in order for character to achieve peak height (in seconds)")]
    public float maxJumpInputTime = 0.5f;
    [Tooltip("How long it takes for character to reach peak height (in seconds)")]
    public float jumpDuration = 0.5f;
    public bool jump;
    private bool prevJump;
    public bool appliedJumpForce;
    private float jumpInputStartTime = float.MinValue;
    public float peakHeight;

    public bool isGrounded;
    private bool prevIsGrounded;
    public bool isFalling;
    private bool prevIsFalling;

    private Coroutine currentStopperRoutine;

    void Update()
    {
        CheckGroundedness();
        ListenForJumpInput();

        SetPreviousValues();
    }

    private void SetPreviousValues()
    {
        prevJump = jump;
        prevIsGrounded = isGrounded;
        prevIsFalling = isFalling;
    }
    private void CheckGroundedness()
    {
        RaycastHit hitInfo;
        float groundDistance = float.MaxValue;
        bool hit = characterBody.transform.RaycastFromBounds(-characterBody.transform.up, out hitInfo, Vector3.down * 0.99f, true, float.MaxValue, true);
        if (hit)
            groundDistance = hitInfo.distance;
        peakHeight = Mathf.Max(peakHeight, groundDistance);

        isGrounded = groundDistance < 0.01f;
        isFalling = !isGrounded && (Vector3.Dot(characterBody.velocity.normalized, Physics.gravity.normalized) > 0);
        characterAnimator.SetBool(fallingAnimatorName, isFalling);

        bool landed = isGrounded && !prevIsGrounded;
        characterAnimator.SetBool(landAnimationName, landed);
        characterAnimator.SetFloat(peakHeightAnimationName, peakHeight);
        if (landed)
            OnCharacterLanded();

        if (!isGrounded)
            characterAnimator.SetFloat(jumpAnimatorName, 0);
    }

    private void ListenForJumpInput()
    {
        if (jump && !prevJump)
            jumpInputStartTime = Time.time;
        if ((jump && Time.time - jumpInputStartTime >= maxJumpInputTime) || (!jump && prevJump))
        {
            float percentTime = Mathf.Clamp01((Time.time - jumpInputStartTime) / maxJumpInputTime);
            float jumpAmount = MathHelpers.SetDecimalPlaces(percentTime * (maxJumpLength - minJumpLength) + minJumpLength, 1);
            Jump(jumpAmount);
        }
    }

    public void Jump(float amount)
    {
        if (isGrounded)
        {
            if (!appliedJumpForce)
            {
                characterAnimator.SetFloat(jumpAnimatorName, amount);

                SendTo(characterBody.transform.up * amount, jumpDuration);

                appliedJumpForce = true;
            }
        }
    }

    private void ResetPeakHeight()
    {
        peakHeight = 0;
    }
    private void OnCharacterLanded()
    {
        ResetPeakHeight();
        appliedJumpForce = false;
    }

    private void SendTo(Vector3 position, float time)
    {
        Vector3 velocity = characterBody.CalculateRequiredVelocityForPosition(position, time);
        Vector3 force = characterBody.CalculateRequiredForceForSpeed(velocity, Time.fixedDeltaTime, true);
        //Debug.Log("Velocity to reach position: " + velocity + " Force to reach velocity: " + force);
        characterBody.AddForce(force, ForceMode.Force);

        StopVelocityAfter(time);
    }

    private void StopVelocityAfter(float time)
    {
        if (currentStopperRoutine != null)
            StopCoroutine(currentStopperRoutine);
            
        currentStopperRoutine = StartCoroutine(CommonRoutines.WaitToDoAction((isComplete) =>
        {
            Vector3 counterVelocityForce = characterBody.CalculateRequiredForceForSpeed(Vector3.zero, Time.fixedDeltaTime, true);
            characterBody.AddForce(counterVelocityForce, ForceMode.Force);
        }, time));
    }
}
