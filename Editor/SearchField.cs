//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using UnityEditor;
using UnityEngine;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    internal class SearchField : UnityEditor.IMGUI.Controls.SearchField
    {
        EditorWindow m_EditorWindow;
        float m_FinishTime;
        string m_SearchString = "";

        public float delay
        {
            get;
            set;
        }

        public string text
        {
            get;
            private set;
        }

        public SearchField(EditorWindow editorWindow)
        {
            autoSetFocusOnFindCommand = false;
            delay = 1.0f;
            text = m_SearchString;
            m_EditorWindow = editorWindow;
        }

        public bool OnToolbarGUI(params GUILayoutOption[] options)
        {
            var isEnter = HasFocus() && Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
            var isESC = HasFocus() && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape;

            var newString = OnToolbarGUI(m_SearchString, options);
            if (newString != m_SearchString)
            {
                m_SearchString = newString;
                m_FinishTime = Time.realtimeSinceStartup + delay;
            }

            if (isEnter || isESC)
                m_FinishTime = 0;

            if (m_FinishTime > Time.realtimeSinceStartup)
            {
                m_EditorWindow.Repaint();
                return false;
            }

            if (m_SearchString != text)
            {
                text = m_SearchString;
                m_EditorWindow.Repaint();
                return true;
            }

            return false;
        }
    }
}
