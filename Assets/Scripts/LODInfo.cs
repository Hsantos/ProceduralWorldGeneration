using ChannelThree.ProcedutalWorld.Data;
using UnityEngine;

[System.Serializable]
public struct LODInfo
{
    [Range(0,MeshSettings.numSupportedLODs - 1)]
    public int lod;
    public float visibleDstThreshold;
    public float sqrVisibleDstThreshold => visibleDstThreshold * visibleDstThreshold;
}