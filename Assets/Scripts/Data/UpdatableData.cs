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
            if (autoUpdate)
                UnityEditor.EditorApplication.update += NotifyOfUpdateValues;
        }

        public void NotifyOfUpdateValues()
        {
            UnityEditor.EditorApplication.update -= NotifyOfUpdateValues;
            OnValuesUpdated?.Invoke();
        }
    }
}
