using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SandboxWindow : EditorWindow
{
    float _exampleFloat;
    Transform _exampleTransform1;
    Transform _exampleTransform2;
    bool _enableFields;

    // Always remember that a SerializedObject is just a copy of the actual Object.
    // 在 Inspector 改变属性值并不会立即应用到 SerializedObject, 更新需要调用 SerializedObject.Update
    SerializedObject _targetWeaponSo;

    [MenuItem("Example/Sandbox")]
    static void OpenWindow()
    {
        SandboxWindow sandboxWindow = GetWindow<SandboxWindow>();
        sandboxWindow.Show();
    }

    void Awake()
    {
        // GUIContent 类主要包含 text,tooltip,image 等 GUI element
        titleContent.text = "Sandbox";
        titleContent.tooltip = "This is a sandbox window.";

        // EditorGUIUtility.IconContent is a method to get Unity’s built-in icon asset.
        // https://github.com/halak/unity-editor-icons
        titleContent.image = EditorGUIUtility.IconContent("P4_LockedRemote@2x").image;
        minSize = new Vector2(300, 200);
        maxSize = new Vector2(800, 600);
    }

    void OnEnable()
    {
        Weapon targetWeapon = Resources.Load<Weapon>("Pistol");
        _targetWeaponSo = new SerializedObject(targetWeapon);
    }

    // OnGUI 方法绘制 IMGUI (而 CreateGUI 是新的使用 UI Toolkit 绘制)
    // 绘制 IMGUI 主要使用 GUI, GUILayout, EditorGUI ,EditorGUILayout 类
    // GUI,GUILayout 可绘制 runtime UI
    // EditorGUI, EditorGUILayout 只能绘制 editor-only UI
    // GUI, EditorGUI 需要使用 rect/area 排版
    // GUILayout, EditorGUILayout 使用自动排版
    // 另外 GUIUtility and EditorGUIUtility 类中提供一些帮助方法以及部分样式设置,如 Label width
    void OnGUI()
    {
        # region 使用 GUI, EditorGUI 进行布局

        /*
        // 定义一个 checkbox
        _enableFields = EditorGUI.Toggle(new Rect(20, 20, 300, 16), "Enable Fields", _enableFields);
        // GUI.enabled 的作用是可以开启或禁用控件
        // 设置 GUI 样式和 Gizmos 一样是一旦设置后将影响后面所有的控件,记录前值是为了对一些控件设置样式后恢复到初值,使其不影响后面的控件
        bool previousEnabled = GUI.enabled;
        GUI.enabled = previousEnabled && _enableFields;
        Color previousColor = GUI.color;
        GUI.color = Color.red;

        // GUI.enabled 与 GUI.color 的配置将影响下面代码块中的控件
        {
            // 给 FloatField 一个名称,类似与 ID 的作用
            GUI.SetNextControlName("ExampleFloat");
            _exampleFloat = EditorGUI.FloatField(new Rect(20, 38, 300, 16),
                new GUIContent("Example Float", "A tooltip for the example float"),
                _exampleFloat);

            // 设置 Label 宽度
            EditorGUIUtility.labelWidth = 120f; // 默认是 150px
            _exampleTransform1 = EditorGUI.ObjectField(new Rect(20, 57, 300, 16),
                "Example Transform1", // 可以省略则没有 Label
                _exampleTransform1,
                typeof(Transform),
                true) as Transform;
            _exampleTransform2 = EditorGUI.ObjectField(new Rect(20, 73, 300, 16),
                "Example Transform2",
                _exampleTransform2,
                typeof(Transform),
                true) as Transform;
        }

        GUI.enabled = previousEnabled;
        GUI.color = previousColor;

        if (GUI.Button(new Rect(20, 101, 300, 16), "Focus to Example Float"))
        {
            // 聚焦到指定名称的控件
            GUI.FocusControl("ExampleFloat");
        }
        */

        # endregion

        # region 使用 GUILayout, EditorGUILayout 进行布局

        /*
        // 指定一个区域 (不指定则使用 0,0 点作为窗口起点)
        // position's x, y 可以获取窗口位置, width, height 获取窗口尺寸
        GUILayout.BeginArea(new Rect(10, 10, position.width - 20, position.height - 20));

        // 开始水平布局
        EditorGUILayout.BeginHorizontal(); // same as GUILayout.BeginHorizontal();

        {
            _exampleFloat = EditorGUILayout.FloatField("Example Float", _exampleFloat);

            GUILayout.FlexibleSpace(); // automatically fill extra space
            // GUILayout.Space(20);

            _exampleTransform1 = EditorGUILayout.ObjectField("Example Transform1", _exampleTransform1,
                typeof(Transform), true) as Transform;
        }

        // 结束水平布局
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        if (GUILayout.Button("My Button", GUILayout.ExpandHeight(true)))
        {
            Debug.Log("Button Clicked.");
        }

        GUILayout.EndArea();
        */

        #endregion


        #region Chapter 13. 绘制 SO 对象的所有属性到自定义窗口

        GUILayout.BeginArea(new Rect(10, 10, position.width - 20, position.height - 20));

        // 将 Inspector 中对属性的修改同步到 SerializedObject
        _targetWeaponSo.Update();

        // SerializedObject 是对象的副本,而 SerializedProperty 则是属性的副本
        // 使用 FindProperty 获取 SerializedObject 副本中的属性, 属性名大小写一定要对应 (shift 加右键点击显示变量名称)
        SerializedProperty weaponNameProp = _targetWeaponSo.FindProperty("weaponName");
        weaponNameProp.stringValue = EditorGUILayout.TextField("Weapon Name", weaponNameProp.stringValue);

        SerializedProperty priceProp = _targetWeaponSo.FindProperty("price");
        priceProp.intValue = EditorGUILayout.IntField("Price", priceProp.intValue);

        SerializedProperty isRangedProp = _targetWeaponSo.FindProperty("isRanged");
        isRangedProp.boolValue = EditorGUILayout.Toggle("IsRanged", isRangedProp.boolValue);

        // 获取数组跟普通属性一样
        SerializedProperty ammoTypesProp = _targetWeaponSo.FindProperty("ammoTypes");
        // 获取数组大小
        ammoTypesProp.arraySize = EditorGUILayout.IntField("Ammo Types", ammoTypesProp.arraySize);

        // 增加缩进
        EditorGUI.indentLevel++;
        // 根据数组大小创建元素
        for (int i = 0; i < ammoTypesProp.arraySize; i++)
        {
            // 获取数组中的元素
            SerializedProperty ammoTypeElementProp = ammoTypesProp.GetArrayElementAtIndex(i);
            ammoTypeElementProp.stringValue = EditorGUILayout.TextField("Element " + i, ammoTypeElementProp.stringValue);
        }

        EditorGUI.indentLevel--;

        SerializedProperty statsProp = _targetWeaponSo.FindProperty("stats");
        // 创建折叠菜单
        statsProp.isExpanded = EditorGUILayout.Foldout(statsProp.isExpanded, "Stats", true);
        if (statsProp.isExpanded)
        {
            EditorGUI.indentLevel++;

            // 使用 stats.damage 访问子元素
            SerializedProperty damageProp = _targetWeaponSo.FindProperty("stats.damage");
            damageProp.floatValue = EditorGUILayout.FloatField("Damage", damageProp.floatValue);

            SerializedProperty accuracyProp = _targetWeaponSo.FindProperty("stats.accuracy");
            accuracyProp.floatValue = EditorGUILayout.FloatField("Accuracy", accuracyProp.floatValue);

            SerializedProperty mobilityProp = _targetWeaponSo.FindProperty("stats.mobility");
            mobilityProp.floatValue = EditorGUILayout.FloatField("Mobility", mobilityProp.floatValue);

            EditorGUI.indentLevel--;
        }

        // Unity 还提供了 PropertyField 可以根据属性类型自动创建 Field
        // 和上面使用 EditorGUILayout.xxxField 创建的效果一样
        // EditorGUILayout.PropertyField(weaponNameProp);
        // EditorGUILayout.PropertyField(priceProp);
        // EditorGUILayout.PropertyField(isRangedProp);
        // EditorGUILayout.PropertyField(ammoTypesProp);
        // EditorGUILayout.PropertyField(statsProp);


        // 另外 Unity 还提供使用迭代的方式来创建
        SerializedProperty iterator = _targetWeaponSo.GetIterator();
        // 获取所有可见的属性 ([HideInInspector] 除外)
        while (iterator.NextVisible(true))
        {
            EditorGUILayout.PropertyField(iterator);
        }

        // 在将 SerializedObject 的修改应用到实际对象中去
        _targetWeaponSo.ApplyModifiedProperties();

        GUILayout.EndArea();

        #endregion
    }


    # region CreateGUI 方法使用新的 UI Toolkit 进行绘制

    // UI Toolkit 可以使用 C# 创建, 也可以使用 UXML 和 USS 来创建
    /*void CreateGUI()
    {
        var label = new Label()
        {
            text = "Hello World!",
            style =
            {
                position = Position.Absolute,
                left = new StyleLength(new Length(50, LengthUnit.Pixel)),
                top = new StyleLength(new Length(30, LengthUnit.Pixel)),
                width = new StyleLength(new Length(200, LengthUnit.Pixel)),
                height = new StyleLength(new Length(16, LengthUnit.Pixel))
            }
        };

        var button = new Button(() => { Debug.Log("Clicked."); })
        {
            text = "Click Me!",
            style =
            {
                position = Position.Absolute,
                left = 50,
                top = 50,
                width = 200,
                height = 16
            }
        };

        rootVisualElement.Add(label);
        rootVisualElement.Add(button);
    }*/

    #endregion
}