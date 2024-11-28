using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MyTools
{
    // 创建一个实例后注释掉,避免重复创建这个配置资源
    [CreateAssetMenu(menuName = "Color Hierarchy/Color Hierarchy")]
    public class ColorHierarchy : ScriptableObject
    {
        public int fontSize = 12;
        public List<KeyConfig> keyConfigs;

        [System.Serializable]
        public struct KeyConfig
        {
            public string key;
            public Color textColor;
            public Color backgroundColor;
        }
    }

    [InitializeOnLoad]
    public class ColorHierarchyEditor
    {
        static readonly ColorHierarchy ColorHierarchyAsset;

        static ColorHierarchyEditor()
        {
            // 查找 ColorHierarchy ScriptableObject 配置资源
            var guids = AssetDatabase.FindAssets(("t:ColorHierarchy"));
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                ColorHierarchyAsset = AssetDatabase.LoadAssetAtPath<ColorHierarchy>(path);

                // Delegate for OnGUI events for every visible list item in the HierarchyWindow.
                EditorApplication.hierarchyWindowItemOnGUI += HierarchyOnGUI;
            }
        }

        static void HierarchyOnGUI(int instanceId, Rect selectionRect)
        {
            if (ColorHierarchyAsset.keyConfigs.Count == 0)
            {
                Debug.Log("请先对 ColorHierarchy 进行配置");
                return;
            }

            // obj 对应 Hierarchy 中的每一个对象 (或者每一栏) 
            var obj = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (obj != null)
            {
                foreach (var config in ColorHierarchyAsset.keyConfigs)
                {
                    if (obj.name.StartsWith(config.key))
                    {
                        var textStyle = new GUIStyle
                        {
                            fontSize = ColorHierarchyAsset.fontSize,
                            fontStyle = FontStyle.Bold,
                            alignment = TextAnchor.MiddleCenter,
                            normal = new GUIStyleState() { textColor = config.textColor }
                        };

                        string text = obj.name.Substring(config.key.Length);
                        EditorGUI.DrawRect(selectionRect, config.backgroundColor);
                        EditorGUI.LabelField(selectionRect, text.ToUpperInvariant(), textStyle);

                        // 一旦匹配到了就不再匹配后面的了
                        break;
                    }
                }
            }
        }
    }
}