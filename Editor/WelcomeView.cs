//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq.Expressions;
using System;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    public class WelcomeView : BuildLayoutView
    {
        GUIStyle m_Heading1Style;
        GUIStyle m_Heading2Style;

        public override void Awake()
        {
            base.Awake();

            viewMenuOrder = -1;
        }

        public override void OnGUI()
        {
            if (m_Heading1Style == null)
            {
                m_Heading1Style = new GUIStyle(EditorStyles.boldLabel);
                m_Heading1Style.fontSize = 28;
                m_Heading1Style.fontStyle = FontStyle.Bold;

                m_Heading2Style = new GUIStyle(EditorStyles.boldLabel);
                m_Heading2Style.fontSize = 18;
                m_Heading2Style.fontStyle = FontStyle.Bold;
            }

            GUILayout.Space(4);
            GUILayout.Label($"Addressables BuildLayout Explorer for Unity", m_Heading1Style);
            GUILayout.Label("Created by Peter Schraut (www.console-dev.de)");
            GUILayout.Space(16);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    DrawRecentPaths();
                    GUILayout.Space(8);

                    DrawHelp();
                    GUILayout.Space(8);

                    GUILayout.FlexibleSpace();
                }

                GUILayout.FlexibleSpace();
            }
        }

        void DrawRecentPaths()
        {
            if (window.recentPaths.Length == 0)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Recent", m_Heading2Style);

                for (int n = 0, nend = window.recentPaths.Length; n < nend; ++n)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        var path = window.recentPaths[n];

                        GUILayout.Label(string.Format("{0,2:##}", n + 1), GUILayout.Width(20));

                        if (GUILayout.Button(new GUIContent("X", "Remove entry from list"), GUILayout.Width(20), GUILayout.Height(16)))
                        {
                            window.RemoveRecentPath(path);
                            break;
                        }

                        if (LinkButton(path))
                            window.LoadBuildLayout(path);

                        if (Event.current.type == EventType.Repaint)
                            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                    }
                }
            }
        }

        void DrawHelp()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var docuURL = "https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer";
                GUILayout.Label("Latest version", m_Heading2Style);
                if (LinkButton(docuURL))
                    Application.OpenURL(docuURL);
                GUILayout.Space(8);

                var changelogURL = "https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer/CHANGLOG.MD";
                GUILayout.Label("Changelog", m_Heading2Style);
                if (LinkButton(changelogURL))
                    Application.OpenURL(changelogURL);
                GUILayout.Space(8);
            }
        }

        bool LinkButton(string title)
        {
            var result = GUILayout.Button(new GUIContent(title), EditorStyles.linkLabel);
            if (Event.current.type == EventType.Repaint)
                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            return result;
        }
    }
}
