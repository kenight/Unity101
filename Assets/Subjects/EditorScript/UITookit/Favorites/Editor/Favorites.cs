using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

namespace MyTools
{
    public class Favorites : EditorWindow
    {
        [SerializeField] VisualTreeAsset uxmlAsset;
        [SerializeField] VisualTreeAsset itemUxmlAsset;

        // 记录文件名
        const string SaveFileName = "favorites.json";

        List<Object> _favoriteObjects;
        List<string> _savedFavorites;
        VisualElement _listViewContainer;
        Button _addFavoriteButton;
        Button _removeFavoriteButton;
        ListView _listView;
        bool _newAssetDragged;

        [MenuItem("Window/My Tools/Favorites %g")]
        static void OpenWindow()
        {
            var win = GetWindow<Favorites>();
            // 初始窗口大小
            win.minSize = new Vector2(200, 180);
            var tIconContent = EditorGUIUtility.IconContent("Favorite");
            tIconContent.text = "Favorites";
            win.titleContent = tIconContent;
        }

        void OnEnable()
        {
            _favoriteObjects = new List<Object>();
            _savedFavorites = new List<string>();
        }

        void CreateGUI()
        {
            var root = rootVisualElement;

            _listViewContainer = uxmlAsset.Instantiate();
            // 发现必须在代码中设置才能撑满窗口
            _listViewContainer.style.flexGrow = 1;

            // 为什么这里使用 ListView 父级 uxml 注册拖拽事件
            // 原因：ListView Tab 拖动后不能接受拖拽输入,bug?
            // 使用 uxml 响应拖拽又会被 ListView 阻挡,所以需要临时禁用 ListView
            _listViewContainer.RegisterCallback<DragEnterEvent>(OnDragEnter);
            _listViewContainer.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            _listViewContainer.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            _listViewContainer.RegisterCallback<DragPerformEvent>(OnDragPerform);

            _listView = _listViewContainer.Q<ListView>();
            _listView.fixedItemHeight = 20;
            _listView.itemsSource = _favoriteObjects;
            _listView.makeItem = MakeItem;
            _listView.bindItem = BindItem;
            _listView.selectedIndicesChanged += OnSelectedIndicesChanged;

            _addFavoriteButton = _listViewContainer.Q<Button>("Add");
            _removeFavoriteButton = _listViewContainer.Q<Button>("Remove");

            _addFavoriteButton.RegisterCallback<ClickEvent>(OnAddClicked);
            _removeFavoriteButton.RegisterCallback<ClickEvent>(OnRemoveClicked);

            root.Add(_listViewContainer);

            // 加载记录文件
            LoadFavorites();
        }

        // 当窗口失去焦点时触发
        void OnLostFocus()
        {
            // 失去聚焦时重置 selectedIndex, 再次点击同一个 item 时使其触发 selectedIndicesChanged (否则不会触发因为没有改变发生)
            _listView.selectedIndex = -1;
        }

        void OnDisable()
        {
            _listViewContainer.UnregisterCallback<DragEnterEvent>(OnDragEnter);
            _listViewContainer.UnregisterCallback<DragUpdatedEvent>(OnDragUpdated);
            _listViewContainer.UnregisterCallback<DragLeaveEvent>(OnDragLeave);
            _listViewContainer.UnregisterCallback<DragPerformEvent>(OnDragPerform);
            _listView.selectedIndicesChanged -= OnSelectedIndicesChanged;
            _addFavoriteButton.UnregisterCallback<ClickEvent>(OnAddClicked);
            _removeFavoriteButton.UnregisterCallback<ClickEvent>(OnRemoveClicked);
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
            var selectedIndex = _listView.selectedIndex;
            if (selectedIndex < 0 || selectedIndex > _favoriteObjects.Count - 1)
            {
                return;
            }

            var selectedObject = _favoriteObjects[selectedIndex];
            var assetPath = AssetDatabase.GetAssetPath(selectedObject);

            // 是文件夹
            if (Directory.Exists(assetPath))
            {
                Selection.activeObject = selectedObject;
                // await OpenFolder();

                EditorApplication.delayCall += () =>
                {
                    // 没有找到更合适的方法进入文件夹,使用菜单命令模拟这个过程
                    // 且发现不延迟执行这个命名无效
                    EditorApplication.ExecuteMenuItem("Assets/Open");
                };
            }
            else
            {
                Selection.activeObject = selectedObject;
                // PingObject 在 Project windows 中有一个选中的动画 (但不会转到对应的 Inspector)
                EditorGUIUtility.PingObject(selectedObject);
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

        // 获取记录文件路径
        string GetSavePath()
        {
            return Application.persistentDataPath + "/" + SaveFileName;
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

            // 解析记录文件
            try
            {
                var saveData = JsonUtility.FromJson<FavoritesSaveData>(File.ReadAllText(savePath));
                _favoriteObjects.Clear();
                foreach (var path in saveData.assetPath)
                {
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                    // 资源路径是否有效 (资源可能被移动导致加载失败)
                    if (obj != null)
                    {
                        _favoriteObjects.Add(obj);
                    }
                    else
                    {
                        Debug.LogWarning($"Favorites:资源 {path} 不存在");
                    }
                }

                _listView.RefreshItems();
            }
            catch
            {
                Debug.LogWarning("Favorites:记录文件解析错误");
            }
        }

        // JsonUtility 不能直接转换 List,需要使用 class/struct 进行包装
        [System.Serializable]
        public struct FavoritesSaveData
        {
            public List<string> assetPath;

            public FavoritesSaveData(List<string> path)
            {
                assetPath = path;
            }
        }
    }
}