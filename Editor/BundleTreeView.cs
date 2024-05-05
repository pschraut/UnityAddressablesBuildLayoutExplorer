//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2024 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Build.Layout;
using UnityEditor.IMGUI.Controls;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    public class BundleTreeView : BuildLayoutTreeView
    {
        static class ColumnIDs
        {
            public const int name = 0;
            public const int size = 1;
            public const int uncompressedSize = 2;
            public const int compression = 3;
            public const int dependencies = 4;
            public const int referencedByBundles = 5;
        }

        const string kAssetSizeTooltip = "Uncompressed asset size";
        const string kCompressionTooltip = "LZMA should be used for remote-content\nLZ4HC should be used for local-content";

        public BundleTreeView(BuildLayoutWindow window)
                   : base(window, new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[] {
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Name"), width = 250, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Size"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Uncompressed Size"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Compression"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Refs To"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Refs By"), width = 80, autoResize = true },
                            })))
        {
            multiColumnHeader.SetSortDirection(ColumnIDs.name, true);
            multiColumnHeader.SetSortDirection(ColumnIDs.size, false);
            multiColumnHeader.SetSortDirection(ColumnIDs.uncompressedSize, false);
            multiColumnHeader.SetSortDirection(ColumnIDs.compression, true);
            multiColumnHeader.SetSortDirection(ColumnIDs.dependencies, false);
            multiColumnHeader.SetSortDirection(ColumnIDs.referencedByBundles, false);
            multiColumnHeader.sortedColumnIndex = ColumnIDs.size;
        }

        public TreeViewItem FindItem(BuildLayout.Bundle bundle)
        {
            TreeViewItem result = null;

            IterateItems(delegate (TreeViewItem i)
            {
                var b = i as BundleItem;
                if (b == null)
                    return false;

                if (b.bundle != bundle)
                    return false;

                result = b;
                return true;
            });

            return result;
        }

        protected override void OnBuildTree(TreeViewItem rootItem, BuildLayout buildLayout)
        {
            var bundles = new HashSet<BuildLayout.Bundle>();
            bundles.UnionWith(buildLayout.BuiltInBundles);
            foreach (var group in buildLayout.Groups)
                bundles.UnionWith(group.Bundles);

            foreach (var bundle in bundles)
            {
                var bundleItem = new BundleItem
                {
                    treeView = this,
                    bundle = bundle,
                    id = m_UniqueId++,
                    depth = 0,
                    displayName = Utility.TransformBundleName(bundle.Name),
                    icon = Styles.GetBuildLayoutObjectIcon(bundle)
                };
                rootItem.AddChild(bundleItem);

                foreach (var file in bundle.Files)
                {
                    foreach (var asset in file.OtherAssets)
                    {
                        var assetItem = new AssetItem
                        {
                            treeView = this,
                            other = asset,
                            id = m_UniqueId++,
                            depth = bundleItem.depth + 1,
                            displayName = Utility.TransformBundleName(asset.AssetPath),
                            icon = Styles.GetBuildLayoutObjectIcon(asset),
                            ghosted = true
                        };
                        bundleItem.AddChild(assetItem);
                    }

                    foreach (var asset in file.Assets)
                    {
                        var assetItem = new AssetItem
                        {
                            treeView = this,
                            asset = asset,
                            id = m_UniqueId++,
                            depth = bundleItem.depth + 1,
                            displayName = Utility.TransformBundleName(asset.AssetPath),
                            icon = Styles.GetBuildLayoutObjectIcon(asset)
                        };
                        bundleItem.AddChild(assetItem);
                    }
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

                    case ColumnIDs.size:
                        return bundle.FileSize.CompareTo(otherItem.bundle.FileSize);

                    case ColumnIDs.uncompressedSize:
                        return bundle.UncompressedFileSize.CompareTo(otherItem.bundle.UncompressedFileSize);

                    case ColumnIDs.compression:
                        return string.Compare(bundle.Compression, otherItem.bundle.Compression, true);

                    case ColumnIDs.dependencies:
                        {
                            var a = bundle.Dependencies.Count + bundle.ExpandedDependencies.Count;
                            var b = otherItem.bundle.Dependencies.Count + otherItem.bundle.ExpandedDependencies.Count;
                            return a.CompareTo(b);
                        }

                    case ColumnIDs.referencedByBundles:
                        return bundle.DependentBundles.Count.CompareTo(otherItem.bundle.DependentBundles.Count);
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column, bool selected)
            {
                var text = ToString(column);
                switch (column)
                {
                    case ColumnIDs.name:
                    case ColumnIDs.size:
                    case ColumnIDs.uncompressedSize:
                    case ColumnIDs.dependencies:
                    case ColumnIDs.referencedByBundles:
                        LabelField(position, text);
                        break;

                    case ColumnIDs.compression:
                        LabelField(position, CachedGUIContent(text, kCompressionTooltip));
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

                    case ColumnIDs.uncompressedSize:
                        return EditorUtility.FormatBytes((long)bundle.UncompressedFileSize);

                    case ColumnIDs.compression:
                        return bundle.Compression;

                    case ColumnIDs.dependencies:
                        var dependencyCount = bundle.Dependencies.Count + bundle.ExpandedDependencies.Count;
                        return $"{dependencyCount}";

                    case ColumnIDs.referencedByBundles:
                        return $"{bundle.DependentBundles.Count}";
                }

                return base.ToString(column);
            }
        }

#if false
        [System.Serializable]
        class FileItem : BaseItem
        {
            public BuildLayout.File file;

            public FileItem()
            {
                supportsSearch = false;
                supportsSortingOrder = false;
            }

            public override object GetObject()
            {
                return file;
            }

            public override int CompareTo(TreeViewItem other, int column)
            {
                var otherItem = other as AssetItem;
                if (otherItem == null)
                    return 1;

                switch (column)
                {
                    case ColumnIDs.name:
                        return string.Compare(displayName, otherItem.displayName);
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
#endif

        [System.Serializable]
        class AssetItem : BaseItem
        {
            public BuildLayout.ExplicitAsset asset;
            public BuildLayout.DataFromOtherAsset other;
            public bool ghosted;

            string address => asset?.AddressableName ?? "";
            string assetPath => asset?.AssetPath ?? other.AssetPath;
            ulong serializedSize => asset?.SerializedSize ?? other.SerializedSize;
            ulong streamedSize => asset?.StreamedSize ?? other.StreamedSize;
            ulong uncompressedSize => streamedSize > 0 ? streamedSize : serializedSize;

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

                    case ColumnIDs.uncompressedSize:
                        return uncompressedSize.CompareTo(otherItem.uncompressedSize);
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
                            NavigateTo(asset != null ? asset : other);

                        if (GUI.Button(ButtonSpaceR(ref position), CachedGUIContent(Styles.selectAssetIcon, "Select asset in project (double click)"), Styles.iconButtonStyle))
                            TrySelectAsset(assetPath);

                        LabelField(position, text, ghosted);
                        break;

                    case ColumnIDs.uncompressedSize:
                        LabelField(position, CachedGUIContent(text, kAssetSizeTooltip), ghosted);
                        break;
                }
            }

            public override void OnDoubleClick()
            {
                base.OnDoubleClick();

                TrySelectAsset(assetPath);
            }

            public override string ToString(int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        return displayName;

                    case ColumnIDs.uncompressedSize:
                        return EditorUtility.FormatBytes((long)uncompressedSize);
                }

                return base.ToString(column);
            }
        }
    }
}
