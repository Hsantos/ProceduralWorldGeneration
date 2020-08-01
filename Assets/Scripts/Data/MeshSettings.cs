using UnityEngine;

namespace ChannelThree.ProcedutalWorld.Data
{
    [CreateAssetMenu(menuName = "Channel 3/Procedural World/Mesh Settings")]
    public class MeshSettings : UpdatableData
    {
        public const int numSupportedLODs = 5;
        public const int numSupportedChunkSizes = 9;
        public const int numSupportedFlatShadedChunkSizes = 3;
    
        public static readonly int[] SupportedChunkSizes = { 48,72,96,120,144,168,192,216,240 };
        
        [SerializeField]
        private float meshScale = 2f;
        public float MeshScale => meshScale;

        [SerializeField]
        private bool useFlatShading = true;
        public bool UseFlatShading => useFlatShading;
        
        [Range(0, numSupportedChunkSizes - 1)]
        [SerializeField]
        private int chunkSizeIndex;
    
        [Range(0, numSupportedFlatShadedChunkSizes - 1)]
        [SerializeField]
        private int flatShadedChunkSizeIndex;
        
        // Number of verts per line of mesh rendered at LOD = 0 (max resolution).
        // Includes the 2 extra verts used for calculate the normals but are exclude from final mesh
        public int NumVerticesPerLine => SupportedChunkSizes[UseFlatShading ? flatShadedChunkSizeIndex : chunkSizeIndex] + 1;

        public float MeshWorldSize => (NumVerticesPerLine - 3) * meshScale;
    }
}
