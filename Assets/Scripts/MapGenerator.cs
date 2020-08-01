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
    private MeshSettings meshSettings;
    public MeshSettings MeshSettings => meshSettings;

    [SerializeField]
    private HeightMapSettings heightMapSettings;

    [SerializeField]
    private TextureDataScriptableObject textureData;
    
    [SerializeField]
    private Material terrainMaterial;

    [SerializeField]
    private DrawMode drawMode = DrawMode.NoiseMap;

    [Range(0,MeshSettings.numSupportedLODs - 1)]
    [SerializeField]
    private int editorPreviewLOD;
    
    [SerializeField]
    private bool autoUpdate = true;
    public bool AutoUpdate => autoUpdate;

    private float[,] falloffMap;
    
    Queue<MapThreadInfo<HeightMap>> heightMapThreadInfoQueue = new Queue<MapThreadInfo<HeightMap>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    private void OnValidate()
    {
        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdate;
            meshSettings.OnValuesUpdated += OnValuesUpdate;
        }

        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdate;
            heightMapSettings.OnValuesUpdated += OnValuesUpdate;
        }

        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    private void Start()
    {
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.MinHeight, heightMapSettings.MaxHeight);
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
        if (heightMapThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < heightMapThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<HeightMap> threadInfo = heightMapThreadInfoQueue.Dequeue();
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
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.MinHeight, heightMapSettings.MaxHeight);

        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(
                                                                    meshSettings.NumVerticesPerLine, 
                                                                    meshSettings.NumVerticesPerLine, 
                                                                    heightMapSettings, 
                                                                    Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();

        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap.values));
                break;
            
            case DrawMode.Mesh:
                display.DrawMesh(
                    MeshGenerator.GenerateTerrainMesh( heightMap.values, meshSettings, editorPreviewLOD));
                break;
            
            case DrawMode.FalloffMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FallOffGenerator.GenerateFalloffMap(meshSettings.NumVerticesPerLine)));
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void RequestHeightMap(Vector2 center, Action<HeightMap> callback)
    {
        ThreadStart threadStart = delegate
        {
            HeightMapThread(center, callback);
        };
        
        new Thread(threadStart).Start();
    }

    void HeightMapThread(Vector2 center, Action<HeightMap> callback)
    {
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(
                                                                    meshSettings.NumVerticesPerLine, 
                                                                    meshSettings.NumVerticesPerLine, 
                                                                    heightMapSettings, 
                                                                    center);

        lock (heightMapThreadInfoQueue)
        {
            heightMapThreadInfoQueue.Enqueue(new MapThreadInfo<HeightMap>(callback, heightMap));
        }
    }
    
    public void RequestMeshData(HeightMap heightMap, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(heightMap, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(HeightMap heightMap, int lod, Action<MeshData> callback)
    {
        MeshData meshData = 
            MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod);
        
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
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
