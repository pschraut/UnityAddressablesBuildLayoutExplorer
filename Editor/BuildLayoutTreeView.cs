//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Oddworm.EditorFramework
{
    public abstract class BuildLayoutTreeView : UnityEditor.IMGUI.Controls.TreeView
    {
        [SerializeField] protected BuildLayoutWindow m_Window;
        [SerializeField] TreeViewItem m_CachedTree;
        [SerializeField] int m_FirstVisibleRow;
        [SerializeField] protected int m_UniqueId = 100;

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

        [System.Serializable]
        protected abstract class BaseItem : TreeViewItem
        {
            public abstract void OnGUI(Rect position, int column);
        }
    }
}
