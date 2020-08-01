﻿using ChannelThree.ProcedutalWorld.Data;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail)
    {
        int meshSimplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        
        // The borderedSize will not be included in the final mesh. Using just to calculate the normals
        int borderedSize = heightMap.GetLength(0); 
        int meshSize = borderedSize - 2 * meshSimplificationIncrement;
        int meshSizeUnsimplified = borderedSize - 2;
        
        // To set the vertex to the center
        float topLeftX = (meshSizeUnsimplified - 1)/-2f;
        float topLeftZ = (meshSizeUnsimplified - 1)/2f;

        int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;
        
        MeshData meshData = new MeshData(verticesPerLine, meshSettings.UseFlatShading);
        
        int [,] vertexIndicesMap = new int[borderedSize,borderedSize];
        
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;

        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                bool isBorderVertex = (y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1);

                if (isBorderVertex)
                {
                    vertexIndicesMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                int vertexIndex = vertexIndicesMap[x, y];
                
                Vector2 percent = new Vector2((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);
                
                float meshHeight = heightMap[x, y];
                Vector3 vertexPosition = new Vector3((topLeftX + percent.x * meshSizeUnsimplified) * meshSettings.MeshScale,
                                                     meshHeight, 
                                                     (topLeftZ - percent.y * meshSizeUnsimplified) * meshSettings.MeshScale);
                
                meshData.AddVertex(vertexPosition, percent, vertexIndex);

                if (x < borderedSize - 1 && y < borderedSize - 1)
                {
                    int pointA = vertexIndicesMap[x, y];
                    int pointB = vertexIndicesMap[x + meshSimplificationIncrement, y];
                    int pointC = vertexIndicesMap[x, y + meshSimplificationIncrement];
                    int pointD = vertexIndicesMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];

                    /* Setting the triangle according to the square point:
                     * 
                     *     a    b
                     *      O    O
                     * 
                     *      O    O
                     *     c    d
                     *
                     * Run clockwise to define the 2 triangles we have
                     * Triangle A = adc
                     * Triangle B = dab
                     */
                    meshData.AddTriangle(pointA, pointD, pointC);
                    meshData.AddTriangle(pointD, pointA, pointB);
                }
                
                vertexIndex++;
            }
        }

        meshData.ProcessMesh();

        return meshData;
    }
}