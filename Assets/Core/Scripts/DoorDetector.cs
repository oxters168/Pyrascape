using UnityEngine;
using System.Collections;

public class DoorDetector : MonoBehaviour
{
    public float detectionRadius = 1;
    public LayerMask doorMask = ~0;
    public float castDelay = 0.4f;

    [Space(10)]
    public Transform door;

    void Update()
    {
        StartCoroutine(Cast());
    }

    IEnumerator Cast()
    {
        var castHit = Physics2D.CircleCast(transform.position, detectionRadius, Vector2.up, 0, doorMask);
        if (castHit)
            door = castHit.transform;
        else
            door = null;

        yield return new WaitForSeconds(castDelay);
    }
}
