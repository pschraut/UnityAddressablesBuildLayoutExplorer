//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    public class ReferencesView : BuildLayoutView
    {
        ReferencesTreeView m_TreeView;
        SearchField m_SearchField;

        public override void Awake()
        {
            base.Awake();

            m_TreeView = new ReferencesTreeView(window);
            m_SearchField = new SearchField(window);
        }

        public void Clear()
        {
            m_TreeView.Clear();
        }

        public void ShowReferences(List<object> references)
        {
            m_TreeView.Clear();
            m_TreeView.references = references ?? new List<object>();
            m_TreeView.SetBuildLayout(buildLayout);
            m_TreeView.ExpandCategories();
        }

        public override void Rebuild(RichBuildLayout buildLayout)
        {
            base.Rebuild(buildLayout);

            m_TreeView.SetBuildLayout(buildLayout);
        }

        public override void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField(titleContent, EditorStyles.boldLabel);

                GUILayout.Space(10);

                if (m_SearchField.OnToolbarGUI(GUILayout.ExpandWidth(true)))
                    m_TreeView.Search(m_SearchField.text);
            }

            var rect = GUILayoutUtility.GetRect(10, 10, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            m_TreeView.OnGUI(rect);
        }

        public override NavigationBookmark GetBookmark()
        {
            var bm = new Bookmark();
            bm.referencesState = m_TreeView.GetState();
            return bm;
        }

        public override void SetBookmark(NavigationBookmark bookmark)
        {
            var bm = bookmark as Bookmark;
            if (bm == null)
            {
                Debug.LogError($"Cannot set bookmark, because the argument '{nameof(bookmark)}' is of the wrong type or null.");
                return;
            }

            m_TreeView.SetState(bm.referencesState);
            m_TreeView.SetFocus();
        }

        class Bookmark : NavigationBookmark
        {
            public BuildLayoutTreeViewState referencesState;
        }
    }
}
