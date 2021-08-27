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
        RichBuildLayout m_Layout;

        Rect m_ViewButtonRect;
        Rect m_FileButtonRect;
        Rect m_SettingsButtonRect;
        List<BuildLayoutView> m_Views = new List<BuildLayoutView>();
        string m_LoadedPath;
        string[] m_RecentPaths = new string[0];
        Rect m_StatusbarRect;
        NavigationHistory m_Navigation = new NavigationHistory();

        public string[] recentPaths
        {
            get => m_RecentPaths;
        }

        public BuildLayoutView activeView
        {
            get
            {
                foreach(var view in m_Views)
                {
                    if (view.isVisible)
                        return view;
                }
                return null;
            }
        }

        void OnEnable()
        {
            titleContent = new GUIContent("BuildLayout Explorer");
            m_Layout = new RichBuildLayout();
            m_LoadedPath = "";
            m_Views = new List<BuildLayoutView>();
            Settings.changed += OnSettingsChanged;
            Settings.LoadSettings();
            LoadRecentPaths();

            CreateView(typeof(WelcomeView));
            foreach(var viewType in TypeCache.GetTypesWithAttribute<BuildLayoutViewAttribute>())
                CreateView(viewType);

            ShowView(FindView<WelcomeView>());
        }

        void OnDisable()
        {
            Settings.changed -= OnSettingsChanged;
            Settings.SaveSettings();
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
            if (view.isVisible)
                return;

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

        void OnSettingsChanged()
        {
            if (string.IsNullOrEmpty(m_Layout.unityVersion))
                return;

            RebuildViews();
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

        void DrawNavigationToolbarItem()
        {
            using (new EditorGUI.DisabledGroupScope(!m_Navigation.HasBack()))
            {
                if (GUILayout.Button(new GUIContent(Styles.navigateBackwardsIcon, "Navigate Backward"), EditorStyles.toolbarButton, GUILayout.Width(24)))
                {
                    if (m_Navigation.current != null)
                        m_Navigation.current.bookmark = m_Navigation.current.view.GetBookmark();

                    var cmd = m_Navigation.Back();
                    if (cmd != null)
                    {
                        ShowView(cmd.view);
                        cmd.view.SetBookmark(cmd.bookmark);
                    }
                }
            }

            using (new EditorGUI.DisabledGroupScope(!m_Navigation.HasForward()))
            {
                if (GUILayout.Button(new GUIContent(Styles.navigateForwardsIcon, "Navigate Forward"), EditorStyles.toolbarButton, GUILayout.Width(24)))
                {
                    if (m_Navigation.current != null)
                        m_Navigation.current.bookmark = m_Navigation.current.view.GetBookmark();

                    var cmd = m_Navigation.Forward();
                    if (cmd != null)
                    {
                        ShowView(cmd.view);
                        cmd.view.SetBookmark(cmd.bookmark);
                    }
                }
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

            var buildLayoutTXT = "Library/com.unity.addressables/buildlayout.txt";
            if (System.IO.File.Exists(buildLayoutTXT))
            {
                menu.AddItem(new GUIContent("Open buildlayout.txt"), false, delegate () { LoadBuildLayout(buildLayoutTXT); });
                //menu.AddSeparator("");
                //menu.AddItem(new GUIContent("Open buildlayout.txt Folder"), false, delegate () { EditorUtility.RevealInFinder(buildLayoutTXT); });
                //menu.AddItem(new GUIContent("Open buildlayout.txt with default App"), false, delegate () { EditorUtility.OpenWithDefaultApp(buildLayoutTXT); });
            }
            menu.AddSeparator("");

            if (m_Layout != null && !string.IsNullOrEmpty(m_Layout.addressablesVersion))
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

        void DrawSettingsToolbarItem()
        {
            var click = GUILayout.Button(new GUIContent(Styles.settingsIcon, "Settings"), Styles.iconToolbarButtonStyle, GUILayout.Width(20));
            if (Event.current.type == EventType.Repaint)
                m_SettingsButtonRect = GUILayoutUtility.GetLastRect();
            if (!click)
                return;

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Strip Hash from name"), Settings.stripHashFromName, delegate()
            {
                Settings.stripHashFromName = !Settings.stripHashFromName;
                Settings.SaveSettings();
                RebuildViews();
            });

            menu.AddItem(new GUIContent("Strip Extension from name"), Settings.stripExtensionFromName, delegate ()
            {
                Settings.stripExtensionFromName = !Settings.stripExtensionFromName;
                Settings.SaveSettings();
                RebuildViews();
            });

            menu.AddItem(new GUIContent("Strip Directory from name"), Settings.stripDirectoryFromName, delegate ()
            {
                Settings.stripDirectoryFromName = !Settings.stripDirectoryFromName;
                Settings.SaveSettings();
                RebuildViews();
            });

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Developer Tools/Debug View Menu"), Settings.debugViewMenu, delegate ()
            {
                Settings.debugViewMenu = !Settings.debugViewMenu;
                Settings.SaveSettings();
                RebuildViews();
            });

            menu.DropDown(m_SettingsButtonRect);
        }


        void DrawViewToolbarItem()
        {
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(m_LoadedPath)))
            {
                var click = GUILayout.Button("View", EditorStyles.toolbarDropDown, GUILayout.Width(60));
                if (Event.current.type == EventType.Repaint)
                    m_ViewButtonRect = GUILayoutUtility.GetLastRect();
                if (!click)
                    return;

                var prevOrder = -1;
                var menu = new GenericMenu();

                // sorts views by their viewMenuOrder
                var views = new List<BuildLayoutView>(m_Views);
                views.Sort(delegate (BuildLayoutView a, BuildLayoutView b)
                {
                    return a.viewMenuOrder.CompareTo(b.viewMenuOrder);
                });

                // add views to menu
                foreach (var view in views)
                {
                    if (view.viewMenuOrder < 0)
                        continue; // negative numbers indicate to ignore in view menu
                    if (prevOrder == -1)
                        prevOrder = view.viewMenuOrder;

                    var p0 = prevOrder;
                    var p1 = view.viewMenuOrder;
                    if (p1 - p0 >= 100)
                    {
                        var i = view.titleContent.text.LastIndexOf("/");
                        if (i == -1)
                            menu.AddSeparator("");
                        else
                            menu.AddSeparator(view.titleContent.text.Substring(0, i));
                    }
                    prevOrder = view.viewMenuOrder;

                    var c = new GUIContent(view.titleContent);
                    if (Settings.debugViewMenu)
                        c.text = $"{c.text}   [Order={view.viewMenuOrder}, Type={view.GetType().Name}]";

                    menu.AddItem(c, view.isVisible, (GenericMenu.MenuFunction2)delegate (object o)
                    {
                        var fromCommand = new NavigationCommand();
                        fromCommand.view = activeView;
                        fromCommand.bookmark = activeView.GetBookmark();

                        var v = o as BuildLayoutView;
                        var toCommand = new NavigationCommand();
                        toCommand.view = v;
                        toCommand.bookmark = v.GetBookmark();

                        m_Navigation.Add(fromCommand, toCommand);

                        ShowView(o as BuildLayoutView);
                    }, view);
                }

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Open with default App"), false, OpenInDefaultApp);
                menu.AddItem(new GUIContent("Open containing Folder"), false, OpenContainingFolder);

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("New Window"), false, NewWindow);

                menu.DropDown(m_ViewButtonRect);
            }
        }

        void OnToolbarGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                DrawNavigationToolbarItem();
                DrawFileToolbarItem();
                DrawViewToolbarItem();
                GUILayout.FlexibleSpace();
                DrawSettingsToolbarItem();
            }
        }

        void OpenInDefaultApp()
        {
            EditorUtility.OpenWithDefaultApp(m_LoadedPath);
        }

        void OpenContainingFolder()
        {
            EditorUtility.RevealInFinder(m_LoadedPath);
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
            m_Layout = new RichBuildLayout();
            m_LoadedPath = "";
            m_Navigation = new NavigationHistory();

            foreach (var view in m_Views)
                view.OnDestroy();
            m_Views = new List<BuildLayoutView>();

            CreateView(typeof(WelcomeView));
            foreach (var viewType in TypeCache.GetTypesWithAttribute<BuildLayoutViewAttribute>())
                CreateView(viewType);

            ShowView(FindView<WelcomeView>());
        }

        void RebuildViews()
        {
            foreach (var view in m_Views)
                view.Rebuild(m_Layout);
        }

        void SaveRecentPaths()
        {
            for (var n = 0; n < m_RecentPaths.Length; ++n)
                Settings.SetString($"m_RecentPaths[{n}]", m_RecentPaths[n]);

            for (var n = m_RecentPaths.Length; n < 10; ++n)
                Settings.SetString($"m_RecentPaths[{n}]", "");
        }

        void LoadRecentPaths()
        {
            var list = new List<string>();

            for (var n = 0; n < 10; ++n)
            {
                var value=Settings.GetString($"m_RecentPaths[{n}]", "");
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

        public void NavigateTo(object target)
        {
            BuildLayoutView activeView = null;
            foreach (var view in m_Views)
            {
                if (!view.isVisible)
                    continue;
                activeView = view;
                break;
            }

            var sortedViews = new List<BuildLayoutView>(m_Views);
            for (var n = sortedViews.Count - 1; n >= 0; --n)
            {
                if (sortedViews[n].viewMenuOrder < 0)
                    sortedViews.RemoveAt(n);
            }

            sortedViews.Sort(delegate (BuildLayoutView a, BuildLayoutView b)
            {
                return a.viewMenuOrder.CompareTo(b.viewMenuOrder);
            });

            foreach(var view in sortedViews)
            {
                if (!view.CanNavigateTo(target))
                    continue;

                var fromCommand = new NavigationCommand();
                fromCommand.bookmark = activeView.GetBookmark();
                fromCommand.view = activeView;

                var toCommand = new NavigationCommand();
                toCommand.bookmark = view.GetBookmark();
                toCommand.view = view;
                toCommand.target = target;

                m_Navigation.Add(fromCommand, toCommand);

                //command.toView = view;
                ShowView(view);
                view.NavigateTo(target);
                return;
            }
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
                m_Layout = new RichBuildLayout(BuildLayout.Load(path));
                m_LoadedPath = path;
            }
            catch (System.Exception e)
            {
                CloseBuildLayout();
                Debug.LogException(e);
                EditorUtility.DisplayDialog("Error", $"BuildLayout Explorer cannot load the file '{path}'.\n\nError message:\n{e.Message}\n\nSee Console window for additional information.", "OK");
                return;
            }

            RebuildViews();

            var welcomeView = FindView<WelcomeView>();
            if (welcomeView != null && welcomeView.isVisible)
                ShowView(FindView<BundlesView>());

            AddRecentPath(path);
        }

        BuildLayoutView CreateView(System.Type type)
        {
            var view = (BuildLayoutView)System.Activator.CreateInstance(type);
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
