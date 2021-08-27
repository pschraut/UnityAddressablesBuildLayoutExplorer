//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    [BuildLayoutView]
    public class BundlesView : BuildLayoutView
    {
        BundleTreeView m_TreeView;
        SearchField m_SearchField;
        ReferencesView m_ReferencesToView;
        ReferencesView m_ReferencedByView;
        float m_SplitterTree = 0.333f;
        float m_SplitterReferences = 0.5f;
        string m_SplitterTreeKey = $"{nameof(BundlesView)}.{nameof(m_SplitterTree)}";
        string m_SplitterReferencesKey = $"{nameof(BundlesView)}.{nameof(m_SplitterReferences)}";

        public override void Awake()
        {
            base.Awake();

            viewMenuOrder = 5;
            m_TreeView = new BundleTreeView(window);
            m_TreeView.selectedItemChanged += SelectionChanged;
            m_SearchField = new SearchField(window);

            m_ReferencesToView = CreateView<ReferencesView>();
            m_ReferencedByView = CreateView<ReferencesView>();

            m_SplitterTree = Settings.GetFloat(m_SplitterTreeKey, m_SplitterTree);
            m_SplitterReferences = Settings.GetFloat(m_SplitterReferencesKey, m_SplitterReferences);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Settings.SetFloat(m_SplitterTreeKey, m_SplitterTree);
            Settings.SetFloat(m_SplitterReferencesKey, m_SplitterReferences);
        }

        public override void Rebuild(RichBuildLayout buildLayout)
        {
            base.Rebuild(buildLayout);

            m_TreeView.SetBuildLayout(buildLayout);
        }

        public override void OnGUI()
        {
            m_ReferencesToView.titleContent = new GUIContent(" References to", Styles.referencesToIcon);
            m_ReferencedByView.titleContent = new GUIContent(" Referenced by", Styles.referencedByIcon);

            using (new EditorGUILayout.VerticalScope(Styles.viewStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent(" Bundles", Styles.bundleIcon), EditorStyles.boldLabel);

                    if (m_SearchField.OnToolbarGUI(GUILayout.ExpandWidth(true)))
                        m_TreeView.Search(m_SearchField.text);
                }

                var rect = GUILayoutUtility.GetRect(10, 10, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                m_TreeView.OnGUI(rect);
            }

            m_SplitterTree = SplitterGUI.VerticalSplitter(nameof(m_SplitterTree).GetHashCode(), m_SplitterTree, 0.2f, 0.8f, window);

            // Bottom views
            using (new EditorGUILayout.HorizontalScope(GUILayout.Height(window.position.height * m_SplitterTree)))
            {
                using (new EditorGUILayout.VerticalScope(Styles.viewStyle, GUILayout.Width(window.position.width * (1 - m_SplitterReferences))))
                {
                    m_ReferencesToView.OnGUI();
                }

                m_SplitterReferences = SplitterGUI.HorizontalSplitter(nameof(m_SplitterReferences).GetHashCode(), m_SplitterReferences, 0.3f, 0.7f, window);

                using (new EditorGUILayout.VerticalScope(Styles.viewStyle, GUILayout.Width(window.position.width * m_SplitterReferences)))
                {
                    m_ReferencedByView.OnGUI();
                }
            }
        }

        public override bool CanNavigateTo(object target)
        {
            if (target is RichBuildLayout.Archive)
                return true;

            return base.CanNavigateTo(target);
        }

        public override void NavigateTo(object target)
        {
            // is the target object a bundle?
            var bundle = target as RichBuildLayout.Archive;
            if (bundle == null)
                return; // nope, we can only process bundle

            // find item that represents the bundle
            var item = m_TreeView.FindItem(bundle);
            if (item == null)
                return;

            // select the item
            m_TreeView.SetSelection(new[] { item.id }, TreeViewSelectionOptions.RevealAndFrame | TreeViewSelectionOptions.FireSelectionChanged);
            m_TreeView.SetFocus();
        }

        public override void SetBookmark(NavigationBookmark bookmark)
        {
            var bm = bookmark as Bookmark;
            if (bm == null)
            {
                Debug.LogError($"Cannot set bookmark, because the argument '{nameof(bookmark)}' is of the wrong type or null.");
                return;
            }

            m_TreeView.SetState(bm.bundlesState);
            m_TreeView.SetFocus();

            m_ReferencesToView.SetBookmark(bm.referencesToBookmark);
            m_ReferencedByView.SetBookmark(bm.referencedByBookmark);
        }

        public override NavigationBookmark GetBookmark()
        {
            var bm = new Bookmark();
            bm.bundlesState = m_TreeView.GetState();
            bm.referencesToBookmark = m_ReferencesToView.GetBookmark();
            bm.referencedByBookmark = m_ReferencedByView.GetBookmark();
            return bm;
        }

        void SelectionChanged(BuildLayoutTreeView.BaseItem item)
        {
            m_ReferencesToView.ShowReferences(Utility.GetReferencesTo(item.GetObject()));
            m_ReferencedByView.ShowReferences(Utility.GetReferencedBy(item.GetObject()));
        }

        class Bookmark : NavigationBookmark
        {
            public BuildLayoutTreeViewState bundlesState;
            public NavigationBookmark referencesToBookmark;
            public NavigationBookmark referencedByBookmark;
        }
    }
}
