using UnityEngine;

public static class MeshGenerator
{
    public const int numSupportedLODs = 5;
    public const int numSupportedChunkSizes = 9;
    public const int numSupportedFlatShadedChunkSizes = 3;
    
    public static readonly int[] supportedChunkSizes = { 48,72,96,120,144,168,192,216,240 };
    public static readonly int[] supportedFlatShadedChunkSizes = { 48,72,96 };
    
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve curve, int levelOfDetail, bool useFlatShading)
    {
        //To avoid weird hills spikes now every thread has its own animation curve. :(
        AnimationCurve heightCurve = new AnimationCurve(curve.keys);
        
        int meshSimplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        
        // The borderedSize will not be included in the final mesh. Using just to calculate the normals
        int borderedSize = heightMap.GetLength(0); 
        int meshSize = borderedSize - 2 * meshSimplificationIncrement;
        int meshSizeUnsimplified = borderedSize - 2;
        
        // To set the vertex to the center
        float topLeftX = (meshSizeUnsimplified - 1)/-2f;
        float topLeftZ = (meshSizeUnsimplified - 1)/2f;

        int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;
        
        MeshData meshData = new MeshData(verticesPerLine, useFlatShading);
        
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
                
                float meshHeight = heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplified,meshHeight, topLeftZ - percent.y * meshSizeUnsimplified);
                
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