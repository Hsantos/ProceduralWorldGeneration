using UnityEngine;

/// <summary>
/// For this class we will calculate the mesh according to those points example:
///
/// X X X X X 
/// X O O O X
/// X O O O X
/// X O O O X
/// X X X X X
///
/// Where:
///     O = The vertices used to create the mesh
///     X = The borders used to calculate the normals
///
/// We use the borders so when connecting to meshes we don-t have any colour difference.
/// 
/// </summary>

public class MeshData
{
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;
    private Vector3[] backedNormals;

    private Vector3[] outOfMeshVertices;
    private int[] outOfMeshTriangles;
    
    private int triangleIndex = 0;
    private int outOfMeshTriangleIndex;

    private bool usingFlatShading;

    public MeshData(int numVertsPerLine, int skipIncrement, bool usingFlatShading)
    {
        this.usingFlatShading = usingFlatShading;

        CalculateTheSizeOfVerticesArray(numVertsPerLine, skipIncrement);
        CalculateTheTrianglesArraySize(numVertsPerLine);

        int numberOfBorderLines = 4;
        int numberOfBorderEdges = 4;
        outOfMeshVertices = new Vector3[numVertsPerLine * numberOfBorderLines - numberOfBorderEdges];
        
        // Need to get the index of the 6 vertices used to create the 2 triangles per square
        // The number of vertices per square used to create the triangle
        int verticesPerSquare = 6;
        // The number of squares per border line or column = 4
        int squaresPerBorder = 4;
        // So the index here is 6 * 4 * numVertsPerLine
        outOfMeshTriangles = new int[verticesPerSquare * squaresPerBorder * (numVertsPerLine  - 2)];
    }

    private void CalculateTheSizeOfVerticesArray(int numVertsPerLine, int skipIncrement)
    {
        int numMeshEdgeVertices = (numVertsPerLine - 2) * 4 - 4;
        int numEdgeConnectionVertices = (skipIncrement - 1) * (numVertsPerLine - 5) / skipIncrement * 4;
        int numMainVerticesPerLine = (numVertsPerLine - 5) / skipIncrement + 1;
        int numMainVertices = numMainVerticesPerLine * numMainVerticesPerLine;
        
        vertices = new Vector3[numMeshEdgeVertices + numEdgeConnectionVertices + numMainVertices];
        uvs = new Vector2[vertices.Length];
    }

    private void CalculateTheTrianglesArraySize(int numVertsPerLine)
    {
        int numMeshEdgeTriangles = 8 * (numVertsPerLine - 4);
        int numMainTriangles = (numVertsPerLine - 1) * (numVertsPerLine - 1) * 2;
        
        triangles = new int[(numMeshEdgeTriangles + numMainTriangles) * 3];
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        bool isBorder = vertexIndex < 0;
        if (isBorder)
        {
            outOfMeshVertices[GetBorderIndex(vertexIndex)] = vertexPosition;
        }
        else
        {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int pointA, int pointB, int pointC)
    {
        bool isBorder = pointA < 0 || pointB < 0 || pointC < 0;

        if (isBorder)
        {
            outOfMeshTriangles[outOfMeshTriangleIndex] = pointA;
            outOfMeshTriangles[outOfMeshTriangleIndex + 1] = pointB;
            outOfMeshTriangles[outOfMeshTriangleIndex + 2] = pointC;
            outOfMeshTriangleIndex += 3;
        }
        else
        {
            triangles[triangleIndex] = pointA;
            triangles[triangleIndex + 1] = pointB;
            triangles[triangleIndex + 2] = pointC;
            triangleIndex += 3;
        }
    }

    public Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }
        
        int borderTriangleCount = outOfMeshTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = outOfMeshTriangles[normalTriangleIndex];
            int vertexIndexB = outOfMeshTriangles[normalTriangleIndex + 1];
            int vertexIndexC = outOfMeshTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            
            if(vertexIndexA >= 0)
                vertexNormals[vertexIndexA] += triangleNormal;
            if(vertexIndexB >= 0)
                vertexNormals[vertexIndexB] += triangleNormal;
            if(vertexIndexC >= 0)
                vertexNormals[vertexIndexC] += triangleNormal;
        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = indexA < 0 ? outOfMeshVertices[GetBorderIndex(indexA)] : vertices[indexA];
        Vector3 pointB = indexB < 0 ? outOfMeshVertices[GetBorderIndex(indexB)] : vertices[indexB];
        Vector3 pointC = indexC < 0 ? outOfMeshVertices[GetBorderIndex(indexC)] : vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    private int GetBorderIndex(int index)
    {
        return -index - 1;
    }

    public void ProcessMesh()
    {
        if (usingFlatShading)
        {
            FlatShading();
        }
        else
        {
            BakeNormals();
        }
    }

    private void BakeNormals()
    {
        backedNormals = CalculateNormals();
    }

    private void FlatShading()
    {
        Vector3[] flatShaderVertices = new Vector3[triangles.Length];
        Vector2[] flatShaderUVs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            flatShaderVertices[i] = vertices[triangles[i]];
            flatShaderUVs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShaderVertices;
        uvs = flatShaderUVs;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        if (usingFlatShading)
        {
            mesh.RecalculateNormals();
        }
        else
        {
            mesh.normals = backedNormals;
        }
        return mesh;
    }
}
