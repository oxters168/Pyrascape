using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Pyrascape/NoiseInfo", order = 5)]
public class NoiseInfo : ScriptableObject
{
    public FastNoise.NoiseType noiseType = FastNoise.NoiseType.Perlin;
    public bool invert;
    public float threshold = 0.38f;
    public float frequency = 0.38f;
    public int octaves = 3;
    public float power = 1.13f;
}
