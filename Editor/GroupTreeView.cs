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
                    icon = Styles.groupIcon
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
                        icon = Styles.bundleIcon
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

            public override void OnGUI(Rect position, int column)
            {
                switch(column)
                {
                    case ColumnIDs.name:
                        EditorGUI.LabelField(position, group.name);
                        break;

                    case ColumnIDs.size:
                        EditorGUI.LabelField(position, $"{EditorUtility.FormatBytes(group.size)}");
                        break;

                    case ColumnIDs.bundles:
                        EditorGUI.LabelField(position, $"{group.bundles.Count}");
                        break;
                }
            }
        }

        [System.Serializable]
        class BundleItem : BaseItem
        {
            public RichBuildLayout.Archive bundle;

            public override int CompareTo(TreeViewItem other, int column)
            {
                var otherItem = other as BundleItem;
                if (otherItem == null)
                    return 1;

                switch (column)
                {
                    case ColumnIDs.name:
                        return string.Compare(displayName, otherItem.displayName, true);

                    case ColumnIDs.size:
                        return bundle.size.CompareTo(otherItem.bundle.size);

                    case ColumnIDs.bundles:
                        return 0;
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        if (GUI.Button(ButtonSpaceR(ref position), CachedGUIContent(Styles.bundleIcon, "Navigate to bundle")))
                            NavigateTo(bundle);
                        EditorGUI.LabelField(position, displayName);
                        break;

                    case ColumnIDs.size:
                        EditorGUI.LabelField(position, EditorUtility.FormatBytes(bundle.size), Styles.ghostLabelStyle);
                        break;

                    case ColumnIDs.bundles:
                        EditorGUI.LabelField(position, $"1", Styles.ghostLabelStyle);
                        break;
                }
            }
        }
    }
}
