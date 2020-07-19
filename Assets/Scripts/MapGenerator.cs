using System;
using System.Collections.Generic;
using System.Threading;
using ChannelThree.ProcedutalWorld.Data;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        Mesh,
        FalloffMap
    }

    [SerializeField]
    private TerrainDataScriptableObject terrainData;
    public TerrainDataScriptableObject TerrainData => terrainData;

    [SerializeField]
    private NoiseDataScriptableObject noiseData;

    [SerializeField]
    private TextureDataScriptableObject textureData;
    
    [SerializeField]
    private Material terrainMaterial;

    [SerializeField]
    private DrawMode drawMode = DrawMode.NoiseMap;

    [Range(0,6)]
    [SerializeField]
    private int editorPreviewLOD;
    
    [SerializeField]
    private bool autoUpdate = true;
    public bool AutoUpdate => autoUpdate;

    private float[,] falloffMap;
    
    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public int mapChunkSize
    {
        get
        {
            if (terrainData.UseFlatShading)
                return 95;
            else
                return 239;
        }
    }

    private void OnValidate()
    {
        if (terrainData != null)
        {
            terrainData.OnValuesUpdated -= OnValuesUpdate;
            terrainData.OnValuesUpdated += OnValuesUpdate;
        }

        if (noiseData != null)
        {
            noiseData.OnValuesUpdated -= OnValuesUpdate;
            noiseData.OnValuesUpdated += OnValuesUpdate;
        }

        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    private void OnValuesUpdate()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    private void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
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

        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
                break;
            
            case DrawMode.Mesh:
                display.DrawMesh(
                    MeshGenerator.GenerateTerrainMesh
                    (
                        mapData.heightMap, 
                        TerrainData.MeshHeightMultiplier, 
                        TerrainData.MeshHeightCurve, 
                        editorPreviewLOD,
                        TerrainData.UseFlatShading
                    ));
                break;
            
            case DrawMode.FalloffMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FallOffGenerator.GenerateFalloffMap(mapChunkSize)));
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
        MeshData meshData = 
            MeshGenerator.GenerateTerrainMesh
                (
                    mapData.heightMap, 
                    TerrainData.MeshHeightMultiplier, 
                    TerrainData.MeshHeightCurve,
                    lod, 
                    TerrainData.UseFlatShading
                );
        
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap
            (
            mapChunkSize + 2, 
            mapChunkSize + 2, 
                noiseData.Seed, 
                noiseData.NoiseScale, 
                noiseData.Octaves, 
                noiseData.Persistence, 
                noiseData.Lacunarity, 
                center + noiseData.Offset,
                noiseData.NormalizeMode
            );

        if (terrainData.UseFalloffMap)
        {
            if (falloffMap == null)
            {
                falloffMap = FallOffGenerator.GenerateFalloffMap(mapChunkSize + 2);
            }
            
            for (int y = 0; y < mapChunkSize + 2; y++)
            {
                for (int x = 0; x < mapChunkSize + 2; x++)
                {
                    if (TerrainData.UseFalloffMap)
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                }
            } 
        }
        
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.MinHeight, terrainData.MaxHeight);
        
        return new MapData(noiseMap);
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
