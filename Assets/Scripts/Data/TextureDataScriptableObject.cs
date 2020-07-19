using UnityEngine;

namespace ChannelThree.ProcedutalWorld.Data
{
    [CreateAssetMenu(menuName = "Channel 3/Procedural World/Texture Data")]
    public class TextureDataScriptableObject : UpdatableData
    {
        [SerializeField]
        private Color[] baseColours;

        [SerializeField]
        [Range(0,1)]
        private float[] baseStartHeights;
        
        private float savedMinHeight;
        private float savedMaxHeight;
        
        public void ApplyToMaterial(Material material)
        {
            material.SetFloat("corDoCao", 1f);
            material.SetInt("baseColourCount", baseColours.Length);
            material.SetColorArray("baseColours", baseColours);
            material.SetFloatArray("baseStartHeights", baseStartHeights);
            UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
        }

        public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
        {
            savedMinHeight = minHeight;
            savedMaxHeight = maxHeight;
            
            material.SetFloat("minHeight", minHeight);
            material.SetFloat("maxheight", maxHeight);
        }
    }
}
