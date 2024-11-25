using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class ScatterTool : EditorWindow
{
    public float radius = 1f;
    public int spawnCount = 8;
    public GameObject spawnPrefab;

    SerializedObject _so;
    SerializedProperty _radiusProp;
    SerializedProperty _spawnCountProp;
    SerializedProperty _spawnPrefabProp;

    Vector2[] _randPoint;
    List<GameObject> _prefabAsset;
    List<GameObject> _spawnPrefabs;
    bool[] _selectedPrefabs;


    [MenuItem("Window/EditorScript/IMGUI/Scatter Tool")]
    public static void OpenWindow()
    {
        GetWindow<ScatterTool>("Scatter Tool");
    }

    void OnEnable()
    {
        _so = new SerializedObject(this);
        _radiusProp = _so.FindProperty("radius");
        _spawnCountProp = _so.FindProperty("spawnCount");
        _spawnPrefabProp = _so.FindProperty("spawnPrefab");

        GenerateRandomPoint();
        GetPrefabAsset();

        _selectedPrefabs = new bool[_prefabAsset.Count];
        _spawnPrefabs = new List<GameObject>();

        SceneView.duringSceneGui += DuringSceneGui;
    }

    // 查找 Prefab 资源
    void GetPrefabAsset()
    {
        _prefabAsset = new List<GameObject>();
        string[] prefabGuid = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        foreach (var guid in prefabGuid)
        {
            _prefabAsset.Add(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)));
        }
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGui;
    }

    void OnGUI()
    {
        // Update target object to SerializedObject
        _so.Update();

        EditorGUILayout.PropertyField(_radiusProp);
        _radiusProp.floatValue = Mathf.Clamp(_radiusProp.floatValue, 1f, 10f);

        EditorGUILayout.PropertyField(_spawnCountProp);
        _spawnCountProp.intValue = Mathf.Clamp(_spawnCountProp.intValue, 0, 100);

        EditorGUILayout.PropertyField(_spawnPrefabProp);

        // Apply SerializedObject to target object
        bool propApplied = _so.ApplyModifiedProperties();
        if (propApplied)
        {
            GenerateRandomPoint();
        }

        // 如果鼠标左键点击到当前 EditorWindow 内 (控件以为的地方)
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            // 取消 focus
            GUI.FocusControl(null);
            // 立即重绘当前 EditorWindow
            Repaint();
        }

        // Prefab 选择菜单
        GUILayout.BeginHorizontal();
        for (int i = 0; i < _prefabAsset.Count; i++)
        {
            Texture icon = AssetPreview.GetAssetPreview(_prefabAsset[i]);
            EditorGUI.BeginChangeCheck();
            _selectedPrefabs[i] = GUILayout.Toggle(_selectedPrefabs[i], new GUIContent(icon), GUILayout.Width(64), GUILayout.Height(64));
            if (EditorGUI.EndChangeCheck())
            {
                var pf = _prefabAsset[i];
                if (_selectedPrefabs[i])
                {
                    if (!_spawnPrefabs.Contains(pf))
                    {
                        _spawnPrefabs.Add(pf);
                    }
                }
                else
                {
                    _spawnPrefabs.Remove(pf);
                }
            }
        }

        GUILayout.EndHorizontal();
    }

    void DuringSceneGui(SceneView sceneView)
    {
        Handles.zTest = CompareFunction.LessEqual;

        // 是否按住 Alt
        bool holdingAlt = Event.current.modifiers.HasFlag(EventModifiers.Alt);
        // modifiers 存储的是二进制的 bit flag,使用按位与操作也是一样的效果
        // bool holdingCtrl = (Event.current.modifiers & EventModifiers.Control) != 0;

        // Alt + 鼠标中键修改 radius
        if (holdingAlt && Event.current.type == EventType.ScrollWheel)
        {
            _so.Update();
            _radiusProp.floatValue += Event.current.delta.y * 0.1f;
            // 立即应用修改,让 SceneGui 根据新的值进行绘制
            _so.ApplyModifiedPropertiesWithoutUndo();
            // 同时重绘 EditorWindow
            Repaint();
            // 消耗掉事件,避免同时缩放视图
            Event.current.Use();
        }

        // 获取场景视图摄像机的变换
        Transform cameraTransform = sceneView.camera.transform;

        // 使用摄像机构建射线检测落点
        // Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        // 使用鼠标构建射线
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        // 鼠标移动时重绘场景
        if (Event.current.type == EventType.MouseMove)
            sceneView.Repaint();

        // 记录最终生成点的位置与旋转
        List<Pose> pointPoses = new List<Pose>();

        // 使用 Ray 找到落点
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 构建表面的切线坐标,使用切线坐标的 XY 平面来分布随机点
            Vector3 axisZ = hit.normal;
            Vector3 axisX = Vector3.Cross(axisZ, cameraTransform.up).normalized;
            Vector3 axisY = Vector3.Cross(axisX, axisZ).normalized;

            Handles.color = Color.red;
            Handles.DrawAAPolyLine(2, hit.point, hit.point + axisX);
            Handles.color = Color.blue;
            Handles.DrawAAPolyLine(2, hit.point, hit.point + axisZ);
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(2, hit.point, hit.point + axisY);
            Handles.color = Color.white;
            Handles.DrawWireDisc(hit.point, hit.normal, radius);

            // 绘制表面上的点
            foreach (var p in _randPoint)
            {
                // 将 point 转换到 hit.point 的本地坐标系下
                var origin = hit.point + (axisX * p.x + axisY * p.y) * radius;
                // 将原点朝 normal 方向抬高一点
                origin += hit.normal * 2f;
                Ray ptRay = new Ray(origin, -hit.normal);
                // 从随机点上往下发射射线检测表面上的点
                if (Physics.Raycast(ptRay, out RaycastHit ptHit))
                {
                    // 方法一：使用 FromToRotation 方法将物体 Y 轴对齐到 hit.normal
                    // Quaternion rot = Quaternion.FromToRotation(Vector3.up, ptHit.normal);
                    // 方法二：使用 LookRotation 先将 X 对齐到 hit.normal 再绕 X 轴旋转 90 度
                    Quaternion rot = Quaternion.LookRotation(ptHit.normal) * Quaternion.Euler(90f, 0f, 0f);
                    var pose = new Pose(ptHit.point, rot);
                    pointPoses.Add(pose);

                    Handles.SphereHandleCap(-1, ptHit.point, Quaternion.identity, 0.05f, EventType.Repaint);
                    Handles.DrawLine(ptHit.point, ptHit.point + ptHit.normal * 0.2f);
                }
            }
        }

        if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Space)
        {
            SpawnObjects(pointPoses);
        }
    }

    // 实例化 prefab (Pose 结构体包含 position and rotation)
    void SpawnObjects(List<Pose> poses)
    {
        if (_spawnPrefabs == null || _spawnPrefabs.Count == 0)
            return;

        foreach (var pose in poses)
        {
            GameObject randPrefab = _spawnPrefabs[Random.Range(0, _spawnPrefabs.Count)];
            // 使用 InstantiatePrefab 才能在编辑器中正确生成 prefab
            GameObject newPrefab = PrefabUtility.InstantiatePrefab(randPrefab) as GameObject;
            // 创建对象专属的 Undo 方法
            Undo.RegisterCreatedObjectUndo(newPrefab, "Spawn Object");
            newPrefab.transform.position = pose.position;
            newPrefab.transform.rotation = pose.rotation;
        }

        GenerateRandomPoint();
    }

    void GenerateRandomPoint()
    {
        _randPoint = new Vector2[spawnCount];
        for (int i = 0; i < spawnCount; i++)
        {
            _randPoint[i] = Random.insideUnitCircle;
        }
    }
}