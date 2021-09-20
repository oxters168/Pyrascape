using UnityEngine;
using UnityHelpers;

public class BackgroundLoop : MonoBehaviour
{
    [Tooltip("The target to follow")]
    public Transform target;
    [Space(10), Tooltip("The background layers")]
    public LoopInfo[] backgroundLoop;

    private Transform[][] backgrounds;

    void Start()
    {
        backgrounds = new Transform[backgroundLoop.Length][];
        for (int i = 0; i < backgrounds.GetLength(0); i++)
        {
            Init(i, backgroundLoop[i]);
            RefreshBackground(i, backgroundLoop[i]);
        }
    }
    void Update()
    {
        for (int i = 0; i < backgrounds.Length; i++)
            RefreshBackground(i, backgroundLoop[i]);
    }

    void Init(int loopIndex, LoopInfo backgroundLoop)
    {
        backgrounds[loopIndex] = new Transform[backgroundLoop.loopCount % 2 == 0 ? Mathf.Abs(backgroundLoop.loopCount) - 1 : Mathf.Abs(backgroundLoop.loopCount)];
        for (int i = 0; i < backgrounds[loopIndex].Length; i++)
        {
            GameObject bgObj = new GameObject("Image " + i);
            var sprRndr = bgObj.AddComponent<SpriteRenderer>();
            sprRndr.sprite = backgroundLoop.image;
            sprRndr.sortingOrder = backgroundLoop.layerOrder;
            bgObj.transform.SetParent(transform);
            backgrounds[loopIndex][i] = bgObj.transform;
        }
    }
    void RefreshBackground(int loopIndex, LoopInfo backgroundLoop)
    {
        if (backgrounds[loopIndex].Length > 0)
        {
            Bounds bgBounds = backgrounds[loopIndex][0].GetTotalBounds(Space.World);;
            float scale = 0.076923f * backgroundLoop.distance + 1; //scale = 0.076923z + 1

            float xOffset = 0;
            if (target != null)
                xOffset = Mathf.RoundToInt((target.position.x - 0) / bgBounds.size.x) * bgBounds.size.x;

            int total = backgrounds[loopIndex].Length;
            int symmetricalHalf = (total - 1) / 2;

            backgrounds[loopIndex][0].localPosition = new Vector3(xOffset, 0, backgroundLoop.distance);
            backgrounds[loopIndex][0].localScale = Vector3.one * scale;

            for (int i = 1; i <= symmetricalHalf; i++)
            {
                int leftIndex = i;
                int rightIndex = total - i;
                
                backgrounds[loopIndex][leftIndex].localPosition = new Vector3(xOffset + bgBounds.size.x * i, 0, backgroundLoop.distance);
                backgrounds[loopIndex][leftIndex].localScale = Vector3.one * scale;
                backgrounds[loopIndex][rightIndex].localPosition = new Vector3(xOffset - bgBounds.size.x * i, 0, backgroundLoop.distance);
                backgrounds[loopIndex][rightIndex].localScale = Vector3.one * scale;
                
            }
        }
    }
}
