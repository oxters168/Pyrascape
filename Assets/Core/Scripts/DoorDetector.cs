using UnityEngine;
using System.Collections;

public class DoorDetector : MonoBehaviour
{
    public Vector2 origin = Vector2.zero;
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
    void OnDrawGizmos()
    {
        #if UNITY_EDITOR
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.DrawWireDisc(transform.position + new Vector3(origin.x, origin.y), transform.forward, detectionRadius);
        #endif
    }
}
