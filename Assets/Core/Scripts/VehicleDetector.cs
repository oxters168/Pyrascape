using UnityEngine;
using System.Collections;

public class VehicleDetector : MonoBehaviour
{
    public float detectionRadius = 1;
    public LayerMask vehiclesMask = ~0;
    public float castDelay = 0.4f;

    [Space(10)]
    public Transform vehicle;

    void Update()
    {
        StartCoroutine(Cast());
    }

    IEnumerator Cast()
    {
        var castHit = Physics2D.CircleCast(transform.position, detectionRadius, Vector2.up, 0, vehiclesMask);
        if (castHit)
            vehicle = castHit.transform;
        else
            vehicle = null;

        yield return new WaitForSeconds(castDelay);
    }
}
