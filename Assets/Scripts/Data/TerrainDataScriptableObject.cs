using UnityEngine;

namespace ChannelThree.ProcedutalWorld.Data
{
    [CreateAssetMenu(menuName = "Channel 3/Procedural World/Terrain Data")]
    public class TerrainDataScriptableObject : UpdatableData
    {
        [SerializeField]
        private float uniformScale = 2f;
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

        public float MinHeight => uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(0);
        public float MaxHeight => uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(1);

        protected override void OnValidate()
        {
            if(meshHeightMultiplier < 1) meshHeightMultiplier = 1;
            
            base.OnValidate();
        }
    }
}
