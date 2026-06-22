// IMPORTANT: This file MUST be placed in Assets/Scripts/Editor/ (or any folder named Editor).
// If placed outside an Editor folder, Unity will try to include it in the WebGL build and throw errors.
// Create the folder if it doesn't exist: Assets/Scripts/Editor/

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TownGenerator))]
public class TownGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw all the normal serialized fields (colors, prefab, etc.) as usual
        DrawDefaultInspector();

        TownGenerator generator = (TownGenerator)target;

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Town Generation", EditorStyles.boldLabel);

        // Generate button — clears existing children first, then regenerates fresh
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Generate Town", GUILayout.Height(36)))
        {
            if (generator.transform.childCount > 0)
            {
                bool confirm = EditorUtility.DisplayDialog(
                    "Regenerate Town",
                    "This will clear all existing generated children and regenerate from scratch. Any manual changes to generated objects will be lost. Continue?",
                    "Yes, Regenerate",
                    "Cancel"
                );

                if (!confirm) return;
            }

            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Generate Town");
            generator.ClearGenerated();
            generator.GenerateTown();

            // Mark scene dirty so Unity knows to save the changes
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
            );

            Debug.Log("[TownGenerator] Town generated successfully in Edit Mode.");
        }

        GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
        if (GUILayout.Button("Clear Generated", GUILayout.Height(28)))
        {
            bool confirm = EditorUtility.DisplayDialog(
                "Clear Generated Town",
                "This will destroy all generated children. Are you sure?",
                "Yes, Clear",
                "Cancel"
            );

            if (!confirm) return;

            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Clear Generated Town");
            generator.ClearGenerated();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
            );
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "Group Enemy Placement:\n" +
            "Group_A spawns at (-3, 0, +22)\n" +
            "Group_B spawns at (+3, 0, +22)\n" +
            "Place your GroupTrigger at approx (0, 0.5, +22) with radius 5.",
            MessageType.Info
        );
    }
}