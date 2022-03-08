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
    public class BundleTreeView : BuildLayoutTreeView
    {
        static class ColumnIDs
        {
            public const int name = 0;
            public const int size = 1;
            public const int compression = 2;
            public const int dependencies = 3;
            public const int referencedByBundles = 4;
        }

        const string kAssetSizeTooltip = "Uncompressed asset size";
        const string kCompressionTooltip = "LZMA should be used for remote-content\nLZ4HC should be used for local-content";

        public BundleTreeView(BuildLayoutWindow window)
                   : base(window, new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[] {
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Name"), width = 250, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Size"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Compression"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("References to Bundles"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Referenced by Bundles"), width = 80, autoResize = true },
                            })))
        {
            multiColumnHeader.SetSortDirection(ColumnIDs.name, true);
            multiColumnHeader.SetSortDirection(ColumnIDs.size, false);
            multiColumnHeader.SetSortDirection(ColumnIDs.compression, true);
            multiColumnHeader.SetSortDirection(ColumnIDs.dependencies, false);
            multiColumnHeader.SetSortDirection(ColumnIDs.referencedByBundles, false);
            multiColumnHeader.sortedColumnIndex = ColumnIDs.size;
        }

        public TreeViewItem FindItem(RichBuildLayout.Archive bundle)
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

        protected override void OnBuildTree(TreeViewItem rootItem, RichBuildLayout buildLayout)
        {
            foreach(var bundle in buildLayout.bundles.Values)
            {
                var bundleItem = new BundleItem
                {
                    treeView = this,
                    bundle = bundle,
                    id = m_UniqueId++,
                    depth = 0,
                    displayName = Utility.TransformBundleName(bundle.name),
                    icon = Styles.GetBuildLayoutObjectIcon(bundle)
                };
                rootItem.AddChild(bundleItem);

                foreach (var asset in bundle.allAssets)
                {
                    var assetItem = new AssetItem
                    {
                        treeView = this,
                        asset = asset,
                        id = m_UniqueId++,
                        depth = bundleItem.depth + 1,
                        displayName = Utility.TransformBundleName(asset.name),
                        icon = Styles.GetBuildLayoutObjectIcon(asset)
                    };
                    bundleItem.AddChild(assetItem);
                }

                //foreach (var asset in bundle.explicitAssets)
                //{
                //    var assetItem = new AssetItem
                //    {
                //        treeView = this,
                //        asset = asset,
                //        id = m_UniqueId++,
                //        depth = bundleItem.depth + 1,
                //        displayName = Utility.TransformBundleName(asset.name),
                //        icon = Styles.GetBuildLayoutObjectIcon(asset)
                //    };
                //    bundleItem.AddChild(assetItem);

                //    foreach(var internalReference in asset.internalReferences)
                //    {
                //        var assetReference = new AssetItem()
                //        {
                //            treeView = this,
                //            asset = internalReference,
                //            id = m_UniqueId++,
                //            depth = assetItem.depth + 1,
                //            displayName = Utility.TransformBundleName(internalReference.name),
                //            //icon = Styles.GetBuildLayoutObjectIcon(internalReference)
                //        };
                //        assetItem.AddChild(assetReference);
                //    }
                //}
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

                    case ColumnIDs.compression:
                        return string.Compare(bundle.compression, otherItem.bundle.compression, true);

                    case ColumnIDs.dependencies:
                        {
                            var a = bundle.bundleDependencies.Count + bundle.expandedBundleDependencies.Count;
                            var b = otherItem.bundle.bundleDependencies.Count + otherItem.bundle.expandedBundleDependencies.Count;
                            return a.CompareTo(b);
                        }

                    case ColumnIDs.referencedByBundles:
                        return bundle.referencedByBundles.Count.CompareTo(otherItem.bundle.referencedByBundles.Count);
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

                    case ColumnIDs.compression:
                        LabelField(position, CachedGUIContent(text, kCompressionTooltip));
                        break;

                    case ColumnIDs.dependencies:
                        LabelField(position, text);
                        break;

                    case ColumnIDs.referencedByBundles:
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
                        return EditorUtility.FormatBytes(bundle.size);

                    case ColumnIDs.compression:
                        return bundle.compression;

                    case ColumnIDs.dependencies:
                        var dependencyCount = bundle.bundleDependencies.Count + bundle.expandedBundleDependencies.Count;
                        return $"{dependencyCount}";

                    case ColumnIDs.referencedByBundles:
                        return $"{bundle.referencedByBundles.Count}";
                }

                return base.ToString(column);
            }
        }

        [System.Serializable]
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
                        return string.Compare(asset.name, otherItem.asset.name, true);

                    case ColumnIDs.size:
                        return asset.size.CompareTo(otherItem.asset.size);
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
                            NavigateTo(asset);

                        if (GUI.Button(ButtonSpaceR(ref position), CachedGUIContent(Styles.selectAssetIcon, "Select asset in project (double click)"), Styles.iconButtonStyle))
                            TrySelectAsset(asset.name);

                        LabelField(position, text);
                        break;

                    case ColumnIDs.size:
                        LabelField(position, CachedGUIContent(text, kAssetSizeTooltip), true);
                        break;
                }
            }

            public override void OnDoubleClick()
            {
                base.OnDoubleClick();

                TrySelectAsset(asset.name);
            }

            public override string ToString(int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        return displayName;

                    case ColumnIDs.size:
                        return EditorUtility.FormatBytes(asset.size);
                }

                return base.ToString(column);
            }
        }
    }
}
