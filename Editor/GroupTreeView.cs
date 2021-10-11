//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    public class GroupTreeView : BuildLayoutTreeView
    {
        static class ColumnIDs
        {
            public const int name = 0;
            public const int size = 1;
            public const int bundles = 2;
        }

        public GroupTreeView(BuildLayoutWindow window)
                   : base(window, new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[] {
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Name"), width = 250, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Size"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Bundles"), width = 80, autoResize = true },
                            })))
        {
            multiColumnHeader.SetSortDirection(ColumnIDs.name, true);
            multiColumnHeader.SetSortDirection(ColumnIDs.size, false);
            multiColumnHeader.SetSortDirection(ColumnIDs.bundles, false);
            multiColumnHeader.sortedColumnIndex = ColumnIDs.size;
        }

        public TreeViewItem FindItem(RichBuildLayout.Group group)
        {
            TreeViewItem result = null;

            IterateItems(delegate (TreeViewItem i)
            {
                var b = i as GroupItem;
                if (b == null)
                    return false;

                if (b.group != group)
                    return false;

                result = b;
                return true;
            });

            return result;
        }

        protected override void OnBuildTree(TreeViewItem rootItem, RichBuildLayout buildLayout)
        {
            foreach (var group in buildLayout.groups)
            {
                var groupItem = new GroupItem
                {
                    treeView = this,
                    group = group,
                    id = m_UniqueId++,
                    depth = 0,
                    displayName = group.name,
                    icon = Styles.GetBuildLayoutObjectIcon(group)
                };
                rootItem.AddChild(groupItem);

                foreach (var bundle in group.bundles)
                {
                    var bundleItem = new BundleItem()
                    {
                        treeView = this,
                        bundle = bundle,
                        id = m_UniqueId++,
                        depth = groupItem.depth + 1,
                        displayName = Utility.TransformBundleName(bundle.name),
                        icon = Styles.GetBuildLayoutObjectIcon(bundle)
                    };
                    groupItem.AddChild(bundleItem);
                }
            }
        }


        [System.Serializable]
        class GroupItem : BaseItem
        {
            public RichBuildLayout.Group group;

            public GroupItem()
            {
                supportsSearch = true;
            }

            public override object GetObject()
            {
                return group;
            }

            public override int CompareTo(TreeViewItem other, int column)
            {
                var otherItem = other as GroupItem;
                if (otherItem == null)
                    return 1;

                switch (column)
                {
                    case ColumnIDs.name:
                        return string.Compare(group.name, otherItem.group.name, true);

                    case ColumnIDs.size:
                        return group.size.CompareTo(otherItem.group.size);

                    case ColumnIDs.bundles:
                        return group.bundles.Count.CompareTo(otherItem.group.bundles.Count);
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column, bool selected)
            {
                var text = ToString(column);
                switch (column)
                {
                    case ColumnIDs.name:
                        LabelField(position, text);
                        break;

                    case ColumnIDs.size:
                        LabelField(position, text);
                        break;

                    case ColumnIDs.bundles:
                        LabelField(position, text);
                        break;
                }
            }

            public override string ToString(int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        return group.name;

                    case ColumnIDs.size:
                        return EditorUtility.FormatBytes(group.size);

                    case ColumnIDs.bundles:
                        return $"{group.bundles.Count}";
                }

                return base.ToString(column);
            }
        }

        [System.Serializable]
        class BundleItem : BaseItem
        {
            public RichBuildLayout.Archive bundle;

            public BundleItem()
            {
                supportsSearch = true;
            }

            public override object GetObject()
            {
                return bundle;
            }

            public override int CompareTo(TreeViewItem other, int column)
            {
                var otherItem = other as BundleItem;
                if (otherItem == null)
                    return 1;

                switch (column)
                {
                    case ColumnIDs.name:
                        return string.Compare(bundle.name, otherItem.bundle.name, true);

                    case ColumnIDs.size:
                        return bundle.size.CompareTo(otherItem.bundle.size);

                    case ColumnIDs.bundles:
                        return 0;
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column, bool selected)
            {
                var text = ToString(column);
                switch (column)
                {
                    case ColumnIDs.name:
                        if (GUI.Button(ButtonSpaceR(ref position), CachedGUIContent(Styles.navigateIcon, "Navigate to bundle"), Styles.iconButtonStyle))
                            NavigateTo(bundle);
                        LabelField(position, text);
                        break;

                    case ColumnIDs.size:
                        LabelField(position, text, true);
                        break;

                    case ColumnIDs.bundles:
                        LabelField(position, text, true);
                        break;
                }
            }

            public override string ToString(int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        return displayName;

                    case ColumnIDs.size:
                        return EditorUtility.FormatBytes(bundle.size);

                    case ColumnIDs.bundles:
                        return $"1";
                }

                return base.ToString(column);
            }
        }
    }
}
