using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class SnapTool : EditorWindow
{
    public float perGridSize = 1f;

    SerializedObject _serializedObject;
    SerializedProperty _perGridSizeProp;

    [MenuItem("Example/Snap Tool")]
    public static void OpenWindow() => GetWindow<SnapTool>("Snap Tool");

    void OnEnable()
    {
        _serializedObject = new SerializedObject(this);
        _perGridSizeProp = _serializedObject.FindProperty("perGridSize");

        // 选中对象发生变化时,重绘 GUI
        Selection.selectionChanged += Repaint;
        SceneView.duringSceneGui += DuringSceneGui;
    }

    void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
        SceneView.duringSceneGui -= DuringSceneGui;
    }

    void OnGUI()
    {
        _serializedObject.Update();
        EditorGUILayout.PropertyField(_perGridSizeProp);
        _serializedObject.ApplyModifiedProperties();

        // 在该范围内的控件可被 Disable
        using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
        {
            if (GUILayout.Button("Snap Selection"))
            {
                SnapSelection();
            }
        }
    }

    void SnapSelection()
    {
        foreach (var go in Selection.gameObjects)
        {
            // 注意,这里要记录 Transform 才能退回位移操作
            Undo.RecordObject(go.transform, "Snap GameObject");
            go.transform.position = go.transform.position.Round(perGridSize);
        }
    }

    void DuringSceneGui(SceneView obj)
    {
        // 默认情况下,任何 action 都会触发 DuringSceneGui 执行
        // 这个在 UnityEngine 命名空间下的 Event 类定义了与 UnityGUI 相关的 event
        // 让只有在 Repaint 的情况下重新绘制
        if (Event.current.type == EventType.Repaint)
        {
            Handles.zTest = CompareFunction.LessEqual;

            // 画格子
            // 整体尺寸的一半
            const float halfGridSize = 16f;
            int lineCount = Mathf.RoundToInt((halfGridSize * 2) / perGridSize);
            if (lineCount % 2 == 0)
            {
                // 让其始终是奇数
                lineCount++;
            }

            int halfLineCount = lineCount / 2;

            for (int i = 0; i < lineCount; i++)
            {
                int offset = i - halfLineCount;
                float x = offset * perGridSize;
                float z0 = halfLineCount * perGridSize;
                float z1 = -halfLineCount * perGridSize;
                Vector3 p0 = new Vector3(x, 0, z0);
                Vector3 p1 = new Vector3(x, 0, z1);
                Handles.DrawAAPolyLine(p0, p1);
                p0 = new Vector3(z0, 0, x);
                p1 = new Vector3(z1, 0, x);
                Handles.DrawAAPolyLine(p0, p1);
            }
        }
    }
}