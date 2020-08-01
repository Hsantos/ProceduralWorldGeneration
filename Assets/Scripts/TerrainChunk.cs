using System;
using ChannelThree.ProcedutalWorld.Data;
using UnityEngine;

public class TerrainChunk
{
    private const float colliderGenerationDistanceThreshold = 5;
    
    public event Action<TerrainChunk, bool> onVisibilityChanged;
    
    public Vector2 coord;
    
    private GameObject meshObject;
    private Vector2 sampleCenter;
    private Bounds bounds;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private LODInfo[] detailLevels;
    private LODMesh[] lodMeshes;

    private HeightMap heightMap;
    private int previousLODIndex = -1;
    private int colliderLodIndex;

    private float maxViewDst;
    
    private bool heightMapDataReceived;
    private bool hasSetCollider;

    private HeightMapSettings heightMapSettings;
    private MeshSettings meshSettings;

    private Transform viewer;
    private Vector2 viewerPosition => new Vector2(viewer.position.x, viewer.position.z);
    
    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLodIndex, Transform parent, Transform viewer, Material material)
    {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLodIndex = colliderLodIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;
        
        sampleCenter = coord * meshSettings.MeshWorldSize / meshSettings.MeshScale;
        Vector2 position = coord * meshSettings.MeshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.MeshWorldSize);
        
        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshRenderer.material = material;
        
        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];

        for (int i = 0; i < detailLevels.Length; i++)
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;
            if (i == colliderLodIndex)
            {
                lodMeshes[i].updateCallback += UpdateCollisionMesh;
            }
        }

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        
        
    }

    public void Load()
    {
        ThreadedDataRequester.RequestData(() => 
                HeightMapGenerator.GenerateHeightMap
                (
                    meshSettings.NumVerticesPerLine, 
                    meshSettings.NumVerticesPerLine, 
                    this.heightMapSettings, 
                    sampleCenter
                ), 
            OnHeightMapReceived);
    }

    void OnHeightMapReceived(object heightMapObject)
    {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapDataReceived = true;
        
        UpdateTerrainChunk();
    }

    public void UpdateTerrainChunk()
    {
        if(!heightMapDataReceived)
            return;
        
        float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

        bool wasVisible = IsVisible();
        bool visible = viewerDstFromNearestEdge <= maxViewDst;

        if (visible)
        {
            int lodIndex = 0;
            for (int i = 0; i < detailLevels.Length - 1; i++)
            {
                if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                    lodIndex = i + 1;
                else
                    break;
            }

            if (lodIndex != previousLODIndex)
            {
                LODMesh lodMesh = lodMeshes[lodIndex];
                if (lodMesh.hasMesh)
                {
                    previousLODIndex = lodIndex;
                    meshFilter.mesh = lodMesh.mesh;
                }
                else if(!lodMesh.hasRequestedMesh)
                {
                    lodMesh.RequestMesh(heightMap, meshSettings);
                }
            }
        }

        if (wasVisible != visible)
        {
            SetVisible(visible);
            onVisibilityChanged?.Invoke(this, visible);
        }
    }

    public void UpdateCollisionMesh()
    {
        if(hasSetCollider)
            return;
        
        float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);
        

        if (sqrDstFromViewerToEdge < detailLevels[colliderLodIndex].sqrVisibleDstThreshold)
        {
            if (!lodMeshes[colliderLodIndex].hasRequestedMesh)
                lodMeshes[colliderLodIndex].RequestMesh(heightMap, meshSettings);
        }
        
        if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
        {
            if (lodMeshes[colliderLodIndex].hasMesh)
            {
                meshCollider.sharedMesh = lodMeshes[colliderLodIndex].mesh;
                hasSetCollider = true;
            }
        }
    }

    public void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);
    }

    public bool IsVisible()
    {
        return meshObject.activeSelf;
    }
}

class LODMesh
{
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    private int lod;

    public event Action updateCallback;

    public LODMesh(int lod)
    {
        this.lod = lod;
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
    }

    void OnMeshDataReceived(object meshDataObject)
    {
        mesh = ((MeshData) meshDataObject).CreateMesh();
        hasMesh = true;
        updateCallback();
    }
}