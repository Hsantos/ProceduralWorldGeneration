﻿using System;
using System.Collections;
using System.Collections.Generic;
using ChannelThree.ProcedutalWorld.Data;
using UnityEngine;

public partial class EndlessTerrain : MonoBehaviour
{
    private const float viewerMoveThresholdForChunkUpdate = 25f;
    private const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    private const float colliderGenerationDistanceThreshold = 5;

    public int colliderLODIndex;
    public LODInfo[] detailLevels;
    public static float maxViewDst;
    
    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    private Vector2 viewerPositionOld;

    private float meshWorldSize;
    private int chuncksVisibleInViewDst;
    private static MapGenerator mapGenerator;
    
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private static List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        meshWorldSize = mapGenerator.MeshSettings.MeshWorldSize;
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
                        terrainChunkDictionary.Add
                        (
                            viewedChunkCoord, 
                            new TerrainChunk
                            (
                                viewedChunkCoord, 
                                meshWorldSize, 
                                detailLevels,
                                colliderLODIndex,
                                transform, 
                                mapMaterial
                            )
                        );
                    }  
                }
            }
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

        public void RequestMesh(HeightMap heightMap)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(heightMap, lod, OnMeshDataReceived);
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCallback();
        }
    }
    
    [System.Serializable]
    public struct LODInfo
    {
        [Range(0,MeshSettings.numSupportedLODs - 1)]
        public int lod;
        public float visibleDstThreshold;
        public float sqrVisibleDstThreshold => visibleDstThreshold * visibleDstThreshold;
    }
}
