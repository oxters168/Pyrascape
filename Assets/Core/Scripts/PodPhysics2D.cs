using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityHelpers;

public class PodPhysics2D : MonoBehaviour
{
    private Rigidbody2D _podBody;
    private Rigidbody2D PodBody { get { if (_podBody == null) _podBody = GetComponentInChildren<Rigidbody2D>(); return _podBody; } }

    [Space(10), Tooltip("The minimum distance the pod keeps itself floating above the ground (in meters)")]
    public float minGroundDistance = 1;

    [Space(10), Tooltip("The layer(s) to be raycasted when looking for the ground")]
    public LayerMask groundMask = ~0;

    [Tooltip("How much distance beyond the minimum ground height before anti-gravity wears off")]
    public float antigravityFalloffDistance = 20;
    public AnimationCurve antigravityFalloffCurve = AnimationCurve.Linear(0, 0, 1, 1);

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
    private Vector2 prevErr;

    void FixedUpdate()
    {
        vehicleBounds = transform.GetTotalBounds(Space.World);

        ApplyFloatation();

        PodBody.AddTorque(PodBody.CalculateRequiredTorqueForRotation(0, proTerm, devTerm, 0.02f), ForceMode2D.Force);
    }

    private void ApplyFloatation()
    {
        Vector3 expectedFloatingForce = CalculateFloatingForce();
            
        // if (Mathf.Abs(fly) > float.Epsilon)
        //     expectedFloatingForce = Vector3.zero;

        PodBody.AddForce(expectedFloatingForce, ForceMode2D.Force);
    }
    private Vector2 CalculateFloatingForce()
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
        Vector2 antigravityForce = PodBody.CalculateAntiGravityForce() * antigravityMultiplier;

        Vector2 floatingForce = Vector2.zero;
        if (groundDistance < float.MaxValue) //If the ground is within range
        {
            //Thanks to @bmabsout for a much better and more stable floatation method
            //based on pid but just the p and the d
            groundedPosition = hitInfo.point + Vector2.up * minGroundDistance;
            Vector2 err = Vector2.up * Vector2.Dot(Vector2.up, groundedPosition - PodBody.position);
            Vector2 proportional = Kp * err;
            Vector2 derivative = Kd * (err - prevErr);
            floatingForce = proportional + derivative;
            prevErr = err;
        }

        return antigravityForce + floatingForce;
    }
}
