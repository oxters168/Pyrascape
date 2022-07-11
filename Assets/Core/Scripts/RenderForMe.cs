using UnityEngine;

public class RenderForMe : MonoBehaviour
{
    [Tooltip("How many tiles should the world generator make visible around you at once")]
    public Vector2Int renderSize = new Vector2Int(4, 4);
    private Vector2Int prevRenderSize;

    [Space(10), Tooltip("If set to on will tell the world generator to generate around you")]
    public bool renderTerrain = true;
    [Tooltip("If set to true will tell background loop to follow you")]
    public bool renderBackground = true;

    private WorldGenerator terrain;
    private BackgroundLoop backgroundLoop;

    void Start()
    {
        terrain = FindObjectOfType<WorldGenerator>();
        backgroundLoop = FindObjectOfType<BackgroundLoop>();
        ApplySelf();
    }
    void Update()
    {
        ApplySelf();
    }
    void OnDestroy()
    {
        // Debug.Log("Destroying " + transform.name);
        if (terrain != null && terrain.HasTarget(transform))
            terrain.RemoveTarget(transform);
        if (terrain != null && backgroundLoop.HasTarget(transform))
            backgroundLoop.RemoveTarget(transform);
    }

    private void ApplySelf()
    {
        if (renderTerrain && renderSize != prevRenderSize)
        {
            terrain.AddOrSetTarget(transform, renderSize);
            prevRenderSize = renderSize;
        }
        else if (!renderTerrain && terrain.HasTarget(transform))
        {
            terrain.RemoveTarget(transform);
        }

        if (renderBackground && !backgroundLoop.HasTarget(transform))
            backgroundLoop.AddTarget(transform);
        else if (!renderBackground && backgroundLoop.HasTarget(transform))
            backgroundLoop.RemoveTarget(transform);
    }
}
