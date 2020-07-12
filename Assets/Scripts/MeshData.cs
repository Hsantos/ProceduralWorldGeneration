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

    private Vector3[] borderVertices;
    private int[] borderTriangles;
    
    private int triangleIndex = 0;
    private int borderTriangleIndex;

    public MeshData(int verticesPerLine)
    {
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        uvs = new Vector2[verticesPerLine * verticesPerLine];
        
        int triangleArraySize = ((verticesPerLine - 1) * (verticesPerLine - 1)) * 6;
        triangles = new int[triangleArraySize];
        
        int numberOfBorderLines = 4;
        int numberOfBorderEdges = 4;
        borderVertices = new Vector3[verticesPerLine * numberOfBorderLines + numberOfBorderEdges];
        
        // Need to get the index of the 6 vertices used to create the 2 triangles per square
        // The number of vertices per square used to create the triangle
        int verticesPerSquare = 6;
        // The number of squares per border line or column = 4
        int squaresPerBorder = 4;
        // So the index here is 6 * 4 * verticesPerLine
        borderTriangles = new int[verticesPerLine * squaresPerBorder * verticesPerLine];
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        bool isBorder = vertexIndex < 0;
        if (isBorder)
        {
            borderVertices[GetBorderIndex(vertexIndex)] = vertexPosition;
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
            borderTriangles[borderTriangleIndex] = pointA;
            borderTriangles[borderTriangleIndex + 1] = pointB;
            borderTriangles[borderTriangleIndex + 2] = pointC;
            borderTriangleIndex += 3;
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
        
        int borderTriangleCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

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
        Vector3 pointA = indexA < 0 ? borderVertices[GetBorderIndex(indexA)] : vertices[indexA];
        Vector3 pointB = indexB < 0 ? borderVertices[GetBorderIndex(indexB)] : vertices[indexB];
        Vector3 pointC = indexC < 0 ? borderVertices[GetBorderIndex(indexC)] : vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    private int GetBorderIndex(int index)
    {
        return -index - 1;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = CalculateNormals();
        return mesh;
    }
}
