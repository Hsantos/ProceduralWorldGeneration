using UnityEngine;

public static class Noise
{
    public enum NormalizeMode
    {
        Local,
        Global
    }
    
    /// <summary>
    /// Generates the Perlin Noise Map
    /// https://en.wikipedia.org/wiki/Perlin_noise
    /// </summary>
    /// <param name="mapWidth">The Width of the map</param>
    /// <param name="mapHeight">The Height of the map</param>
    /// <param name="seed"></param>
    /// <param name="scale">The Scale of the noise</param>
    /// <param name="octaves"></param>
    /// <param name="persistence"></param>
    /// <param name="lacunarity"></param>
    /// <param name="offset">Used to scroll through the noise</param>
    /// <returns>The generated Perlin Noise map</returns>
    public static float[,] GenerateNoiseMap
    (
        int mapWidth, 
        int mapHeight, 
        int seed, 
        float scale, 
        int octaves, 
        float persistence, 
        float lacunarity, 
        Vector2 offset, 
        NormalizeMode normalizeMode
    )
    {
        float [,] noiseMap = new float[mapWidth,mapHeight];
        
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float amplitude = 1;
        float frequency = 1;
        
        float maxPossibleHeight = 0;
        

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }

        if (scale <= 0)
            scale = 0.0001f;

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;
        
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;
                
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight)
                    maxLocalNoiseHeight = noiseHeight;
                if (noiseHeight < minLocalNoiseHeight)
                    minLocalNoiseHeight = noiseHeight;
                
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if(normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / maxPossibleHeight;
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
                    
            }
        }
        
        return noiseMap;
    }
}
