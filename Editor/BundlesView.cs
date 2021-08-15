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
        string m_StatusLabel;

        public override void Awake()
        {
            base.Awake();

            viewMenuOrder = 5;
            m_TreeView = new BundleTreeView(window);
            m_SearchField = new SearchField(window);
        }

        public override void Rebuild(RichBuildLayout buildLayout)
        {
            base.Rebuild(buildLayout);

            m_TreeView.SetBuildLayout(buildLayout);

            var size = 0L;
            var count = 0;
            foreach(var group in buildLayout.groups)
            {
                foreach(var bundle in group.bundles)
                {
                    size += bundle.size;
                    count++;
                }
            }
            m_StatusLabel = $"{count} bundles making up {EditorUtility.FormatBytes(size)}";
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

            bm = bookmark as Bookmark;
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
