using UnityEngine;
using System.Collections.Generic;

public class BackgroundLoop : MonoBehaviour
{
    public const int MAX_PARALLAX_LAYERS = 4;
    //Background infos should be a given from a global thing that gives what biome is at what position (biomes have their backgrounds)
    //Then we have a list of targets which are used to retrieve the backgrounds and place them in their proper positions
    private List<Transform> targets = new List<Transform>();
    private List<Background>[] allBackgrounds = new List<Background>[MAX_PARALLAX_LAYERS];

    void Update()
    {
        for (int j = 0; j < allBackgrounds.Length; j++)
        {
            if (allBackgrounds[j] == null)
                allBackgrounds[j] = new List<Background>();
            var backgrounds = allBackgrounds[j];

            //Calculate how many backgrounds will exist based on targets' positions
            List<int> bgIndices = new List<int>();
            foreach (var target in targets)
            {
                int index = Mathf.RoundToInt((target.position.x - 0) / Background.BACKGROUND_BASE_WIDTH);
                if (!bgIndices.Contains(index))
                    bgIndices.Add(index);
                if (!bgIndices.Contains(index - 1))
                    bgIndices.Add(index - 1);
                if (!bgIndices.Contains(index + 1))
                    bgIndices.Add(index + 1);
            }

            //Adjust backgrounds array to fit given count
            int bgDiff = bgIndices.Count - backgrounds.Count;
            for (int i = 0; i < Mathf.Abs(bgDiff); i++)
            {
                if (bgDiff < 0)
                {
                    backgrounds[0].Destroy();
                    backgrounds.RemoveAt(0);
                }
                else
                    backgrounds.Add(new Background(transform, j));
            }

            //Refresh backgrounds
            for (int i = 0; i < backgrounds.Count; i++)
            {
                backgrounds[i].posIndex = bgIndices[i];
                backgrounds[i].RefreshBackground();
            }
        }
    }

    public void AddTarget(Transform target)
    {
        if (!targets.Contains(target))
            targets.Add(target);
        else
            Debug.LogError("Cannot add a target that is already in the background targets collection");
    }
    public void RemoveTarget(Transform target)
    {
        if (targets.Contains(target))
            targets.Remove(target);
        else
            Debug.LogError("Cannot remove a target that is not in the background targets collection");
    }

    public class Background
    {
        private Biomes BiomeData { get { if (_biomeData == null) _biomeData = FindObjectOfType<Biomes>(); return _biomeData; } }
        private Biomes _biomeData;
        
        public const float DEPTH_SCALE = 0.076923f;
        public const float BACKGROUND_BASE_WIDTH = 30;//39.23076f; //Makes it easier if every background is same size (in unity units, so meters)
        public const string BACKGROUND_LAYER = "Background";

        public int posIndex;
        private int parallaxIndex;
        private Transform backgroundsParent;
        private Transform currentImageObj;
        // private Transform[] parallaxTransforms;
        private BackgroundInfo bgInfo;

        public Background(Transform backgroundsParent, int parallaxIndex)
        {
            this.parallaxIndex = parallaxIndex;
            this.backgroundsParent = backgroundsParent;
            // loopParent = new GameObject("Parallax_Layer_" + parallaxIndex).transform;
            // loopParent.SetParent(backgroundsParent);
        }
        public static float CalculateScale(float distance)
        {
            return DEPTH_SCALE * distance + 1; //scale = 0.076923z + 1
        }
        private static GameObject CreateImage(Transform loopParent, LoopInfo backgroundLoop, float distance)
        {
            GameObject bgObj = new GameObject("BGImage");
            bgObj.layer = LayerMask.NameToLayer(BACKGROUND_LAYER);
            var sprRndr = bgObj.AddComponent<SpriteRenderer>();
            sprRndr.sprite = backgroundLoop.image;
            sprRndr.sortingOrder = backgroundLoop.layerOrder;
            
            float scale = CalculateScale(distance);
            bgObj.transform.SetParent(loopParent);
            // bgObj.transform.localPosition = new Vector3(0, backgroundLoop.relativeHeight, distance);
            bgObj.transform.localScale = Vector3.one * scale;
            return bgObj;
        }
        public void RefreshBackground()
        {
            //Retrieve background info from position index and background width
            BackgroundInfo newBackgroundInfo = BiomeData.GetBackgroundInfoAt(new Vector3(posIndex * BACKGROUND_BASE_WIDTH, 0, 0));

            float distance = BackgroundInfo.DISTANCE[parallaxIndex];
            var backgroundLoop = newBackgroundInfo.layers[parallaxIndex];
            if (bgInfo != newBackgroundInfo)
            {
                //Destroy current parallaxes
                // if (parallaxTransforms != null)
                //     Destroy();
                Destroy();

                bgInfo = newBackgroundInfo;
                
                //Create new parallaxes
                // parallaxTransforms = new Transform[bgInfo.layers.Length];
                // for (int i = 0; i < 4; i++)
                //     if (bgInfo.layers[i] != null)
                //         parallaxTransforms[i] = CreateImage(loopParent, bgInfo.layers[i], BackgroundInfo.DISTANCE[i]).transform;
                currentImageObj = CreateImage(backgroundsParent, backgroundLoop, distance).transform;
            }

            //Move the parent to the proper x and y
            currentImageObj.localPosition = new Vector3(posIndex * BACKGROUND_BASE_WIDTH * CalculateScale(distance), bgInfo.height + backgroundLoop.relativeHeight, distance);
        }

        public void Destroy()
        {
            bgInfo = null;
            if (currentImageObj != null)
                GameObject.Destroy(currentImageObj.gameObject);

            // for (int i = 0; i < parallaxTransforms.Length; i++)
            //     GameObject.Destroy(parallaxTransforms[i].gameObject);

            // parallaxTransforms = null;
        }
    }
}
