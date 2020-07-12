using System;
using UnityEngine;

namespace ChannelThree.ProcedutalWorld.Data
{
    public class UpdatableData : ScriptableObject
    {
        public event System.Action OnValuesUpdated;
        public bool autoUpdate;

        protected virtual void OnValidate()
        {
            if(autoUpdate)
                NotifyOfUpdateValues();
        }

        public void NotifyOfUpdateValues()
        {
            OnValuesUpdated?.Invoke();
        }
    }
}
