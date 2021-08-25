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
    public class AssetDuplicatesView : BuildLayoutView
    {
        AssetDuplicatesTreeView m_TreeView;
        SearchField m_SearchField;

        public override void Awake()
        {
            base.Awake();

            viewMenuOrder = 16;
            m_TreeView = new AssetDuplicatesTreeView(window);
            m_TreeView.selectedItemChanged += SelectionChanged;
            m_SearchField = new SearchField(window);
        }

        public override void Rebuild(RichBuildLayout buildLayout)
        {
            base.Rebuild(buildLayout);

            m_TreeView.SetBuildLayout(buildLayout);
        }

        public override void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope(Styles.viewStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent(" Asset Duplicates", Styles.assetIcon), EditorStyles.boldLabel);

                    if (m_SearchField.OnToolbarGUI(GUILayout.ExpandWidth(true)))
                        m_TreeView.Search(m_SearchField.text);
                }

                var rect = GUILayoutUtility.GetRect(10, 10, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                m_TreeView.OnGUI(rect);
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
            var asset = target as RichBuildLayout.Asset;
            if (asset == null)
                return;

            var item = m_TreeView.FindItem(asset);
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
        }

        public override NavigationBookmark GetBookmark()
        {
            var bm = new Bookmark();
            bm.assetsState = m_TreeView.GetState();
            return bm;
        }

        void SelectionChanged(BuildLayoutTreeView.BaseItem item)
        {
        }

        class Bookmark : NavigationBookmark
        {
            public BuildLayoutTreeViewState assetsState;
        }
    }
}
