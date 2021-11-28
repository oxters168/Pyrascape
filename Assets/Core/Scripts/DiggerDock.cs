using UnityEngine;
using UnityHelpers;

public class DiggerDock : MonoBehaviour
{
    public bool diggerDocked;
    [Tooltip("The speed limit the digger needs to be abiding by in order to dock")]
    public float maxSpeed = 1;

    [Space(10)]
    public Vector2 origin = Vector2.zero;
    public float radius = 0.3f;
    public LayerMask diggerMask = ~0;

    void Update()
    {
        var circleHit = Physics2D.CircleCast(transform.position.xy() + origin, radius, Vector2.up, 0, diggerMask);
        if (circleHit && circleHit.transform.GetComponent<Digger>())
        {
            var pod = circleHit.transform.GetComponent<PodPhysics2D>();
            var podVelocity = pod.PodBody.velocity;
            diggerDocked = pod.fly > -float.Epsilon && pod.fly < float.Epsilon
                && pod.horizontal > -float.Epsilon && pod.horizontal < float.Epsilon
                && podVelocity.x > -maxSpeed && podVelocity.x < maxSpeed
                && podVelocity.y > -maxSpeed && podVelocity.y < maxSpeed;
        }
        else
            diggerDocked = false;
    }
    void OnDrawGizmos()
    {
        #if UNITY_EDITOR
        UnityEditor.Handles.color = Color.green;
        UnityEditor.Handles.DrawWireDisc(transform.position + new Vector3(origin.x, origin.y), transform.forward, radius);
        #endif
    }
}
