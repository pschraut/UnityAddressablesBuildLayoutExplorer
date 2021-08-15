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
        string m_StatusLabel;

        public override void Awake()
        {
            base.Awake();

            viewMenuOrder = 15;
            m_TreeView = new AssetsTreeView(window);
            m_SearchField = new SearchField(window);
        }

        public override void Rebuild(RichBuildLayout buildLayout)
        {
            base.Rebuild(buildLayout);

            m_TreeView.SetBuildLayout(buildLayout);

            var size = 0L;
            var count = 0;
            foreach (var asset in buildLayout.assets)
            {
                size += asset.size;
                count++;
            }
            m_StatusLabel = $"{count} assets making up {EditorUtility.FormatBytes(size)}";
        }

        public override void OnGUI()
        {
            var rect = GUILayoutUtility.GetRect(10, 10, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            m_TreeView.OnGUI(rect);
        }

        public override void OnToolbarGUI()
        {
            base.OnToolbarGUI();

            GUILayout.Space(10);

            if (m_SearchField.OnToolbarGUI(GUILayout.ExpandWidth(true)))
                m_TreeView.Search(m_SearchField.text);
        }

        public override void OnStatusbarGUI()
        {
            base.OnStatusbarGUI();

            GUILayout.Label(m_StatusLabel);
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

            m_TreeView.SetExpanded(bm.expandedIDs);
            m_TreeView.SetSelection(bm.selectedIDs, TreeViewSelectionOptions.RevealAndFrame | TreeViewSelectionOptions.FireSelectionChanged);
            m_TreeView.SetFocus();
        }

        public override NavigationBookmark GetBookmark()
        {
            var command = new Bookmark();
            command.selectedIDs = new List<int>(m_TreeView.GetSelection());
            command.expandedIDs = new List<int>(m_TreeView.GetExpanded());
            return command;
        }

        class Bookmark : NavigationBookmark
        {
            public List<int> selectedIDs = new List<int>();
            public List<int> expandedIDs = new List<int>();
        }
    }
}
