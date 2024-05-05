//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2024 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor.AddressableAssets.Build.Layout;

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

        CategoryItem m_BundlesCategory;
        CategoryItem m_AssetsCategory;
        CategoryItem m_GroupsCategory;

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

        public void ExpandCategories()
        {
            if (m_BundlesCategory != null)
                SetExpanded(m_BundlesCategory.id, true);

            if (m_AssetsCategory != null)
                SetExpanded(m_AssetsCategory.id, true);

            if (m_GroupsCategory != null)
                SetExpanded(m_GroupsCategory.id, true);
        }

        protected override void OnBuildTree(TreeViewItem rootItem, BuildLayout buildLayout)
        {
            m_BundlesCategory = null;
            m_AssetsCategory = null;
            m_GroupsCategory = null;

            foreach(var r in references)
            {
                TryAddBundle(r as BuildLayout.Bundle);
                TryAddAsset(r as BuildLayout.ExplicitAsset);
                TryAddAsset2(r as BuildLayout.DataFromOtherAsset);
                TryAddGroup(r as BuildLayout.Group);
            }

            void TryAddBundle(BuildLayout.Bundle bundle)
            {
                if (bundle == null)
                    return;

                if (m_BundlesCategory == null)
                {
                    m_BundlesCategory = new CategoryItem()
                    {
                        treeView = this,
                        sortValue = 5,
                        id = m_UniqueId++,
                        depth = rootItem.depth + 1,
                        displayName = "Bundles"
                    };
                    rootItem.AddChild(m_BundlesCategory);
                }

                var bundleItem = new BundleItem()
                {
                    treeView = this,
                    bundle = bundle,
                    id = m_UniqueId++,
                    depth = m_BundlesCategory.depth + 1,
                    displayName = Utility.TransformBundleName(bundle.Name),
                    icon = Styles.GetBuildLayoutObjectIcon(bundle)
                };
                m_BundlesCategory.AddChild(bundleItem);
            }

            void TryAddAsset(BuildLayout.ExplicitAsset asset)
            {
                if (asset == null)
                    return;

                if (m_AssetsCategory == null)
                {
                    m_AssetsCategory = new CategoryItem()
                    {
                        treeView = this,
                        sortValue = 10,
                        id = m_UniqueId++,
                        depth = rootItem.depth + 1,
                        displayName = "Assets"
                    };
                    rootItem.AddChild(m_AssetsCategory);
                }

                var assetsItem = new AssetItem()
                {
                    treeView = this,
                    asset = asset,
                    id = m_UniqueId++,
                    depth = m_AssetsCategory.depth + 1,
                    displayName = Utility.TransformBundleName(asset.AssetPath),
                    icon = Styles.GetBuildLayoutObjectIcon(asset)
                };
                m_AssetsCategory.AddChild(assetsItem);
            }

            void TryAddAsset2(BuildLayout.DataFromOtherAsset asset)
            {
                if (asset == null)
                    return;

                if (m_AssetsCategory == null)
                {
                    m_AssetsCategory = new CategoryItem()
                    {
                        treeView = this,
                        sortValue = 10,
                        id = m_UniqueId++,
                        depth = rootItem.depth + 1,
                        displayName = "Assets"
                    };
                    rootItem.AddChild(m_AssetsCategory);
                }

                var assetsItem = new AssetItem()
                {
                    treeView = this,
                    other = asset,
                    id = m_UniqueId++,
                    depth = m_AssetsCategory.depth + 1,
                    displayName = Utility.TransformBundleName(asset.AssetPath),
                    icon = Styles.GetBuildLayoutObjectIcon(asset)
                };
                m_AssetsCategory.AddChild(assetsItem);
            }

            void TryAddGroup(BuildLayout.Group group)
            {
                if (group == null)
                    return;

                if (m_GroupsCategory == null)
                {
                    m_GroupsCategory = new CategoryItem()
                    {
                        treeView = this,
                        sortValue = 1,
                        id = m_UniqueId++,
                        depth = rootItem.depth + 1,
                        displayName = "Groups"
                    };
                    rootItem.AddChild(m_GroupsCategory);
                }

                var groupsItem = new GroupItem()
                {
                    treeView = this,
                    group = group,
                    id = m_UniqueId++,
                    depth = m_GroupsCategory.depth + 1,
                    displayName = group.Name,
                    icon = Styles.GetBuildLayoutObjectIcon(group)
                };
                m_GroupsCategory.AddChild(groupsItem);
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

            public override void OnGUI(Rect position, int column, bool selected)
            {
                var text = ToString(column);
                switch (column)
                {
                    case ColumnIDs.name:
                        LabelField(position, text);
                        break;

                    case ColumnIDs.count:
                        LabelField(position, text);
                        break;
                }
            }

            public override string ToString(int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        return displayName;

                    case ColumnIDs.count:
                        if (hasChildren)
                            return $"{children.Count}";
                        return $"0";
                }

                return base.ToString(column);
            }
        }

        class BundleItem : BaseItem
        {
            public BuildLayout.Bundle bundle;

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
                        return string.Compare(bundle.Name, otherItem.bundle.Name, true);

                    case ColumnIDs.size:
                        return bundle.FileSize.CompareTo(otherItem.bundle.FileSize);
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
                            NavigateTo(GetObject());

                        LabelField(position, text);
                        break;

                    case ColumnIDs.size:
                        LabelField(position, text);
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
                        return EditorUtility.FormatBytes((long)bundle.FileSize);
                }

                return base.ToString(column);
            }
        }

        class AssetItem : BaseItem
        {
            public BuildLayout.ExplicitAsset asset;
            public BuildLayout.DataFromOtherAsset other;
            public ulong size => asset?.StreamedSize ?? other.StreamedSize;

            public AssetItem()
            {
                supportsSearch = true;
            }

            public override object GetObject()
            {
                return asset != null ? asset : other;
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
                        return size.CompareTo(otherItem.size);
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column, bool selected)
            {
                var text = ToString(column);
                switch (column)
                {
                    case ColumnIDs.name:
                        if (GUI.Button(ButtonSpaceR(ref position), CachedGUIContent(Styles.navigateIcon, "Navigate to asset"), Styles.iconButtonStyle))
                            NavigateTo(GetObject());

                        LabelField(position, text);
                        break;

                    case ColumnIDs.size:
                        LabelField(position, text);
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
                        return EditorUtility.FormatBytes((long)size);
                }

                return base.ToString(column);
            }
        }

        class GroupItem : BaseItem
        {
            public BuildLayout.Group group;
            public long size => -1; // TODO

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
                        return string.Compare(group.Name, otherItem.group.Name, true);

                    case ColumnIDs.size:
                        return size.CompareTo(otherItem.size);
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column, bool selected)
            {
                var text = ToString(column);
                switch (column)
                {
                    case ColumnIDs.name:
                        if (GUI.Button(ButtonSpaceR(ref position), CachedGUIContent(Styles.navigateIcon, "Navigate to group"), Styles.iconButtonStyle))
                            NavigateTo(GetObject());

                        LabelField(position, text);
                        break;

                    case ColumnIDs.size:
                        LabelField(position, text);
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
                        return EditorUtility.FormatBytes(size);
                }

                return base.ToString(column);
            }
        }
    }
}
