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
        }

        public BundleTreeView(BuildLayoutWindow window)
                   : base(window, new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[] {
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Name"), width = 250, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Size"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Compression"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Dependencies"), width = 80, autoResize = true },
                            })))
        {
            multiColumnHeader.sortedColumnIndex = ColumnIDs.size;
        }

        protected override void OnBuildTree(TreeViewItem rootItem, RichBuildLayout buildLayout)
        {
            var processed = new Dictionary<string, bool>();

            foreach (var group in buildLayout.groups)
            {
                foreach(var bundle in group.bundles)
                {
                    if (processed.ContainsKey(bundle.name))
                        continue;

                    var bundleItem = new BundleItem
                    {
                        treeView = this,
                        bundle = bundle,
                        id = m_UniqueId++,
                        depth = 0,
                        displayName = bundle.name,
                        icon = Styles.bundleIcon
                    };
                    rootItem.AddChild(bundleItem);

                    if (bundle.explicitAssets.Count > 0)
                    {
                        var assetsCategoryItem = new CategoryItem
                        {
                            treeView = this,
                            id = m_UniqueId++,
                            depth = bundleItem.depth + 1,
                            displayName = "Explicit Assets",
                            icon = Styles.explicitAssetsIcon
                        };
                        bundleItem.AddChild(assetsCategoryItem);

                        foreach (var asset in bundle.explicitAssets)
                        {
                            var assetItem = new AssetItem
                            {
                                asset = asset,
                                id = m_UniqueId++,
                                depth = assetsCategoryItem.depth + 1,
                                displayName = asset.name
                            };
                            assetsCategoryItem.AddChild(assetItem);

                            if (asset.internalReferences.Count > 0)
                            {
                                var irefCategoryItem = new CategoryItem
                                {
                                    id = m_UniqueId++,
                                    depth = assetItem.depth + 1,
                                    displayName = "Internal References",
                                    icon = Styles.internalAssetReferenceIcon,
                                    sortValue = 1
                                };
                                assetItem.AddChild(irefCategoryItem);

                                foreach (var eref in asset.internalReferences)
                                {
                                    var erefItem = new AssetReferenceItem
                                    {
                                        id = m_UniqueId++,
                                        depth = irefCategoryItem.depth + 1,
                                        displayName = eref
                                    };
                                    irefCategoryItem.AddChild(erefItem);
                                }
                            }

                            if (asset.externalReferences.Count > 0)
                            {
                                var erefCategoryItem = new CategoryItem
                                {
                                    id = m_UniqueId++,
                                    depth = assetItem.depth + 1,
                                    displayName = "External References",
                                    icon = Styles.externalAssetReferenceIcon,
                                    sortValue = 2
                                };
                                assetItem.AddChild(erefCategoryItem);

                                foreach (var eref in asset.externalReferences)
                                {
                                    var erefItem = new AssetReferenceItem
                                    {
                                        id = m_UniqueId++,
                                        depth = erefCategoryItem.depth + 1,
                                        displayName = eref
                                    };
                                    erefCategoryItem.AddChild(erefItem);
                                }
                            }
                        }
                    }

                    if (bundle.bundleDependencies.Count > 0)
                    {
                        var categoryItem = new CategoryItem
                        {
                            treeView = this,
                            id = m_UniqueId++,
                            depth = bundleItem.depth + 1,
                            displayName = "Bundle Dependencies",
                            icon = Styles.bundleDependenciesIcon
                        };
                        bundleItem.AddChild(categoryItem);

                        foreach (var dependency in bundle.bundleDependencies)
                        {
                            var dependencyItem = new BundleReferenceItem
                            {
                                treeView = this,
                                bundle = dependency,
                                id = m_UniqueId++,
                                depth = categoryItem.depth + 1,
                                displayName = dependency.name
                            };
                            categoryItem.AddChild(dependencyItem);
                        }
                    }

                    if (bundle.expandedBundleDependencies.Count > 0)
                    {
                        var categoryItem = new CategoryItem
                        {
                            treeView = this,
                            id = m_UniqueId++,
                            depth = bundleItem.depth + 1,
                            displayName = "Expanded Bundle Dependencies",
                            icon = Styles.bundleExpandedDependenciesIcon
                        };
                        bundleItem.AddChild(categoryItem);

                        foreach (var dependency in bundle.expandedBundleDependencies)
                        {
                            var dependencyItem = new BundleReferenceItem
                            {
                                treeView = this,
                                bundle = dependency,
                                id = m_UniqueId++,
                                depth = categoryItem.depth + 1,
                                displayName = dependency.name
                            };
                            categoryItem.AddChild(dependencyItem);
                        }
                    }
                }
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
                        return bundle.bundleDependencies.Count.CompareTo(otherItem.bundle.bundleDependencies.Count);
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column)
            {
                switch(column)
                {
                    case ColumnIDs.name:
                        EditorGUI.LabelField(position, bundle.name);
                        break;

                    case ColumnIDs.size:
                        EditorGUI.LabelField(position, $"{EditorUtility.FormatBytes(bundle.size)}");
                        break;

                    case ColumnIDs.compression:
                        EditorGUI.LabelField(position, bundle.compression);
                        break;

                    case ColumnIDs.dependencies:
                        var dependencyCount = bundle.bundleDependencies.Count + bundle.expandedBundleDependencies.Count;
                        EditorGUI.LabelField(position, $"{dependencyCount}");
                        break;
                }
            }
        }


        [System.Serializable]
        class CategoryItem : BaseItem
        {
            public int sortValue;

            public CategoryItem()
            {
                supportsSortingOrder = false;
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
                }
            }
        }

        [System.Serializable]
        class AssetItem : BaseItem
        {
            public RichBuildLayout.Asset asset;

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

            public override void OnGUI(Rect position, int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        EditorGUI.LabelField(position, asset.name);
                        break;

                    case ColumnIDs.size:
                        EditorGUI.LabelField(position, EditorUtility.FormatBytes(asset.size), Styles.ghostLabelStyle);
                        break;
                }
            }
        }

        [System.Serializable]
        class AssetReferenceItem : BaseItem
        {
            public override int CompareTo(TreeViewItem other, int column)
            {
                var otherItem = other as AssetReferenceItem;
                if (otherItem == null)
                    return 1;

                switch (column)
                {
                    case ColumnIDs.name:
                        return string.Compare(displayName, otherItem.displayName, true);
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        EditorGUI.LabelField(position, displayName);
                        break;
                }
            }
        }

        [System.Serializable]
        class BundleReferenceItem : BaseItem
        {
            public RichBuildLayout.Archive bundle;

            public override int CompareTo(TreeViewItem other, int column)
            {
                var otherItem = other as BundleReferenceItem;
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
                        {
                            if (GUI.Button(SpaceR(ref position, 20), CachedGUIContent(Styles.bundleIcon, "Jump to bundle")))
                                JumpToBundleItem();

                            EditorGUI.LabelField(position, displayName);
                        }
                        break;

                    case ColumnIDs.size:
                        EditorGUI.LabelField(position, EditorUtility.FormatBytes(bundle.size), Styles.ghostLabelStyle);
                        break;

                    case ColumnIDs.compression:
                        EditorGUI.LabelField(position, bundle.compression, Styles.ghostLabelStyle);
                        break;

                    case ColumnIDs.dependencies:
                        var dependencyCount = bundle.bundleDependencies.Count + bundle.expandedBundleDependencies.Count;
                        EditorGUI.LabelField(position, $"{dependencyCount}", Styles.ghostLabelStyle);
                        break;
                }
            }

            void JumpToBundleItem()
            {
                BundleItem jump = null;

                // find the bundle in question
                treeView.IterateItems(delegate (TreeViewItem i)
                {
                    var b = i as BundleItem;
                    if (b == null)
                        return false;

                    if (!string.Equals(b.displayName, displayName, System.StringComparison.OrdinalIgnoreCase))
                        return false;

                    jump = b;
                    return true;
                });

                if (jump == null)
                    return;

                treeView.SetSelection(new[] { jump.id }, TreeViewSelectionOptions.RevealAndFrame | TreeViewSelectionOptions.FireSelectionChanged);
            }
        }
    }
}
