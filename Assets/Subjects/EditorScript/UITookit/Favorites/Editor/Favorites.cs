using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using Object = UnityEngine.Object;

namespace MyTools
{
    public class Favorites : EditorWindow
    {
        [SerializeField] VisualTreeAsset uxmlAsset;
        [SerializeField] VisualTreeAsset itemUxmlAsset;

        List<Object> _favoriteObjects;
        List<string> _savedFavorites;
        ListView _listView;
        bool _newAssetDragged;

        [MenuItem("Window/My Tools/Favorites %g")]
        static void OpenWindow()
        {
            var win = GetWindow<Favorites>();
            // 初始窗口大小
            win.minSize = new Vector2(200, 180);
        }

        void OnEnable()
        {
            _favoriteObjects = new List<Object>();
            _savedFavorites = new List<string>();
        }

        void CreateGUI()
        {
            var root = rootVisualElement;

            var uxml = uxmlAsset.Instantiate();
            // 发现必须在代码中设置才能撑满窗口
            uxml.style.flexGrow = 1;

            // 注册 uxml 的拖拽事件
            // 发现注册 _listView 的拖拽事件响应资源拖入会有 BUG (窗口 Tab 栏拖动后会禁止资源的拖入)
            uxml.RegisterCallback<DragEnterEvent>(OnDragEnter);
            uxml.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            uxml.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            uxml.RegisterCallback<DragPerformEvent>(OnDragPerform);

            _listView = uxml.Q<ListView>();
            _listView.fixedItemHeight = 20;
            _listView.itemsSource = _favoriteObjects;
            _listView.makeItem = MakeItem;
            _listView.bindItem = BindItem;
            _listView.selectedIndicesChanged += OnSelectedIndicesChanged;

            var addButton = uxml.Q<Button>("Add");
            var removeButton = uxml.Q<Button>("Remove");

            addButton.RegisterCallback<ClickEvent>(OnAddClicked);
            removeButton.RegisterCallback<ClickEvent>(OnRemoveClicked);

            root.Add(uxml);

            // 加载存储文件
            LoadFavorites();
        }

        void OnDragEnter(DragEnterEvent evt)
        {
            // 当 Project Window 中的资源被拖入时,标记为 _newAssetDragged (为了区别与对 _listView 列表项的拖拽)
            if (DragAndDrop.objectReferences.Length > 0)
            {
                _newAssetDragged = true;
                // 暂时禁用 _listView (拖拽会受到 listView 自身拖拽事件的影响)
                _listView.SetEnabled(false);
            }
        }

        void OnDragUpdated(DragUpdatedEvent evt)
        {
            if (_newAssetDragged)
            {
                // 必须设置 visualMode 为非 None 以及 Rejected 事件 OnDragPerform 才会触发
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }
        }

        void OnDragLeave(DragLeaveEvent evt)
        {
            if (_newAssetDragged)
            {
                // 恢复 _listView
                _listView.SetEnabled(true);
            }
        }

        void OnDragPerform(DragPerformEvent evt)
        {
            if (_newAssetDragged)
            {
                AddFavorites(DragAndDrop.objectReferences);
                // 恢复 _listView
                _listView.SetEnabled(true);
                _newAssetDragged = false;
            }
        }

        // 点击列表项时设置对象为激活对象
        void OnSelectedIndicesChanged(IEnumerable<int> indices)
        {
            foreach (var index in indices)
            {
                // 设置这个属性 Unity 会显示对象到 Inspector 中
                Selection.activeObject = _favoriteObjects[index];
                break;
            }
        }

        // Add 按钮添加收藏
        void OnAddClicked(ClickEvent evt)
        {
            AddFavorites(Selection.objects);
        }

        void AddFavorites(Object[] objects)
        {
            var newObjectsAdded = false;
            foreach (var obj in objects)
            {
                if (!_favoriteObjects.Contains(obj))
                {
                    _favoriteObjects.Add(obj);
                    newObjectsAdded = true;
                }
            }

            if (newObjectsAdded)
            {
                _listView.RefreshItems();
                SaveFavorites();
            }
        }

        void OnRemoveClicked(ClickEvent evt)
        {
            var selected = _listView.selectedItem as Object;
            if (selected != null)
            {
                _favoriteObjects.Remove(selected);
                _listView.RefreshItems();
                SaveFavorites();
            }
        }

        void BindItem(VisualElement element, int index)
        {
            var label = element.Q<Label>("AssetName");
            var icon = element.Q<VisualElement>("Icon");
            icon.style.backgroundImage = AssetPreview.GetMiniThumbnail(_favoriteObjects[index]);
            label.text = _favoriteObjects[index].name;
        }

        VisualElement MakeItem()
        {
            return itemUxmlAsset.Instantiate();
        }

        // 获取文件路径
        string GetSavePath()
        {
            return Application.persistentDataPath + "/favorites.json";
        }

        // 保存收藏
        void SaveFavorites()
        {
            _savedFavorites.Clear();
            foreach (var obj in _favoriteObjects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                _savedFavorites.Add(assetPath);
            }

            File.WriteAllText(GetSavePath(), JsonUtility.ToJson(new FavoritesSaveData(_savedFavorites), true));
        }

        // 加载收藏
        void LoadFavorites()
        {
            string savePath = GetSavePath();
            // 文件不存在直接退出
            if (!File.Exists(savePath))
            {
                return;
            }

            // 解析存储的 json 文件到类型
            try
            {
                var saveData = JsonUtility.FromJson<FavoritesSaveData>(File.ReadAllText(savePath));
                _favoriteObjects.Clear();
                foreach (var path in saveData.assetPath)
                {
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                    _favoriteObjects.Add(obj);
                }

                _listView.RefreshItems();
            }
            catch
            {
                Debug.LogWarning("Favorites 存储文件解析错误");
            }
        }

        // JsonUtility 不能直接转换 List,需要使用 class/struct 进行包装
        [System.Serializable]
        public struct FavoritesSaveData
        {
            public string[] assetPath;

            public FavoritesSaveData(List<string> path)
            {
                assetPath = path.ToArray();
            }
        }
    }
}