using System;
using ChannelThree.ProcedutalWorld.Data;
using UnityEngine;

public class MapPreview : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        Mesh,
        FalloffMap,
        CanyonMap
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
    
    [SerializeField]
    private Renderer textureRender;

    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private MeshRenderer meshRenderer;
    
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

    public void DrawMapInEditor()
    {
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.MinHeight, heightMapSettings.MaxHeight);

        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(
                                                                    meshSettings.NumVerticesPerLine, 
                                                                    meshSettings.NumVerticesPerLine, 
                                                                    heightMapSettings, 
                                                                    Vector2.zero);

        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
                break;
            
            case DrawMode.Mesh:
                DrawMesh(
                    MeshGenerator.GenerateTerrainMesh( heightMap.values, meshSettings, editorPreviewLOD));
                break;
            
            case DrawMode.FalloffMap:
                DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FallOffGenerator.GenerateFalloffMap(meshSettings.NumVerticesPerLine),0,1)));
                break;

            case DrawMode.CanyonMap:
            DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(CanyonMapGenerator.GenerateCanyonMap(meshSettings.NumVerticesPerLine),0,1)));
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void DrawTexture(Texture2D texture)
    {
        // Using sharedMaterial so can test it in editor too
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;
        
        textureRender.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        textureRender.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }
}
