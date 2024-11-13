using UnityEditor;
using UnityEngine;

public class DragSceneTool : EditorWindow
{
    [MenuItem("Example/Drag Scene Tool")]
    public static void OpenWindow() => GetWindow<DragSceneTool>("Drag Scene Tool");

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        HandleDragging();
    }

    void HandleDragging()
    {
        Event evt = Event.current;
        Object[] dragAndDropObjects = DragAndDrop.objectReferences;
        switch (evt.type)
        {
            case EventType.DragPerform:
                Debug.Log("DragPerform");
                break;
            case EventType.DragUpdated:
                Debug.Log("DragUpdated");
                break;
        }
    }
}