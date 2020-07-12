using UnityEngine;

namespace ChannelThree.ProcedutalWorld.Data
{
    [CreateAssetMenu(menuName = "Channel 3/Procedural World/Noise Data")]
    public class NoiseDataScriptableObject : UpdatableData
    {
        [SerializeField]
        private Noise.NormalizeMode normalizeMode;
        public Noise.NormalizeMode NormalizeMode => normalizeMode;
        
        [SerializeField]
        private float noiseScale = 0.3f;
        public float NoiseScale => noiseScale;
        
        [SerializeField]
        private int octaves = 4;
        public int Octaves => octaves;

        [Range(0,1)]
        [SerializeField]
        private float persistence = 0.5f;
        public float Persistence => persistence;

        [SerializeField]
        private float lacunarity = 2f;
        public float Lacunarity => lacunarity;

        [SerializeField]
        private int seed = 1;
        public int Seed => seed;
        
        [SerializeField]
        private Vector2 offset = Vector2.zero;
        public Vector2 Offset => offset;

        protected override void OnValidate()
        {
            if (lacunarity < 1) lacunarity = 1;
            if (octaves < 0) octaves = 0;
            
            base.OnValidate();
        }
    }
}
