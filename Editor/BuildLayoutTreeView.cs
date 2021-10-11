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
    public class BuildLayoutTreeViewState : NavigationBookmark
    {
        public List<int> selectedIDs = new List<int>();
        public List<int> expandedIDs = new List<int>();
        public Vector2 scrollPosition;
    }

    public abstract class BuildLayoutTreeView : UnityEditor.IMGUI.Controls.TreeView
    {
        public System.Action<BaseItem> selectedItemChanged;

        protected BuildLayoutWindow m_Window;
        protected int m_UniqueId;

        int m_FirstVisibleRow;
        TreeViewItem m_CachedTree;
        List<TreeViewItem> m_CachedRows;

        class CopyCellTextArgs
        {
            public BaseItem item;
            public int column;
        }

        class CopyRowTextArgs
        {
            public BaseItem item;
        }

        protected class ContextMenuArgs
        {
            public GenericMenu menu;
            public BaseItem item;
            public int column;
            public Rect rect;
        }

        public BuildLayoutTreeView(BuildLayoutWindow window, TreeViewState state, MultiColumnHeader multiColumnHeader)
                   : base(state, multiColumnHeader)
        {
            m_Window = window;

            rowHeight = 22;
            showAlternatingRowBackgrounds = true;
            showBorder = false;
            columnIndexForTreeFoldouts = 0;
            extraSpaceBeforeIconAndLabel = 0;
            baseIndent = 0;
            multiColumnHeader.sortingChanged += OnSortingChanged;
            multiColumnHeader.ResizeToFit();
            Reload();
        }

        public BuildLayoutTreeViewState GetState()
        {
            var result = new BuildLayoutTreeViewState();
            result.selectedIDs = new List<int>(GetSelection());
            result.expandedIDs = new List<int>(GetExpanded());
            result.scrollPosition = state.scrollPos;
            return result;
        }

        public void SetState(BuildLayoutTreeViewState treeViewState)
        {
            if (treeViewState == null)
                return;

            SetExpanded(treeViewState.expandedIDs);
            SetSelection(treeViewState.selectedIDs, TreeViewSelectionOptions.FireSelectionChanged);
            state.scrollPos = treeViewState.scrollPosition;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override TreeViewItem BuildRoot()
        {
            if (m_CachedTree != null)
                return m_CachedTree;

            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            root.AddChild(new TreeViewItem { id = root.id + 1, depth = -1, displayName = "" });
            return root;
        }

        /// <summary>
        /// Iterates the tree and calls the specified <paramref name="callback"/> for each tree item.
        /// </summary>
        /// <param name="callback">The method that is called for each item. Return true to abort the iteration process, false to continue.</param>
        public void IterateItems(System.Func<TreeViewItem, bool> callback)
        {
            IterateItemsInternal(rootItem, callback);
        }

        /// <summary>
        /// Iterates the tree and calls the specified <paramref name="callback"/> for each item that is part of the specified <paramref name="parent"/>.
        /// </summary>
        /// <param name="callback">The method that is called for each item. Return true to abort the iteration process, false to continue.</param>
        public void IterateItems(TreeViewItem parent, System.Func<TreeViewItem, bool> callback)
        {
            IterateItemsInternal(parent, callback);
        }

        // Return value indicates to abort the iteration process. false=continue, true=abort
        bool IterateItemsInternal(TreeViewItem parent, System.Func<TreeViewItem, bool> callback)
        {
            if (parent == null)
                return false;

            if (callback.Invoke(parent))
                return true;

            if (!parent.hasChildren)
                return false;

            for (int n = 0, nend = parent.children.Count; n < nend; ++n)
            {
                var child = parent.children[n];
                if (child == null)
                    continue;

                if (!child.hasChildren)
                {
                    if (callback.Invoke(child))
                        return true;
                    continue;
                }

                if (IterateItemsInternal(child, callback))
                    return true;
            }

            return false;
        }

        public void Clear()
        {
            m_UniqueId = 0;
            var root = new TreeViewItem { id = m_UniqueId++, depth = -1, displayName = "Root" };
            root.AddChild(new TreeViewItem { id = m_UniqueId++, depth = -1, displayName = "" });

            m_CachedTree = root;
            Reload();

            state.scrollPos = Vector2.zero;
            SetExpanded(new List<int>());
            SetSelection(new List<int>());
        }

        public void SetBuildLayout(RichBuildLayout buildLayout)
        {
            var expandedIDs = new List<int>(state.expandedIDs);
            var selectedIDs = new List<int>(state.selectedIDs);
            var scrollPos = state.scrollPos;

            m_UniqueId = 0;
            var root = new TreeViewItem { id = m_UniqueId++, depth = -1, displayName = "Root" };

            OnBuildTree(root, buildLayout);
            if (!root.hasChildren)
                root.AddChild(new TreeViewItem { id = m_UniqueId++, depth = -1, displayName = "" });

            m_CachedTree = root;
            Reload();

            SetExpanded(expandedIDs);
            SetSelection(selectedIDs);
            state.scrollPos = scrollPos;
        }

        public void Search(string search)
        {
            var selection = GetSelection();
            searchString = search;

            SetSelection(selection, TreeViewSelectionOptions.RevealAndFrame);
        }

        protected abstract void OnBuildTree(TreeViewItem rootItem, RichBuildLayout buildLayout);

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);

            var item = FindItem(id, rootItem) as BaseItem;
            if (item != null)
                item.OnDoubleClick();
        }

        protected override void BeforeRowsGUI()
        {
            base.BeforeRowsGUI();

            GetFirstAndLastVisibleRows(out m_FirstVisibleRow, out _);
        }

        protected virtual void OnContextMenu(ContextMenuArgs args)
        {
            AddCopyContextMenuItem(args);
            AddCopyRowContextMenuItem(args);
        }

        void AddCopyContextMenuItem(ContextMenuArgs args)
        {
            args.menu.AddItem(new GUIContent("Copy"), false, OnCopyCellText, new CopyCellTextArgs() { item = args.item, column = args.column });

            void OnCopyCellText(object o)
            {
                var ctx = o as CopyCellTextArgs;
                EditorGUIUtility.systemCopyBuffer = ctx.item.ToString(ctx.column);
            }
        }

        void AddCopyRowContextMenuItem(ContextMenuArgs args)
        {
            args.menu.AddItem(new GUIContent("Copy Row"), false, OnCopyRowText, new CopyRowTextArgs() { item = args.item });

            void OnCopyRowText(object o)
            {
                var text = "";
                var ctx = o as CopyRowTextArgs;

                for (var n = 0; n < multiColumnHeader.state.columns.Length; ++n)
                {
                    if (n > 0)
                        text += $", {ctx.item.ToString(n)}";
                    else
                        text += ctx.item.ToString(n);
                }

                EditorGUIUtility.systemCopyBuffer = text;
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var isContextClick = Event.current.type == EventType.ContextClick;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                var item = args.item as BaseItem;
                if (item == null)
                    continue;
                item.PrepareGUI(args.focused, args.selected);

                var rect = args.GetCellRect(i);

                if (isContextClick && rect.Contains(Event.current.mousePosition))
                {
                    var ctx = new ContextMenuArgs();
                    ctx.menu = new GenericMenu();
                    ctx.item = item;
                    ctx.column = i;
                    ctx.rect = rect;

                    OnContextMenu(ctx);
                    if (ctx.menu.GetItemCount() > 0)
                    {
                        ctx.menu.ShowAsContext();
                        throw new ExitGUIException();
                    }
                }

                // Draw the column separator line
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
                    rect = IndentByDepth(item.depth, rect);

                    if (item.icon != null)
                    {
                        var r = rect;
                        r.width = 20;
                        EditorGUI.LabelField(r, new GUIContent(item.icon), Styles.iconStyle);
                        rect.x += 20;
                        rect.width -= 20;
                    }
                }

                if (item != null)
                {
                    var column = args.GetColumn(i);
                    item.OnGUI(rect, column, args.selected);
                }
            }
        }

        void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            if (rootItem == null || !rootItem.hasChildren)
                return;

            Reload();

            var selected = GetSelection();
            if (selected.Count > 0)
                FrameItem(selected[0]);
        }

        protected int CompareItem(TreeViewItem x, TreeViewItem y)
        {
            var sortingColumn = multiColumnHeader.sortedColumnIndex;
            if (sortingColumn < 0)
                sortingColumn = 0;

            var ascending = multiColumnHeader.IsSortedAscending(sortingColumn);
            var itemA = (ascending ? x : y);
            var itemB = (ascending ? y : x);

            // Some items should not be affected by the sort, for example category
            // nodes should be stable regardless of sorting up or down.
            var typedItemA = itemA as BaseItem;
            if (typedItemA != null && !typedItemA.supportsSortingOrder)
            {
                itemA = x;
                itemB = y;
            }

            var result = 0;
            if (typedItemA != null)
                result = typedItemA.CompareTo(itemB, sortingColumn);

            if (result == 0 && itemA != null && itemB != null)
                result = itemA.id.CompareTo(itemB.id);

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
            if (m_CachedRows == null)
                m_CachedRows = new List<TreeViewItem>(128);
            m_CachedRows.Clear();

            if (hasSearch)
            {
                SearchTree(root, searchString, m_CachedRows);
                m_CachedRows.Sort(CompareItem);
            }
            else
            {
                SortAndAddExpandedRows(root, m_CachedRows);
            }

            return m_CachedRows;
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

                    var item = child as BaseItem;
                    if (item != null && !item.supportsSearch)
                        continue;

                    if (DoesItemMatchSearch(child, search))
                        result.Add(child);

                    stack.Push(child);
                }
            }
        }

        Rect IndentByDepth(int itemDepth, Rect rect)
        {
            var foldoutWidth = 14;
            var indent = itemDepth + 1;

            rect.x += indent * foldoutWidth;
            rect.width -= indent * foldoutWidth;

            return rect;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);

            BaseItem selectedItem = null;

            if (selectedIds != null && selectedIds.Count > 0)
                selectedItem = FindItem(selectedIds[0], rootItem) as BaseItem;

            if (selectedItem != null)
                selectedItemChanged?.Invoke(selectedItem);
        }

        [System.Serializable]
        public abstract class BaseItem : TreeViewItem
        {
            public bool supportsSortingOrder = true;
            public bool supportsSearch = false;
            public BuildLayoutTreeView treeView;

            static GUIContent s_GUIContent = new GUIContent();
            static bool s_Selected = false;
            static bool s_Focused = false;

            public void PrepareGUI(bool focused, bool selected)
            {
                s_Selected = selected;
                s_Focused = focused;
            }

            public abstract object GetObject();
            public abstract void OnGUI(Rect position, int column, bool selected);
            public abstract int CompareTo(TreeViewItem other, int column);

            public virtual void OnDoubleClick()
            {

            }

            public void NavigateTo(object target)
            {
                treeView.m_Window.NavigateTo(target);
            }

            public virtual string ToString(int column)
            {
                return "";
            }

            protected void TrySelectAsset(string assetPath)
            {
                var obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (obj != null)
                    Selection.activeObject = obj;
                else
                    treeView.m_Window.ShowNotification(CachedGUIContent($"'{assetPath}' doesn't exist in project."));
            }

            protected void LabelField(Rect position, string text, bool ghosted = false)
            {
                LabelField(position, CachedGUIContent(text), ghosted);
            }

            protected void LabelField(Rect position, GUIContent text, bool ghosted = false)
            {
                var style = s_Focused && s_Selected ? Styles.selectedLabelStyle : Styles.labelStyle;
                if (ghosted)
                    style = s_Focused && s_Selected ? Styles.selectedGhostLabelStyle: Styles.ghostLabelStyle;

                EditorGUI.LabelField(position, text, style);
            }

            protected Rect ButtonSpaceR(ref Rect position)
            {
                var pixels = treeView.rowHeight - 2;

                var r = position;
                r.x = r.xMax;
                r.width = Mathf.Min(pixels, r.width); //pixels;
                r.x -= (r.width + 2);
                r.y += 1;
                r.height -= 2;
                position.width -= (r.width + 2);
                return r;
            }

            protected static GUIContent CachedGUIContent(string text)
            {
                s_GUIContent.text = text;
                s_GUIContent.tooltip = "";
                s_GUIContent.image = null;
                return s_GUIContent;
            }

            protected static GUIContent CachedGUIContent(string text, string tooltip)
            {
                s_GUIContent.text = text;
                s_GUIContent.tooltip = tooltip;
                s_GUIContent.image = null;
                return s_GUIContent;
            }

            protected static GUIContent CachedGUIContent(Texture image, string tooltip)
            {
                s_GUIContent.text = "";
                s_GUIContent.tooltip = tooltip;
                s_GUIContent.image = image;
                return s_GUIContent;
            }
        }
    }
}
