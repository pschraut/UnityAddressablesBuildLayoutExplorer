//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Oddworm.EditorFramework
{
    public class BuildLayoutWindow : EditorWindow
    {
        BuildLayout m_Layout;

        void OnEnable()
        {
            titleContent = new GUIContent("Build Layout Explorer");
        }

        void OnGUI()
        {
            DrawToolbar();
        }

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                OpenLoadFileDialog();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        void OpenLoadFileDialog()
        {
            var path = EditorUtility.OpenFilePanelWithFilters("Open BuildLayout.txt", "Assets/", new[] { "Text Files (*.txt)", "txt" });
            if (string.IsNullOrEmpty(path))
                return;
            LoadBuildLayout(path);
        }

        void LoadBuildLayout(string path)
        {
            m_Layout = BuildLayout.Load(path);

            var json = JsonUtility.ToJson(m_Layout, true);
            System.IO.File.WriteAllText("Assets/BuildLayout.json", json);
        }



        [MenuItem("Window/Asset Management/Addressables/Build Layout Explorer", priority = 1000)]
        static void OpenWindowMenuItem()
        {
            var wnd = GetWindow<BuildLayoutWindow>();
            wnd.Show();
        }

        [MenuItem("Test/Read Layout")]
        static void TestReadLayoutMenuItem()
        {
            var path = "Assets/buildlayout.txt";
            var layout = BuildLayout.Load(path);
            var json = JsonUtility.ToJson(layout, true);
            System.IO.File.WriteAllText("Assets/BuildLayout.json", json);

            foreach (var group in layout.groups)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append($"{group.name} = {group.size}\n");

                foreach (var archive in group.bundles)
                {
                    sb.Append($"    Archive {archive.name} {archive.size}\n");

                    if (archive.bundleDependencies.Count > 0)
                        sb.Append($"      BundleDependencies {archive.bundleDependencies.Count}\n");

                    foreach (var dep in archive.bundleDependencies)
                    {
                        sb.Append($"        {dep}\n");
                    }
                }

                Debug.Log(sb.ToString());
            }
        }
    }
}
