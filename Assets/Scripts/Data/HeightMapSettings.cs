using UnityEngine;

namespace ChannelThree.ProcedutalWorld.Data
{
    [CreateAssetMenu(menuName = "Channel 3/Procedural World/Height Map Settings")]
    public class HeightMapSettings : UpdatableData
    {
        [SerializeField]
        private NoiseSettings noiseSettings;
        public NoiseSettings NoiseSettings => noiseSettings;
        
        [SerializeField]
        private bool useFalloffMap;
        public bool UseFalloffMap => useFalloffMap;

        [SerializeField]
        private bool useCanyonMap;
        public bool UseCanyonMap => useCanyonMap;
        
        [SerializeField]
        private float heightMultiplier = 40f;
        public float HeightMultiplier => heightMultiplier;

        [SerializeField]
        private AnimationCurve heightCurve;
        public AnimationCurve HeightCurve => heightCurve;

        public float MinHeight => heightMultiplier * heightCurve.Evaluate(0);
        public float MaxHeight => heightMultiplier * heightCurve.Evaluate(1);

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            noiseSettings.ValidateValues();
            if(heightMultiplier < 1) heightMultiplier = 1;
            
            base.OnValidate();
        }
#endif
    }
}
