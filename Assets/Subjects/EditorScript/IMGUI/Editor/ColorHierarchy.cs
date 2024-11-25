using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// [CreateAssetMenu(menuName = "Color Hierarchy/Color Hierarchy")]
public class ColorHierarchy : ScriptableObject
{
    public List<Settings> settings = new List<Settings>();

    [System.Serializable]
    public struct Settings
    {
        public string key;
        public Color backgroundColor;
    }
}

[InitializeOnLoad]
public class ColorHierarchyEditor
{
    static ColorHierarchy _colorHierarchy;

    static ColorHierarchyEditor()
    {
        // 查找指定类型资源
        var guids = AssetDatabase.FindAssets(("t:ColorHierarchy"));
        if (guids.Length > 0)
        {
            // 获取 ColorHierarchy ScriptableObject
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _colorHierarchy = AssetDatabase.LoadAssetAtPath<ColorHierarchy>(path);

            // Delegate for OnGUI events for every visible list item in the HierarchyWindow.
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyOnGUI;
        }
    }

    static void HierarchyOnGUI(int instanceId, Rect selectionRect)
    {
        if (_colorHierarchy == null || _colorHierarchy.settings.Count == 0) return;

        var obj = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
        if (obj != null)
        {
            foreach (var setting in _colorHierarchy.settings)
            {
                if (obj.name.StartsWith(setting.key))
                {
                    string text = obj.name.Substring(setting.key.Length);
                    EditorGUI.DrawRect(selectionRect, setting.backgroundColor);
                    Rect rectOffset = new Rect(selectionRect.position + new Vector2(0f, -0.5f), selectionRect.size);
                    EditorGUI.DropShadowLabel(rectOffset, text.ToUpperInvariant());
                    break;
                }
            }
        }
    }
}