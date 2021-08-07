//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    public abstract class BuildLayoutTreeView : UnityEditor.IMGUI.Controls.TreeView
    {
        [SerializeField] protected BuildLayoutWindow m_Window;
        [SerializeField] TreeViewItem m_CachedTree;
        [SerializeField] int m_FirstVisibleRow;
        [SerializeField] protected int m_UniqueId = 100;
        List<TreeViewItem> m_RowsCache;

        public BuildLayoutTreeView(BuildLayoutWindow window, TreeViewState state, MultiColumnHeader multiColumnHeader)
                   : base(state, multiColumnHeader)
        {
            m_Window = window;

            rowHeight = 20;
            showAlternatingRowBackgrounds = true;
            showBorder = false;
            columnIndexForTreeFoldouts = 0;
            extraSpaceBeforeIconAndLabel = 0;
            baseIndent = 0;
            multiColumnHeader.sortingChanged += OnSortingChanged;
            multiColumnHeader.ResizeToFit();
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            if (m_CachedTree != null)
                return m_CachedTree;

            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            root.AddChild(new TreeViewItem { id = root.id + 1, depth = -1, displayName = "" });
            return root;
        }

        public void SetBuildLayout(BuildLayout buildLayout)
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            OnBuildTree(root, buildLayout);
            if (!root.hasChildren)
                root.AddChild(new TreeViewItem { id = root.id + 1, depth = -1, displayName = "" });

            m_CachedTree = root;
            Reload();
        }

        protected abstract void OnBuildTree(TreeViewItem rootItem, BuildLayout buildLayout);

        protected override void BeforeRowsGUI()
        {
            base.BeforeRowsGUI();

            GetFirstAndLastVisibleRows(out m_FirstVisibleRow, out _);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                var item = args.item as BaseItem;
                var rect = args.GetCellRect(i);

                if (args.row == m_FirstVisibleRow)
                {
                    var r = rect;
                    r.x += r.width + (i > 0 ? 2 : -1);
                    r.width = 1;
                    r.height = 10000;
                    var oldColor = GUI.color;
                    GUI.color = new Color(0, 0, 0, 0.15f);
                    GUI.DrawTexture(r, EditorGUIUtility.whiteTexture);
                    GUI.color = oldColor;
                }

                if (i == 0)
                {
                    rect.x += extraSpaceBeforeIconAndLabel;
                    rect.width -= extraSpaceBeforeIconAndLabel;
                }

                if (item != null)
                {
                    var column = args.GetColumn(i);
                    item.OnGUI(rect, column);
                }
            }
        }

        void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            if (rootItem == null || !rootItem.hasChildren)
                return;

            Reload();
        }

        protected int CompareItem(TreeViewItem x, TreeViewItem y)
        {
            var sortingColumn = multiColumnHeader.sortedColumnIndex;
            if (sortingColumn < 0)
                sortingColumn = 0;

            var ascending = multiColumnHeader.IsSortedAscending(sortingColumn);
            var itemA = (ascending ? x : y);
            var itemB = (ascending ? y : x);

            var result = 0;
            var typedItemA = itemA as BaseItem;
            if (typedItemA != null)
                result = typedItemA.CompareTo(itemB, sortingColumn);
            else if (itemA != null)
                return itemA.id.CompareTo(itemB.id);

            return result;
        }

        protected virtual void SortAndAddExpandedRows(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (!root.hasChildren)
                return;

            root.children.Sort(CompareItem);
            foreach (var child in root.children)
                GetAndSortExpandedRowsRecursive(child, rows);
        }

        void GetAndSortExpandedRowsRecursive(TreeViewItem item, IList<TreeViewItem> expandedRows)
        {
            if (item == null)
                return;

            expandedRows.Add(item);

            if (item.hasChildren && IsExpanded(item.id))
            {
                item.children.Sort(CompareItem);
                foreach (var child in item.children)
                    GetAndSortExpandedRowsRecursive(child, expandedRows);
            }
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            if (m_RowsCache == null)
                m_RowsCache = new List<TreeViewItem>(128);
            m_RowsCache.Clear();

            if (hasSearch)
            {
                SearchTree(root, searchString, m_RowsCache);
                m_RowsCache.Sort(CompareItem);
            }
            else
            {
                SortAndAddExpandedRows(root, m_RowsCache);
            }

            return m_RowsCache;
        }

        protected virtual void SearchTree(TreeViewItem root, string search, List<TreeViewItem> result)
        {
            var stack = new Stack<TreeViewItem>();

            stack.Push(root);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!current.hasChildren)
                    continue;

                foreach (var child in current.children)
                {
                    if (child == null)
                        continue;

                    if (DoesItemMatchSearch(child, search))
                        result.Add(child);

                    stack.Push(child);
                }
            }
        }

        [System.Serializable]
        protected abstract class BaseItem : TreeViewItem
        {
            public abstract void OnGUI(Rect position, int column);
            public abstract int CompareTo(TreeViewItem other, int column);
        }
    }
}
