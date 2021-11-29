using UnityEngine;

public class Biomes : MonoBehaviour
{
    public BiomeInfo[] allBiomes;

    public BackgroundInfo GetBackgroundInfoAt(Vector3 position)
    {
        return allBiomes[0].background; //Will change later when we have more biomes
    }
}
