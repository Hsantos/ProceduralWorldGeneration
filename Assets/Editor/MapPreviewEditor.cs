using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapPreview))]
public class MapPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapPreview mapGenerator = (MapPreview) target;

        if (DrawDefaultInspector() && mapGenerator.AutoUpdate)
        {
            mapGenerator.DrawMapInEditor();
        }

        if (GUILayout.Button("Generate"))
        {
            mapGenerator.DrawMapInEditor();
        }
    }
}
