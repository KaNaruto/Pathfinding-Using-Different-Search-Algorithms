using Script.Map;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator map = target as MapGenerator;
        if (DrawDefaultInspector())
            if (map != null)
                map.GenerateMap();


        if (GUILayout.Button("Generate Map"))
            if (map != null)
                map.GenerateMap();
    }
}