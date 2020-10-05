using System;
using UnityEngine;

[Serializable]
public class TerrainMaterial
{
    public enum Terrains
    {
        Mars,
        Earth,
        Neptune
    }

    [SerializeField] public Terrains materialType;
    [SerializeField] private Material mapMaterial;

    public Material GetMaterial() => mapMaterial;


}
