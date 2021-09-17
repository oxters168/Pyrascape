using UnityEngine;
using UnityEngine.Tilemaps;

public class Digger : MonoBehaviour
{
    private PodPhysics2D _pod;
    private PodPhysics2D Pod { get { if (_pod == null) _pod = GetComponentInChildren<PodPhysics2D>(); return _pod; } }
    private Tilemap physical, foreground;

    private Vector3Int currentCell;

    void Start()
    {
        FindTilemaps();
        GetComponentInChildren<PodPhysics2D>();
    }

    void FixedUpdate()
    {
        currentCell = physical.WorldToCell(Pod.PodBody.position);
        bool leftCell = physical.HasTile(currentCell + Vector3Int.left);
        bool rightCell = physical.HasTile(currentCell + Vector3Int.right);
        bool botCell = physical.HasTile(currentCell + Vector3Int.down);
        if (botCell && Pod.fly < -float.Epsilon)
        {
            physical.SetTile(currentCell + Vector3Int.down, null);
        }
        else if (botCell)
        {
            if (leftCell && Pod.horizontal < -float.Epsilon)
            {
                physical.SetTile(currentCell + Vector3Int.left, null);
            }
            else if (rightCell && Pod.horizontal > float.Epsilon)
            {
                physical.SetTile(currentCell + Vector3Int.right, null);
            }
        }
    }

    private void FindTilemaps()
    {
        Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
        for (int i = 0; i < tilemaps.Length; i++)
        {
            var tileMapRenderer = tilemaps[i].GetComponentInChildren<TilemapRenderer>();
            if (tileMapRenderer != null)
            {
                if (tileMapRenderer.sortingOrder == 0)
                    physical = tilemaps[i];
                else if (tileMapRenderer.sortingOrder == 1)
                    foreground = tilemaps[i];
            }
        }
    }
}
