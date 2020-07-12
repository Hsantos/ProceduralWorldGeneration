using UnityEngine;

namespace ChannelThree.ProcedutalWorld.Data
{
    [CreateAssetMenu(menuName = "Channel 3/Procedural World/Terrain Data")]
    public class TerrainDataScriptableObject : UpdatableData
    {
        private float uniformScale = 2.5f;
        public float UniformScale => uniformScale;
        
        [SerializeField]
        private float meshHeightMultiplier;
        public float MeshHeightMultiplier => meshHeightMultiplier;

        [SerializeField]
        private bool useFlatShading;
        public bool UseFlatShading => useFlatShading;
        
        [SerializeField]
        private bool useFalloffMap;
        public bool UseFalloffMap => useFalloffMap;

        [SerializeField]
        private AnimationCurve meshHeightCurve;
        public AnimationCurve MeshHeightCurve => meshHeightCurve;

        protected override void OnValidate()
        {
            if(meshHeightMultiplier < 1) meshHeightMultiplier = 1;
            
            base.OnValidate();
        }
    }
}
