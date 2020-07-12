using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    [SerializeField]
    private Renderer textureRender;

    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private MeshRenderer meshRenderer;

    public void DrawTexture(Texture2D texture)
    {
        // Using sharedMaterial so can test it in editor too
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        
        meshFilter.transform.localScale = Vector3.one * FindObjectOfType<MapGenerator>().TerrainData.UniformScale;
    }
}
