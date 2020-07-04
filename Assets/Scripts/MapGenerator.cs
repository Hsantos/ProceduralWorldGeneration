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
        Mesh
    }

    [SerializeField]
    private DrawMode drawMode = DrawMode.NoiseMap;

    public Noise.NormalizeMode normalizeMode;

    public const int MapChunkSize = 241;

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
    private float meshHeightMultiplier = 1;

    [SerializeField]
    private AnimationCurve meshHeightCurve;

    [SerializeField]
    private bool autoUpdate = true;
    public bool AutoUpdate => autoUpdate;

    public TerrainType[] regions;
    
    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    private void OnValidate()
    {
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
        if(meshHeightMultiplier< 1) meshHeightMultiplier = 1;
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
    
    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if(drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, MapChunkSize, MapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(
                MeshGenerator.GenerateTerrainMesh
                (
                    mapData.heightMap, 
                    meshHeightMultiplier, 
                    meshHeightCurve, 
                    editorPreviewLOD
                ),
                TextureGenerator.TextureFromColourMap(mapData.colourMap, MapChunkSize, MapChunkSize));
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
                MapChunkSize, 
                MapChunkSize, 
                seed, 
                noiseScale, 
                octaves, 
                persistence, 
                lacunarity, 
                center + offset,
                normalizeMode
            );

        
        Color[] colourMap = new Color[MapChunkSize * MapChunkSize];
        for (int y = 0; y < MapChunkSize; y++)
        {
            for (int x = 0; x < MapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                        colourMap[y * MapChunkSize + x] = regions[i].colour;
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
