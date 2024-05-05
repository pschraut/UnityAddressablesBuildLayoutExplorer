//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2024 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Build.Layout;
using NUnit.Framework;
using System.Collections;
using UnityEditor.VersionControl;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    public class AssetDuplicatesTreeView : BuildLayoutTreeView
    {
        static class ColumnIDs
        {
            public const int name = 0;
            public const int size = 1;
            public const int count = 2;
        }

        const string kAssetSizeTooltip = "Combined uncompressed asset size";

        public AssetDuplicatesTreeView(BuildLayoutWindow window)
                   : base(window, new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[] {
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Name"), width = 250, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Size"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Count"), width = 80, autoResize = true },
                            })))
        {
            multiColumnHeader.SetSortDirection(ColumnIDs.name, true);
            multiColumnHeader.SetSortDirection(ColumnIDs.size, false);
            multiColumnHeader.SetSortDirection(ColumnIDs.count, false);
            multiColumnHeader.sortedColumnIndex = ColumnIDs.size;
        }

        public TreeViewItem FindItem(BuildLayout.ExplicitAsset asset)
        {
            TreeViewItem result = null;

            IterateItems(delegate (TreeViewItem i)
            {
                var b = i as BundleItem;
                if (b == null)
                    return false;

                if (b.bundle != asset.Bundle)
                    return false;

                result = b;
                return true;
            });

            return result;
        }

        protected override void OnBuildTree(TreeViewItem rootItem, BuildLayout buildLayout)
        {
            //foreach (var asset in buildLayout.DuplicatedAssets)
            //{
            //    var assetPath = asset.AssetGuid;
                
            //    var categoryItem = new CategoryItem()
            //    {
            //        treeView = this,
            //        bundles = asset.DuplicatedObjects,
            //        path = assetPath,
            //        id = m_UniqueId++,
            //        depth = rootItem.depth + 1,
            //        displayName = assetPath,
            //        icon = Styles.assetIcon
            //    };
            //    rootItem.AddChild(categoryItem);

            //    foreach (var duplicate in asset.DuplicatedObjects)
            //    {
            //        foreach (var file in duplicate.IncludedInBundleFiles)
            //        {
            //            categoryItem.size += (long)file.Bundle.FileSize;

            //            var bundleItem = new BundleItem
            //            {
            //                treeView = this,
            //                bundle = file.Bundle,
            //                id = m_UniqueId++,
            //                depth = categoryItem.depth + 1,
            //                displayName = Utility.TransformBundleName(file.Bundle.Name),
            //                icon = Styles.GetBuildLayoutObjectIcon(file.Bundle)
            //            };
            //            categoryItem.AddChild(bundleItem);
            //        }
            //    }
            //}

            var sizeLookup = new Dictionary<string, long>();
            var bundleLookup = new Dictionary<string, HashSet<BuildLayout.Bundle>>();

            foreach (var group in buildLayout.Groups) foreach (var bundle in group.Bundles) foreach (var file in bundle.Files)
            {
                foreach(var asset in file.OtherAssets)
                {
                    if (!bundleLookup.TryGetValue(asset.AssetGuid, out var list))
                    {
                        sizeLookup[asset.AssetGuid] = 0;
                        bundleLookup[asset.AssetGuid] = list = new HashSet<BuildLayout.Bundle>();
                    }

                    list.Add(bundle);
                    sizeLookup[asset.AssetGuid] += (long)asset.StreamedSize;
                }

                foreach (var asset in file.Assets)
                {
                    if (!bundleLookup.TryGetValue(asset.Guid, out var list))
                    {
                        sizeLookup[asset.Guid] = 0;
                        bundleLookup[asset.Guid] = list = new HashSet<BuildLayout.Bundle>();
                    }

                    list.Add(bundle);
                    sizeLookup[asset.Guid] += (long)asset.StreamedSize;
                }
            }

            foreach (var pair in bundleLookup)
            {
                var list = pair.Value;
                if (list.Count < 2)
                    continue;

                var categoryItem = new CategoryItem()
                {
                    treeView = this,
                    count = list.Count,
                    path = pair.Key,
                    id = m_UniqueId++,
                    depth = rootItem.depth + 1,
                    displayName = pair.Key,
                    icon = Styles.assetIcon,
                    size = sizeLookup[pair.Key]
                };
                rootItem.AddChild(categoryItem);

                foreach (var bundle in list)
                {
                    var bundleItem = new BundleItem
                    {
                        treeView = this,
                        bundle = bundle,
                        id = m_UniqueId++,
                        depth = categoryItem.depth + 1,
                        displayName = Utility.TransformBundleName(bundle.Name),
                        icon = Styles.GetBuildLayoutObjectIcon(bundle)
                    };
                    categoryItem.AddChild(bundleItem);
                }
            }
        }


        [System.Serializable]
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
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column, bool selected)
            {
                switch(column)
                {
                    case ColumnIDs.name:
                        if (GUI.Button(ButtonSpaceR(ref position), CachedGUIContent(Styles.navigateIcon, "Navigate to bundle"), Styles.iconButtonStyle))
                            NavigateTo(bundle);

                        LabelField(position, displayName);
                        break;
                }
            }

            public override string ToString(int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        return displayName;
                }

                return base.ToString(column);
            }
        }

        [System.Serializable]
        class CategoryItem : BaseItem
        {
            public long count;
            public long size;
            public string path;

            public CategoryItem()
            {
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

                switch (column)
                {
                    case ColumnIDs.name:
                        return string.Compare(displayName, otherItem.displayName, true);

                    case ColumnIDs.size:
                        return size.CompareTo(otherItem.size);

                    case ColumnIDs.count:
                        return count.CompareTo(otherItem.count);
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column, bool selected)
            {
                var text = ToString(column);
                switch (column)
                {
                    case ColumnIDs.name:
                        if (GUI.Button(ButtonSpaceR(ref position), CachedGUIContent(Styles.selectAssetIcon, "Select asset in project (double click)"), Styles.iconButtonStyle))
                            TrySelectAsset(path);

                        LabelField(position, text);
                        break;

                    case ColumnIDs.count:
                        LabelField(position, text);
                        break;

                    case ColumnIDs.size:
                        LabelField(position, CachedGUIContent(text, kAssetSizeTooltip));
                        break;
                }
            }

            public override void OnDoubleClick()
            {
                base.OnDoubleClick();

                TrySelectAsset(path);
            }

            public override string ToString(int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        return displayName;

                    case ColumnIDs.count:
                        return $"{count}";

                    case ColumnIDs.size:
                        return EditorUtility.FormatBytes(size);
                }

                return base.ToString(column);
            }
        }
    }
}
