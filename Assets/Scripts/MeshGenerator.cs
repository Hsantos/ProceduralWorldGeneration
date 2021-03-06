﻿using ChannelThree.ProcedutalWorld.Data;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail)
    {
        int skipIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;

        int numVertsPerLine = meshSettings.NumVerticesPerLine;

        Vector2 topLeft = new Vector2(-1, 1) * meshSettings.MeshWorldSize / 2f;
        
        MeshData meshData = new MeshData(numVertsPerLine, skipIncrement, meshSettings.UseFlatShading);
        
        int [,] vertexIndicesMap = new int[numVertsPerLine,numVertsPerLine];
        
        int meshVertexIndex = 0;
        int outOfMeshVertexIndex = -1;

        for (int y = 0; y < numVertsPerLine; y++)
        {
            for (int x = 0; x < numVertsPerLine; x++)
            {
                bool isOutOfMeshVertex = (y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1);
                bool isMainVertex = ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);
                bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && isMainVertex;
                
                if (isOutOfMeshVertex)
                {
                    vertexIndicesMap[x, y] = outOfMeshVertexIndex;
                    outOfMeshVertexIndex--;
                }
                else if(!isSkippedVertex)
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for (int y = 0; y < numVertsPerLine; y++)
        {
            for (int x = 0; x < numVertsPerLine; x++)
            {
                bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);

                if (!isSkippedVertex)
                {
                    bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;
                    bool isMeshEdgeVertex = (y == 1 || y == numVertsPerLine - 2 || x == 1 || x == numVertsPerLine - 2) && !isOutOfMeshVertex;
                    bool isMainVertex = (x - 2) % skipIncrement == 0 && (y - 2) % skipIncrement == 0 && !isOutOfMeshVertex && !isMeshEdgeVertex;
                    bool isEdgeConnectionVertex = (y == 2 || y == numVertsPerLine - 3 || x == 2 || x == numVertsPerLine - 3) && !isOutOfMeshVertex && !isMeshEdgeVertex && !isMainVertex;
                    
                    int vertexIndex = vertexIndicesMap[x, y];
                
                    Vector2 percent = new Vector2(x-1, y -1) / (numVertsPerLine - 3);
                
                    float height = heightMap[x, y];
                    Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, - percent.y) * meshSettings.MeshWorldSize;

                    if (isEdgeConnectionVertex)
                    {
                        bool isVertical = x == 2 || x == numVertsPerLine - 3;
                        int dstToMainVertexA = (isVertical ? (y - 2) : (x - 2)) % skipIncrement;
                        int dstToMainVertexB = skipIncrement - dstToMainVertexA;

                        float dstPercentFromAToB = dstToMainVertexA / (float)skipIncrement; 
                        float heightMainVertexA = heightMap[isVertical ? x : x - dstToMainVertexA, isVertical ? y - dstToMainVertexA : y];
                        float heightMainVertexB = heightMap[isVertical ? x : x + dstToMainVertexB, isVertical ? y + dstToMainVertexB : y];

                        height = heightMainVertexA * (1 - dstPercentFromAToB) + heightMainVertexB * dstPercentFromAToB;
                    }
                    
                    meshData.AddVertex(new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), percent, vertexIndex);

                    bool createTriangle = x < numVertsPerLine - 1 && y < numVertsPerLine - 1 && (!isEdgeConnectionVertex || (x != 2 && y != 2));
                    
                    if (createTriangle)
                    {
                        int currentIncrement = (isMainVertex && x != numVertsPerLine - 3 && y != numVertsPerLine - 3) ? skipIncrement : 1;
                        
                        int pointA = vertexIndicesMap[x, y];
                        int pointB = vertexIndicesMap[x + currentIncrement, y];
                        int pointC = vertexIndicesMap[x, y + currentIncrement];
                        int pointD = vertexIndicesMap[x + currentIncrement, y + currentIncrement];

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
                }
            }
        }

        meshData.ProcessMesh();

        return meshData;
    }
}