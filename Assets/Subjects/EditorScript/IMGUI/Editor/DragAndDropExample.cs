using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class DragAndDropExample : EditorWindow
{
    [MenuItem("Window/EditorScript/IMGUI/Drag And Drop Example")]
    public static void OpenWindow() => GetWindow<DragAndDropExample>("Drag And Drop Example");

    void OnGUI()
    {
        GUILayout.Label("Drag objects here:");
        Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drop Area");

        Event evt = Event.current;
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            if (dropArea.Contains(evt.mousePosition))
            {
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (Object obj in DragAndDrop.objectReferences)
                    {
                        Debug.Log($"Dropped object: {obj.name}");
                    }
                }
            }
        }
    }
}