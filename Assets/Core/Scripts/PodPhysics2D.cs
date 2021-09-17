using UnityEngine;
using UnityHelpers;

public class PodPhysics2D : MonoBehaviour
{
    [Range(-1f, 1f)]
    public float horizontal;
    [Range(0, 1f)]
    public float fly;

    [Space(10), Tooltip("The speed of the speed (in m/s^2)")]
    public float acceleration = 0.1f;
    [Tooltip("The fastest speed the vehicle can achieve (in m/s)")]
    public float maxSpeed = 5;
    [Tooltip("The speed of the fly speed (in m/s^2)")]
    public float flyAcceleration = 0.04f;
    [Tooltip("The fastest the vehicle can go up")]
    public float maxFlySpeed = 3;

    private Rigidbody2D _podBody;
    public Rigidbody2D PodBody { get { if (_podBody == null) _podBody = GetComponentInChildren<Rigidbody2D>(); return _podBody; } }
    private SpriteRenderer Sprite7Up { get { if (_sprite7Up == null) _sprite7Up = GetComponentInChildren<SpriteRenderer>(); return _sprite7Up; } }
    private SpriteRenderer _sprite7Up;


    [Space(10), Tooltip("The minimum distance the pod keeps itself floating above the ground (in meters)")]
    public float minGroundDistance = 1;

    [Space(10), Tooltip("The layer(s) to be raycasted when looking for the ground")]
    public LayerMask groundMask = ~0;

    [Tooltip("How much distance beyond the minimum ground height before anti-gravity wears off")]
    public float antigravityFalloffDistance = 20;
    public AnimationCurve antigravityFalloffCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Tooltip("How high from the ground the pod can fly")]
    public float flyHeightDistance = 5;
    public AnimationCurve flyHeightCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Space(10), Tooltip("The constant applied to the proportional part of the floatation pd controller")]
    public float Kp = 200;
    [Tooltip("The constant applied to the derivative part of the floatation pd controller")]
    public float Kd = 8000;
    [Space(10)]
    public float proTerm = 0.0002f;
    public float devTerm = 0.002f;

    /// <summary>
    /// The position the pod would be if left motionless
    /// </summary>
    private Vector2 groundedPosition;
    /// <summary>
    /// The bounds of the vehicle calculated every fixed frame
    /// </summary>
    private Bounds vehicleBounds;
    /// <summary>
    /// Used in the pd controller of the floatation
    /// </summary>
    private float prevErr;
    private bool facingLeft;

    void Update()
    {
        if (PodBody.velocity.x < 0 && horizontal < 0)
            facingLeft = true;
        else if (PodBody.velocity.x > 0 && horizontal > 0)
            facingLeft = false;

        Sprite7Up.flipX = facingLeft;
    }
    void FixedUpdate()
    {
        vehicleBounds = transform.GetTotalBounds(Space.World);

        ApplyFloatation();
        ApplyOrientator();

        ApplyInputStrafe();
        ApplyInputFly();
    }

    private void ApplyInputFly()
    {
        if (Mathf.Abs(fly) > float.Epsilon)
        {
            float currentVelocity = Vector2.Dot(PodBody.velocity, Vector2.up);
            float nextVelocity = Mathf.Clamp(currentVelocity + flyAcceleration * fly, -maxFlySpeed, maxFlySpeed);
            float flyForce = Vector2.Dot(PodBody.CalculateRequiredForceForSpeed(Vector2.up * nextVelocity), Vector2.up);
            float antiMomentum = PodBody.CalculateRequiredForceForSpeed(Vector2.zero).y;
            Vector2 antigravityForce = PodBody.CalculateAntiGravityForce();

            RaycastHit2D hitInfo = Physics2D.Raycast(vehicleBounds.center, -Vector2.up, float.MaxValue, groundMask);
            float groundDistance = float.MaxValue;
            if (hitInfo)
                groundDistance = hitInfo.distance;

            float heightCoefficient = Mathf.Clamp01(flyHeightCurve.Evaluate(Mathf.Clamp01(groundDistance / flyHeightDistance)));
            PodBody.AddForce(Vector2.up * (antigravityForce.y + Mathf.Lerp(antiMomentum, flyForce, heightCoefficient)), ForceMode2D.Force);
        }
    }
    private void ApplyInputStrafe()
    {
        if (Mathf.Abs(horizontal) > float.Epsilon)
        {
            float currentSpeed = Vector2.Dot(PodBody.velocity, Vector2.right);

            float inputAcceleration = Mathf.Clamp(horizontal, -1, 1) * acceleration;
            float desiredSpeed = Mathf.Clamp(currentSpeed + inputAcceleration, -maxSpeed, maxSpeed);
            Vector2 appliedForce = PhysicsHelpers.CalculateRequiredForceForSpeed(PodBody.mass, currentSpeed, desiredSpeed) * Vector2.right;
            PodBody.AddForce(appliedForce, ForceMode2D.Force);
        }
    }
    private void ApplyOrientator()
    {
        PodBody.AddTorque(PodBody.CalculateRequiredTorqueForRotation(0, proTerm, devTerm, 0.02f), ForceMode2D.Force);
    }
    private void ApplyFloatation()
    {
        float expectedFloatingForce = CalculateFloatingForce();
            
        if (Mathf.Abs(fly) > float.Epsilon)
            expectedFloatingForce = 0;

        PodBody.AddForce(Vector2.up * expectedFloatingForce, ForceMode2D.Force);
    }
    private float CalculateFloatingForce()
    {
        float vehicleSizeOnUpAxis = Mathf.Abs(Vector2.Dot(vehicleBounds.extents, Vector2.up));

        float groundCastDistance = vehicleSizeOnUpAxis + minGroundDistance * 5;
        float groundDistance = float.MaxValue;
        RaycastHit2D hitInfo = Physics2D.Raycast(vehicleBounds.center, -Vector2.up, groundCastDistance, groundMask);
        if (hitInfo)
            groundDistance = hitInfo.distance - vehicleSizeOnUpAxis;

        float groundOffset = minGroundDistance - groundDistance;

        float antigravityMultiplier = 1;
        if (groundOffset < -float.Epsilon)
            antigravityMultiplier = antigravityFalloffCurve.Evaluate(Mathf.Max(antigravityFalloffDistance - Mathf.Abs(groundOffset), 0) / antigravityFalloffDistance);
        float antigravityForce = PodBody.CalculateAntiGravityForce().y * antigravityMultiplier;

        float floatingForce = 0;
        if (groundDistance < float.MaxValue) //If the ground is within range
        {
            //Thanks to @bmabsout for a much better and more stable floatation method
            //based on pid but just the p and the d
            groundedPosition = hitInfo.point + Vector2.up * minGroundDistance;
            float err = groundedPosition.y - PodBody.position.y;
            float proportional = Kp * err;
            float derivative = Kd * (err - prevErr);
            floatingForce = proportional + derivative;
            prevErr = err;
        }

        return antigravityForce + floatingForce;
    }
}
