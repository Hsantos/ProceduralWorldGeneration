using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    [SerializeField]
    private Noise.NormalizeMode normalizeMode;
    public Noise.NormalizeMode NormalizeMode => normalizeMode;
    
    [SerializeField]
    private float scale = 50f;
    public float Scale => scale;
    
    [SerializeField]
    private int octaves = 6;
    public int Octaves => octaves;

    [Range(0,1)]
    [SerializeField]
    private float persistence = 0.6f;
    public float Persistence => persistence;

    [SerializeField]
    private float lacunarity = 2f;
    public float Lacunarity => lacunarity;

    [SerializeField]
    private int seed = 1;
    public int Seed => seed;
        
    [SerializeField]
    private Vector2 offset = Vector2.zero;
    public Vector2 Offset => offset;

    public void ValidateValues()
    {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1f);
        persistence = Mathf.Clamp01(persistence);
    }
}
