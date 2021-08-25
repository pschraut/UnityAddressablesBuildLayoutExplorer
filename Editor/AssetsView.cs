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
    [BuildLayoutView]
    public class AssetsView : BuildLayoutView
    {
        AssetsTreeView m_TreeView;
        SearchField m_SearchField;
        ReferencesView m_IncludedByView;
        ReferencesView m_IncludedInView;
        float m_SplitterTree = 0.333f;
        float m_SplitterReferences = 0.5f;
        string m_SplitterTreeKey = $"{nameof(AssetsView)}.{nameof(m_SplitterTree)}";
        string m_SplitterReferencesKey = $"{nameof(AssetsView)}.{nameof(m_SplitterReferences)}";

        public override void Awake()
        {
            base.Awake();

            viewMenuOrder = 15;
            m_TreeView = new AssetsTreeView(window);
            m_TreeView.selectedItemChanged += SelectionChanged;
            m_SearchField = new SearchField(window);

            m_IncludedByView = CreateView<ReferencesView>();
            m_IncludedInView = CreateView<ReferencesView>();

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
            m_IncludedByView.titleContent = new GUIContent(" Included by", Styles.includedByIcon);
            m_IncludedInView.titleContent = new GUIContent(" Included in", Styles.includedInIcon);

            using (new EditorGUILayout.VerticalScope(Styles.viewStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent(" Assets", Styles.assetIcon), EditorStyles.boldLabel);

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
                    m_IncludedByView.OnGUI();
                }

                m_SplitterReferences = SplitterGUI.HorizontalSplitter(nameof(m_SplitterReferences).GetHashCode(), m_SplitterReferences, 0.3f, 0.7f, window);

                using (new EditorGUILayout.VerticalScope(Styles.viewStyle, GUILayout.Width(window.position.width * m_SplitterReferences)))
                {
                    m_IncludedInView.OnGUI();
                }
            }
        }

        public override bool CanNavigateTo(object target)
        {
            if (target is RichBuildLayout.Asset)
                return true;

            return base.CanNavigateTo(target);
        }

        public override void NavigateTo(object target)
        {
            var bundle = target as RichBuildLayout.Asset;
            if (bundle == null)
                return;

            var item = m_TreeView.FindItem(bundle);
            if (item == null)
                return;

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

            m_TreeView.SetState(bm.assetsState);
            m_TreeView.SetFocus();

            m_IncludedByView.SetBookmark(bm.referencesToBookmark);
            m_IncludedInView.SetBookmark(bm.referencedByBookmark);
        }

        public override NavigationBookmark GetBookmark()
        {
            var bm = new Bookmark();
            bm.assetsState = m_TreeView.GetState();
            bm.referencesToBookmark = m_IncludedByView.GetBookmark();
            bm.referencedByBookmark = m_IncludedInView.GetBookmark();
            return bm;
        }

        void SelectionChanged(BuildLayoutTreeView.BaseItem item)
        {
            var asset = item.GetObject() as RichBuildLayout.Asset;
            var includedBy = new List<object>();
            var includedIn = new List<object>();

            if (asset.includedByAsset != null)
                includedBy.Add(asset.includedByAsset);

            if (asset.includedInBundle != null)
                includedIn.Add(asset.includedInBundle);

            m_IncludedByView.ShowReferences(includedBy);
            m_IncludedInView.ShowReferences(includedIn);
        }

        class Bookmark : NavigationBookmark
        {
            public BuildLayoutTreeViewState assetsState;
            public NavigationBookmark referencesToBookmark;
            public NavigationBookmark referencedByBookmark;
        }
    }
}
