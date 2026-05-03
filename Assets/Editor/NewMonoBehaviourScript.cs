#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModuleDefinition))]
public class ModuleDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ModuleDefinition module = (ModuleDefinition)target;

        if (GUILayout.Button("Generate Icon from Prefab"))
        {
            if (module.prefab == null)
            {
                Debug.LogWarning("No prefab assigned.");
                return;
            }
            
            module.icon = PrefabIconGenerator.GenerateIcon(module.prefab);;
            EditorUtility.SetDirty(module);
            
            Debug.Log("Icon generated!");
        }
    }
}
#endif