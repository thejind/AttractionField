
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(TileTypeD))]
public class TileManagerEditorD : Editor
{
     private string spriteNameToAssign = ""; // Variable to hold the entered sprite name

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TileTypeD spriteManager = (TileTypeD)target;

        // Text field for entering the sprite name
        GUILayout.Label("Enter Sprite Name to Assign:");
        spriteNameToAssign = EditorGUILayout.TextField(spriteNameToAssign);

        // Button to assign sprite to prefab
        if (GUILayout.Button("Assign Sprite to Prefab"))
        {
            // Get the selected GameObject
            GameObject selectedObject = Selection.activeGameObject;

            // Assign sprite to the prefab using the entered name
            spriteManager.AssignSpriteToPrefab(spriteNameToAssign, selectedObject);
        }
    }
}
#endif