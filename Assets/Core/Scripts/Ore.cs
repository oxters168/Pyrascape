using UnityEngine;

public class Ore : MonoBehaviour
{
    private SpriteRenderer Sprite7Up { get { if (_sprite7Up == null) _sprite7Up = GetComponentInChildren<SpriteRenderer>(); return _sprite7Up; } }
    private SpriteRenderer _sprite7Up;

    public OreData oreData;
    public WorldGenerator world;

    void Update()
    {
        // if (oreData != null)
        //     Sprite7Up.sprite = oreData.sprite;
    }
}
