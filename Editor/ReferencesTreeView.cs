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
    public class ReferencesTreeView : BuildLayoutTreeView
    {
        static class ColumnIDs
        {
            public const int name = 0;
            public const int count = 1;
            public const int size = 2;
        }

        public List<object> references = new List<object>();

        public ReferencesTreeView(BuildLayoutWindow window)
                   : base(window, new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[] {
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Name"), width = 250, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Count"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Size"), width = 80, autoResize = true },
                            })))
        {
            multiColumnHeader.SetSortDirection(ColumnIDs.name, true);
            multiColumnHeader.SetSortDirection(ColumnIDs.count, false);
            multiColumnHeader.SetSortDirection(ColumnIDs.size, false);
            multiColumnHeader.sortedColumnIndex = ColumnIDs.name;
        }

        protected override void OnBuildTree(TreeViewItem rootItem, RichBuildLayout buildLayout)
        {
            CategoryItem bundlesCategory = null;
            CategoryItem assetsCategory = null;
            CategoryItem groupsCategory = null;

            foreach(var r in references)
            {
                TryAddBundle(r as RichBuildLayout.Archive);
                TryAddAsset(r as RichBuildLayout.Asset);
                TryAddGroup(r as RichBuildLayout.Group);
            }

            void TryAddBundle(RichBuildLayout.Archive bundle)
            {
                if (bundle == null)
                    return;

                if (bundlesCategory == null)
                {
                    bundlesCategory = new CategoryItem()
                    {
                        treeView = this,
                        sortValue = 1,
                        id = m_UniqueId++,
                        depth = rootItem.depth + 1,
                        displayName = "Bundles",
                        icon = Styles.bundleIcon
                    };
                    rootItem.AddChild(bundlesCategory);
                }

                var bundleItem = new BundleItem()
                {
                    treeView = this,
                    bundle = bundle,
                    id = m_UniqueId++,
                    depth = bundlesCategory.depth + 1,
                    displayName = Utility.TransformBundleName(bundle.name),
                    icon = Styles.GetBuildLayoutObjectIcon(bundle)
                };
                bundlesCategory.AddChild(bundleItem);
            }

            void TryAddAsset(RichBuildLayout.Asset asset)
            {
                if (asset == null)
                    return;

                if (assetsCategory == null)
                {
                    assetsCategory = new CategoryItem()
                    {
                        treeView = this,
                        sortValue = 2,
                        id = m_UniqueId++,
                        depth = rootItem.depth + 1,
                        displayName = "Assets",
                        icon = Styles.assetIcon
                    };
                    rootItem.AddChild(assetsCategory);
                }

                var assetsItem = new AssetItem()
                {
                    treeView = this,
                    asset = asset,
                    id = m_UniqueId++,
                    depth = assetsCategory.depth + 1,
                    displayName = asset.name,
                    icon = Styles.GetBuildLayoutObjectIcon(asset)
                };
                assetsCategory.AddChild(assetsItem);
            }

            void TryAddGroup(RichBuildLayout.Group group)
            {
                if (group == null)
                    return;

                if (groupsCategory == null)
                {
                    groupsCategory = new CategoryItem()
                    {
                        treeView = this,
                        sortValue = 3,
                        id = m_UniqueId++,
                        depth = rootItem.depth + 1,
                        displayName = "Groups",
                        icon = Styles.groupIcon
                    };
                    rootItem.AddChild(groupsCategory);
                }

                var groupsItem = new GroupItem()
                {
                    treeView = this,
                    group = group,
                    id = m_UniqueId++,
                    depth = groupsCategory.depth + 1,
                    displayName = group.name,
                    icon = Styles.GetBuildLayoutObjectIcon(group)
                };
                groupsCategory.AddChild(groupsItem);
            }
        }

        class CategoryItem : BaseItem
        {
            public int sortValue;

            public CategoryItem()
            {
                supportsSortingOrder = false;
                supportsSearch = true;
            }

            public override object GetObject()
            {
                return null;
            }

            public override int CompareTo(TreeViewItem other, int column)
            {
                var otherItem = other as CategoryItem;
                if (otherItem == null)
                    return 1;

                return sortValue.CompareTo(otherItem.sortValue);
            }

            public override void OnGUI(Rect position, int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        EditorGUI.LabelField(position, displayName);
                        break;

                    case ColumnIDs.count:
                        if (hasChildren)
                            EditorGUI.LabelField(position, $"{children.Count}");
                        else
                            EditorGUI.LabelField(position, $"0");
                        break;
                }
            }
        }

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
                        return string.Compare(displayName, otherItem.displayName, true);

                    case ColumnIDs.size:
                        return bundle.size.CompareTo(otherItem.bundle.size);
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        if (GUI.Button(ButtonSpaceR(ref position), CachedGUIContent(Styles.GetBuildLayoutObjectIcon(GetObject()), "Navigate to bundle"), Styles.iconButtonStyle))
                            NavigateTo(GetObject());

                        EditorGUI.LabelField(position, displayName);
                        break;

                    case ColumnIDs.size:
                        EditorGUI.LabelField(position, EditorUtility.FormatBytes(bundle.size));
                        break;
                }
            }
        }

        class AssetItem : BaseItem
        {
            public RichBuildLayout.Asset asset;

            public AssetItem()
            {
                supportsSearch = true;
            }

            public override object GetObject()
            {
                return asset;
            }

            public override int CompareTo(TreeViewItem other, int column)
            {
                var otherItem = other as AssetItem;
                if (otherItem == null)
                    return 1;

                switch (column)
                {
                    case ColumnIDs.name:
                        return string.Compare(displayName, otherItem.displayName, true);

                    case ColumnIDs.size:
                        return asset.size.CompareTo(otherItem.asset.size);
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        if (GUI.Button(ButtonSpaceR(ref position), CachedGUIContent(Styles.GetBuildLayoutObjectIcon(GetObject()), "Navigate to asset"), Styles.iconButtonStyle))
                            NavigateTo(GetObject());

                        EditorGUI.LabelField(position, displayName);
                        break;

                    case ColumnIDs.size:
                        EditorGUI.LabelField(position, EditorUtility.FormatBytes(asset.size));
                        break;
                }
            }
        }

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
                        return string.Compare(displayName, otherItem.displayName, true);

                    case ColumnIDs.size:
                        return group.size.CompareTo(otherItem.group.size);
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        if (GUI.Button(ButtonSpaceR(ref position), CachedGUIContent(Styles.GetBuildLayoutObjectIcon(GetObject()), "Navigate to group"), Styles.iconButtonStyle))
                            NavigateTo(GetObject());

                        EditorGUI.LabelField(position, displayName);
                        break;

                    case ColumnIDs.size:
                        EditorGUI.LabelField(position, EditorUtility.FormatBytes(group.size));
                        break;
                }
            }
        }
    }
}
