using System.Collections.Generic;
using System.Linq;
using ChannelThree.ProcedutalWorld.Data;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    
    private const float viewerMoveThresholdForChunkUpdate = 25f;
    private const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureDataScriptableObject textureDataSettings;
    
    
    public Transform viewer;
    [SerializeField] private List<TerrainMaterial> allMaterials;
    [SerializeField] private TerrainMaterial.Terrains terrainType;
    private TerrainMaterial terrainMaterial;
    
    private Vector2 viewerPosition;
    private Vector2 viewerPositionOld;
    private float meshWorldSize;
    private int chuncksVisibleInViewDst;
    
    private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    void Awake()
    {
        terrainMaterial = allMaterials.Find(m => m.materialType == terrainType);
    }


    private void Start()
    {
        textureDataSettings.ApplyToMaterial(terrainMaterial.GetMaterial());
        textureDataSettings.UpdateMeshHeights(terrainMaterial.GetMaterial(), heightMapSettings.MinHeight, heightMapSettings.MaxHeight);
        
        float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        meshWorldSize = meshSettings.MeshWorldSize;
        chuncksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);
        
        UpdateVisibleChunks();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if (viewerPosition != viewerPositionOld)
        {
            foreach (TerrainChunk terrainChunk in visibleTerrainChunks)
            {
                terrainChunk.UpdateCollisionMesh();
            }
        }
        
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    private void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoord = new HashSet<Vector2>();
        
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoord.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }
        
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int yOffset = -chuncksVisibleInViewDst; yOffset <= chuncksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chuncksVisibleInViewDst; xOffset <= chuncksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (!alreadyUpdatedChunkCoord.Contains(viewedChunkCoord))
                {
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        TerrainChunk terrainChunk = new TerrainChunk(
                                                            viewedChunkCoord,
                                                            heightMapSettings,
                                                            meshSettings,
                                                            detailLevels,
                                                            colliderLODIndex,
                                                            transform,
                                                            viewer,
                                                            terrainMaterial.GetMaterial());
                            
                        terrainChunkDictionary.Add(viewedChunkCoord, terrainChunk);
                        terrainChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        terrainChunk.Load();
                    }  
                }
            }
        }
    }

    private void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if (isVisible)
            visibleTerrainChunks.Add(chunk);
        else
            visibleTerrainChunks.Remove(chunk);
    }
}