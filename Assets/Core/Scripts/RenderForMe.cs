using UnityEngine;

public class RenderForMe : MonoBehaviour
{
    public WorldGenerator Terrain { get { if (_terrain == null) _terrain = FindObjectOfType<WorldGenerator>(); return _terrain; } }
    private WorldGenerator _terrain;
    public BackgroundLoop TheBackgroundLoop { get { if (_backgroundLoop == null) _backgroundLoop = FindObjectOfType<BackgroundLoop>(); return _backgroundLoop; } }
    private BackgroundLoop _backgroundLoop;

    [Tooltip("How many tiles should the world generator make visible around you at once")]
    public Vector2Int renderSize = new Vector2Int(4, 4);
    private Vector2Int prevRenderSize = Vector2Int.zero;

    [Space(10), SerializeField, Tooltip("If set to on will tell the world generator to generate around you")]
    private bool renderTerrain = true;
    [SerializeField, Tooltip("If set to true will tell background loop to follow you")]
    private bool renderBackground = true;

    void OnEnable()
    {
        ApplySelf();
    }
    void Update()
    {
        ApplySelf();
    }
    void OnDisable()
    {
        ApplySelf();
    }
    void OnDestroy()
    {
        if (Terrain != null && Terrain.HasTarget(transform))
            Terrain.RemoveTarget(transform);
        if (TheBackgroundLoop != null && TheBackgroundLoop.HasTarget(transform))
            TheBackgroundLoop.RemoveTarget(transform);
    }

    public void SetRenderTerrain(bool isOn)
    {
        renderTerrain = isOn;
        ApplySelf();
    }
    public void SetRenderBackground(bool isOn)
    {
        renderBackground = isOn;
        ApplySelf();
    }

    private void ApplySelf()
    {
        if (Terrain != null)
        {
            if (gameObject.activeInHierarchy && renderTerrain && (!Terrain.HasTarget(transform) || renderSize != prevRenderSize))
            {
                // Debug.Log($"Adding or setting {transform.name} in terrain to {renderSize}");
                Terrain.AddOrSetTarget(transform, renderSize);
                prevRenderSize = renderSize;
            }
            else if ((!gameObject.activeInHierarchy || !renderTerrain) && Terrain.HasTarget(transform))
            {
                // Debug.Log($"Removing {transform.name} from terrain");
                Terrain.RemoveTarget(transform);
            }
        }

        if (TheBackgroundLoop != null)
        {
            // Debug.Log($"Does background loop have {transform}? {TheBackgroundLoop.HasTarget(transform)}");
            if (gameObject.activeInHierarchy && renderBackground && !TheBackgroundLoop.HasTarget(transform))
                TheBackgroundLoop.AddTarget(transform);
            else if ((!gameObject.activeInHierarchy || !renderBackground) && TheBackgroundLoop.HasTarget(transform))
                TheBackgroundLoop.RemoveTarget(transform);
        }
    }
}
