using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColourMap,
        Mesh,
        FalloffMap
    }

    [SerializeField]
    private DrawMode drawMode = DrawMode.NoiseMap;

    public Noise.NormalizeMode normalizeMode;

    public const int MAP_CHUNK_SIZE = 239;

    [Range(0,6)]
    [SerializeField]
    private int editorPreviewLOD;

    [SerializeField]
    private float noiseScale = 0.3f;

    [SerializeField]
    private int octaves = 4;

    [Range(0,1)]
    [SerializeField]
    private float persistence = 0.5f;

    [SerializeField]
    private float lacunarity = 2f;

    [SerializeField]
    private int seed = 1;

    [SerializeField]
    private Vector2 offset = Vector2.zero;

    [SerializeField]
    private bool useFalloffMap;

    [SerializeField]
    private float meshHeightMultiplier = 1;

    [SerializeField]
    private AnimationCurve meshHeightCurve;

    [SerializeField]
    private bool autoUpdate = true;
    public bool AutoUpdate => autoUpdate;

    public TerrainType[] regions;

    private float[,] falloffMap;
    
    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    private void OnValidate()
    {
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
        if(meshHeightMultiplier< 1) meshHeightMultiplier = 1;
        
        CreateFalloffMap();
    }

    private void Awake()
    {
        CreateFalloffMap();
    }

    private void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        
        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    private void CreateFalloffMap()
    {
        falloffMap = FallOffGenerator.GenerateFalloffMap(MAP_CHUNK_SIZE);
    }
    
    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();

        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
                break;
            
            case DrawMode.ColourMap:
                display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, MAP_CHUNK_SIZE, MAP_CHUNK_SIZE));
                break;
            
            case DrawMode.Mesh:
                display.DrawMesh(
                    MeshGenerator.GenerateTerrainMesh
                    (
                        mapData.heightMap, 
                        meshHeightMultiplier, 
                        meshHeightCurve, 
                        editorPreviewLOD
                    ),
                    TextureGenerator.TextureFromColourMap(mapData.colourMap, MAP_CHUNK_SIZE, MAP_CHUNK_SIZE));
                break;
            
            case DrawMode.FalloffMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FallOffGenerator.GenerateFalloffMap(MAP_CHUNK_SIZE)));
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };
        
        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }
    
    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap
            (
                MAP_CHUNK_SIZE + 2, 
                MAP_CHUNK_SIZE + 2, 
                seed, 
                noiseScale, 
                octaves, 
                persistence, 
                lacunarity, 
                center + offset,
                normalizeMode
            );

        
        Color[] colourMap = new Color[MAP_CHUNK_SIZE * MAP_CHUNK_SIZE];
        for (int y = 0; y < MAP_CHUNK_SIZE; y++)
        {
            for (int x = 0; x < MAP_CHUNK_SIZE; x++)
            {
                if (useFalloffMap)
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                        colourMap[y * MAP_CHUNK_SIZE + x] = regions[i].colour;
                    else
                        break;
                }
            }
        }
        
        return new MapData(noiseMap, colourMap);
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
