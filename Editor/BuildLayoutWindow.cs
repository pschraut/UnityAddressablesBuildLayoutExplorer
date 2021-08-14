//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    public class BuildLayoutWindow : EditorWindow
    {
        [SerializeField] BuildLayout m_Layout;

        Rect m_ViewButtonRect;
        Rect m_FileButtonRect;
        List<BuildLayoutView> m_Views = new List<BuildLayoutView>();
        string m_LoadedPath;
        string[] m_RecentPaths = new string[0];
        Rect m_StatusbarRect;

        public string[] recentPaths
        {
            get => m_RecentPaths;
        }

        void OnEnable()
        {
            titleContent = new GUIContent("BuildLayout Explorer");
            m_Layout = null;
            m_LoadedPath = "";
            m_Views = new List<BuildLayoutView>();
            LoadRecentPaths();

            CreateView<WelcomeView>();
            CreateView<BundlesView>();
            CreateView<GroupsView>();

            ShowView(FindView< WelcomeView>());
        }

        void OnDisable()
        {
            CloseBuildLayout();

            foreach (var view in m_Views)
                view.OnDestroy();

            m_Views = new List<BuildLayoutView>();
            m_Layout = null;
            m_LoadedPath = "";
        }

        void OnGUI()
        {
            OnToolbarGUI();

            foreach (var view in m_Views)
            {
                if (view.isVisible)
                    view.OnGUI();
            }

            OnStatusbarGUI();
        }

        void ShowView(BuildLayoutView view)
        {
            foreach (var v in m_Views)
            {
                if (v.isVisible)
                    v.Hide();
            }

            view.Show();
        }

        BuildLayoutView FindView<T>() where T : BuildLayoutView
        {
            foreach (var view in m_Views)
            {
                if (view is T)
                    return view;
            }

            return null;
        }

        void OnStatusbarGUI()
        {
            // Darken the status area a little
            if (Event.current.type == EventType.Repaint && m_StatusbarRect.size.magnitude > 1)
            {
                var r = m_StatusbarRect;
                r.height += 4; r.x -= 4; r.width += 8;
                var oldcolor = GUI.color;
                GUI.color = EditorGUIUtility.isProSkin ? new Color(0, 0, 0, 0.25f) : new Color(0, 0, 0, 0.125f);

                GUI.DrawTexture(r, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
                GUI.color = oldcolor;
            }

            using (new GUILayout.HorizontalScope(GUILayout.Height(20), GUILayout.ExpandWidth(true)))
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(2);

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(10);
                        DrawCustomStatusbarGUI();
                        GUILayout.FlexibleSpace();
                        DrawPathStatusbarGUI();
                        GUILayout.Space(10);
                    }
                }
            }
            if (Event.current.type == EventType.Repaint)
                m_StatusbarRect = GUILayoutUtility.GetLastRect();
        }

        void DrawPathStatusbarGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            var size = EditorStyles.label.CalcSize(new GUIContent(m_LoadedPath));
            GUILayout.Label(m_LoadedPath ?? "", GUILayout.MinWidth(size.x + 10));
            EditorGUI.EndDisabledGroup();
        }

        void DrawCustomStatusbarGUI()
        {
            foreach (var view in m_Views)
            {
                if (view.isVisible)
                    view.OnStatusbarGUI();
            }
        }

        void DrawFileToolbarItem()
        {
            var click = GUILayout.Button("File", EditorStyles.toolbarDropDown, GUILayout.Width(60));
            if (Event.current.type == EventType.Repaint)
                m_FileButtonRect = GUILayoutUtility.GetLastRect();
            if (!click)
                return;

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Open File..."), false, OpenFileDialog);
            if (System.IO.File.Exists("Library/com.unity.addressables/buildlayout.txt"))
                menu.AddItem(new GUIContent("Open buildlayout.txt"), false, delegate() { LoadBuildLayout("Library/com.unity.addressables/buildlayout.txt"); });
            menu.AddSeparator("");

            if (m_Layout != null)
                menu.AddItem(new GUIContent("Close"), false, CloseBuildLayout);
            else
                menu.AddDisabledItem(new GUIContent("Close"), false);
            menu.AddSeparator("");

            //if (m_Layout != null)
            //    menu.AddItem(new GUIContent("Save JSON..."), false, SaveJsonDialog);
            //else
            //    menu.AddDisabledItem(new GUIContent("Save JSON..."), false);
            //menu.AddSeparator("");

            menu.AddItem(new GUIContent("Exit"), false, delegate() { Close(); });

            menu.DropDown(m_FileButtonRect);
        }

        void DrawViewToolbarItem()
        {
            using(new EditorGUI.DisabledScope(m_Layout == null))
            {
                var click = GUILayout.Button("View", EditorStyles.toolbarDropDown, GUILayout.Width(60));
                if (Event.current.type == EventType.Repaint)
                    m_ViewButtonRect = GUILayoutUtility.GetLastRect();
                if (!click)
                    return;

                var menu = new GenericMenu();
                foreach (var view in m_Views)
                {
                    if (view.viewMenuOrder < 0)
                        continue;

                    menu.AddItem(view.titleContent, view.isVisible, (GenericMenu.MenuFunction2)delegate (object o)
                    {
                        ShowView(o as BuildLayoutView);
                    }, view);
                }

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("BuildLayout Explorer"), false, NewWindow);

                menu.DropDown(m_ViewButtonRect);
            }
        }

        void DrawCustomToolbarItems()
        {
            foreach (var view in m_Views)
            {
                if (view.isVisible)
                    view.OnToolbarGUI();
            }
        }

        void OnToolbarGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                DrawFileToolbarItem();
                DrawViewToolbarItem();
                DrawCustomToolbarItems();
            }
        }

        void NewWindow()
        {
            var wnd = CreateInstance<BuildLayoutWindow>();
            wnd.Show();

            // Unity positions the window at the same location as the current window is.
            // This looks like no window opened, therefore we slightly change the position of the new window.
            var rect = position;
            rect.x += 50;
            rect.y += 50;
            wnd.position = rect;
        }

        void OpenFileDialog()
        {
            var directory = "Library";
            if (m_RecentPaths.Length > 0)
                directory = System.IO.Path.GetDirectoryName(m_RecentPaths[0]);

            var path = EditorUtility.OpenFilePanelWithFilters("Open BuildLayout.txt", directory, new[] { "Text Files (*.txt)", "txt" });
            if (string.IsNullOrEmpty(path))
                return;
            LoadBuildLayout(path);
        }

        void SaveJsonDialog()
        {
            var path = EditorUtility.SaveFilePanel("Save BuildLayout as JSON", "", "BuildLayout.json", "json");
            if (string.IsNullOrEmpty(path))
                return;

            var json = JsonUtility.ToJson(m_Layout, true);
            System.IO.File.WriteAllText(path, json);
        }

        void CloseBuildLayout()
        {
            m_Layout = new BuildLayout();
            m_LoadedPath = "";

            foreach (var view in m_Views)
                view.Rebuild(m_Layout);

            ShowView(FindView<WelcomeView>());
        }

        void SaveRecentPaths()
        {
            for (var n = 0; n < m_RecentPaths.Length; ++n)
                EditorPrefs.SetString($"BundleLayoutExplorer.m_RecentPaths[{n}]", m_RecentPaths[n]);

            for (var n = m_RecentPaths.Length; n < 10; ++n)
                EditorPrefs.SetString($"BundleLayoutExplorer.m_RecentPaths[{n}]", "");
        }

        void LoadRecentPaths()
        {
            var list = new List<string>();

            for (var n = 0; n < 10; ++n)
            {
                var value=EditorPrefs.GetString($"BundleLayoutExplorer.m_RecentPaths[{n}]", "");
                if (string.IsNullOrEmpty(value))
                    break;

                if (!System.IO.File.Exists(value))
                    continue;

                list.Add(value);
            }

            m_RecentPaths = list.ToArray();
        }

        void AddRecentPath(string path)
        {
            var list = new List<string>(recentPaths);
            var index = list.IndexOf(path);
            if (index != -1)
                list.RemoveAt(index);
            list.Insert(0, path);

            if (list.Count > 10)
                list.RemoveAt(list.Count - 1);

            m_RecentPaths = list.ToArray();
            SaveRecentPaths();
        }

        public void RemoveRecentPath(string path)
        {
            var list = new List<string>(recentPaths);

            var index = list.IndexOf(path);
            if (index != -1)
                list.RemoveAt(index);

            m_RecentPaths = list.ToArray();
            SaveRecentPaths();
        }

        public void LoadBuildLayout(string path)
        {
            try
            {
                m_Layout = BuildLayout.Load(path);
                m_LoadedPath = path;
            }
            catch (System.Exception e)
            {
                CloseBuildLayout();
                Debug.LogException(e);
                EditorUtility.DisplayDialog("Error", $"BuildLayout Explorer cannot load the file '{path}'.\n\nError message:\n{e.Message}\n\nSee Console window for additional information.", "OK");
                return;
            }

            foreach (var view in m_Views)
                view.Rebuild(m_Layout);

            var welcomeView = FindView<WelcomeView>();
            if (welcomeView != null && welcomeView.isVisible)
                ShowView(FindView<BundlesView>());

            AddRecentPath(path);
        }

        T CreateView<T>() where T : BuildLayoutView, new()
        {
            var view = new T();
            view.window = this;
            view.Awake();
            m_Views.Add(view);
            return view;
        }

        [MenuItem("Window/Asset Management/Addressables/BuildLayout Explorer", priority = 100000)]
        static void OpenWindowMenuItem()
        {
            var wnd = GetWindow<BuildLayoutWindow>();
            wnd.Show();
        }
    }
}
