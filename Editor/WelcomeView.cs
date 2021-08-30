//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using UnityEditor;
using UnityEngine;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    public class WelcomeView : BuildLayoutView
    {
        GUIStyle m_Heading1Style;
        GUIStyle m_Heading2Style;
        Package m_Package;

        public override void Awake()
        {
            base.Awake();

            viewMenuOrder = -1; // negative number indicates to hide it in the View menu
            m_Package = Package.Load();
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
            GUILayout.Label($"{m_Package.displayName} {m_Package.version} for Unity", m_Heading1Style);
            GUILayout.Label("Created by Peter Schraut (www.console-dev.de)");
            GUILayout.Space(16);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    DrawAddressablesHelp();

                    DrawRecentPaths();
                    GUILayout.Space(8);

                    DrawHelp();
                    GUILayout.Space(8);

                    GUILayout.FlexibleSpace();
                }

                GUILayout.FlexibleSpace();
            }
        }

        public override void SetBookmark(NavigationBookmark bookmark)
        {
        }

        public override NavigationBookmark GetBookmark()
        {
            return new NavigationBookmark();
        }

        void DrawAddressablesHelp()
        {
#if ADDRESSABLES_PRESENT
            if (!UnityEditor.AddressableAssets.Settings.ProjectConfigData.GenerateBuildLayout)
            {
                EditorGUILayout.HelpBox($"Build Layout is disabled in Addressables Preferences.\nEnable the option from the main menu under 'Edit > Preferences', then select the 'Addressables' tab and tick the 'Debug Build Layout' setting.\nAddressables will then generate the file 'Library/com.unity.addressables/buildlayout.txt' the next time you build Addressables content.", MessageType.Warning);
                GUILayout.Space(8);
            }
#else
            EditorGUILayout.HelpBox($"Addressables package is not installed.\nYou can install the Addressables package from the main menu under 'Window > Package Manager'.", MessageType.Warning);
            GUILayout.Space(8);
#endif
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

                        if (GUILayout.Button(new GUIContent(Styles.deleteIcon, "Remove entry from list"), Styles.iconButtonStyle, GUILayout.Width(20), GUILayout.Height(16)))
                        {
                            window.RemoveRecentPath(path);
                            break;
                        }

                        if (LinkButton(path))
                            window.LoadBuildLayout(path);

                        if (GUILayout.Button(new GUIContent(Styles.openContainingFolderIcon, "Open containing folder"), Styles.iconButtonStyle, GUILayout.Width(20), GUILayout.Height(16)))
                        {
                            EditorUtility.RevealInFinder(path);
                            break;
                        }

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
                var docuURL = m_Package.documentationUrl;
                GUILayout.Label("Latest version", m_Heading2Style);
                if (LinkButton(docuURL))
                    Application.OpenURL(docuURL);
                GUILayout.Space(8);

                var changelogURL = "https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer/CHANGLOG.MD";
                GUILayout.Label("Changelog", m_Heading2Style);
                if (LinkButton(changelogURL))
                    Application.OpenURL(changelogURL);
                GUILayout.Space(8);

                var feedbackURL = "https://forum.unity.com/threads/addressables-buildlayout-explorer-for-unity.1162418/";
                GUILayout.Label("Feedback", m_Heading2Style);
                if (LinkButton(feedbackURL))
                    Application.OpenURL(feedbackURL);
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
