﻿using UnityEngine;

public partial class EndlessTerrain
{
    private class TerrainChunk
    {
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
        
        private bool mapDataReceived;
        private bool hasSetCollider;
        
        public TerrainChunk(Vector2 coord, float meshWorldSize, LODInfo[] detailLevels, int colliderLodIndex, Transform parent, Material material)
        {
            this.coord = coord;
            this.detailLevels = detailLevels;
            this.colliderLodIndex = colliderLodIndex;
            
            sampleCenter = coord * meshWorldSize / mapGenerator.MeshSettings.MeshScale;
            Vector2 position = coord * meshWorldSize;
            bounds = new Bounds(position, Vector2.one * meshWorldSize);
            
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
            
            mapGenerator.RequestHeightMap(sampleCenter,OnMapDataReceived);
        }

        void OnMapDataReceived(HeightMap heightMap)
        {
            this.heightMap = heightMap;
            mapDataReceived = true;
            
            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if(!mapDataReceived)
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
                        lodMesh.RequestMesh(heightMap);
                    }
                }
            }

            if (wasVisible != visible)
            {
                if (visible)
                    visibleTerrainChunks.Add(this);
                else
                    visibleTerrainChunks.Remove(this);
                
                SetVisible(visible);
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
                    lodMeshes[colliderLodIndex].RequestMesh(heightMap);
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
}