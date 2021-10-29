using UnityEngine;
using UnityHelpers;

public class BackgroundLoop : MonoBehaviour
{
    [Tooltip("The target to follow")]
    public Transform target;
    [Space(10)]
    public BackgroundInfo background;

    private Transform[][] backgrounds;

    void Start()
    {
        transform.position = new Vector3(0, background.height, 0);
        backgrounds = new Transform[background.layers.Length][];
        for (int bgIndex = 0; bgIndex < backgrounds.GetLength(0); bgIndex++)
        {
            Init(bgIndex, background.layers[bgIndex]);
            RefreshBackground(bgIndex, background.layers[bgIndex]);
        }
    }
    void Update()
    {
        for (int bgIndex = 0; bgIndex < backgrounds.Length; bgIndex++)
            RefreshBackground(bgIndex, background.layers[bgIndex]);
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

            backgrounds[loopIndex][0].localPosition = new Vector3(xOffset, backgroundLoop.relativeHeight, backgroundLoop.distance);
            backgrounds[loopIndex][0].localScale = Vector3.one * scale;

            for (int i = 1; i <= symmetricalHalf; i++)
            {
                int leftIndex = i;
                int rightIndex = total - i;
                
                backgrounds[loopIndex][leftIndex].localPosition = new Vector3(xOffset + bgBounds.size.x * i, backgroundLoop.relativeHeight, backgroundLoop.distance);
                backgrounds[loopIndex][leftIndex].localScale = Vector3.one * scale;
                backgrounds[loopIndex][rightIndex].localPosition = new Vector3(xOffset - bgBounds.size.x * i, backgroundLoop.relativeHeight, backgroundLoop.distance);
                backgrounds[loopIndex][rightIndex].localScale = Vector3.one * scale;
                
            }
        }
    }
}
