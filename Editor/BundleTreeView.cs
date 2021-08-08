//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

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


        protected override void OnBuildTree(TreeViewItem rootItem, BuildLayout buildLayout)
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
                        source = bundle,
                        id = m_UniqueId++,
                        depth = 0,
                        displayName = bundle.name,
                        icon = Styles.bundleIcon
                    };
                    rootItem.AddChild(bundleItem);

                    if (bundle.bundleDependencies.Count > 0)
                    {
                        var categoryItem = new CategoryItem
                        {
                            id = m_UniqueId++,
                            depth = bundleItem.depth + 1,
                            displayName = "Bundle Dependencies",
                            icon = Styles.bundleDependenciesIcon
                        };
                        bundleItem.AddChild(categoryItem);

                        foreach (var dependency in bundle.bundleDependencies)
                        {
                            var dependencyItem = new DependencyItem
                            {
                                source = dependency,
                                id = m_UniqueId++,
                                depth = categoryItem.depth + 1,
                                displayName = dependency
                            };
                            categoryItem.AddChild(dependencyItem);
                        }
                    }

                    if (bundle.expandedBundleDependencies.Count > 0)
                    {
                        var categoryItem = new CategoryItem
                        {
                            id = m_UniqueId++,
                            depth = bundleItem.depth + 1,
                            displayName = "Expanded Bundle Dependencies",
                            icon = Styles.bundleExpandedDependenciesIcon
                        };
                        bundleItem.AddChild(categoryItem);

                        foreach (var dependency in bundle.expandedBundleDependencies)
                        {
                            var dependencyItem = new DependencyItem
                            {
                                source = dependency,
                                id = m_UniqueId++,
                                depth = categoryItem.depth + 1,
                                displayName = dependency,
                                //icon = Styles.bundleIcon
                            };
                            categoryItem.AddChild(dependencyItem);
                        }
                    }

                    if (bundle.explicitAssets.Count > 0)
                    {
                        var categoryItem = new CategoryItem
                        {
                            id = m_UniqueId++,
                            depth = bundleItem.depth + 1,
                            displayName = "Explicit Assets",
                            icon = Styles.explicitAssetsIcon
                        };
                        bundleItem.AddChild(categoryItem);

                        foreach (var asset in bundle.explicitAssets)
                        {
                            var assetItem = new AssetItem
                            {
                                source = asset,
                                id = m_UniqueId++,
                                depth = categoryItem.depth + 1,
                                displayName = asset.name,
                                //icon = Styles.bundleIcon
                            };
                            categoryItem.AddChild(assetItem);
                        }
                    }
                }
            }
        }

        [System.Serializable]
        class CategoryItem : BaseItem
        {
            public override int CompareTo(TreeViewItem other, int column)
            {
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
        class BundleItem : BaseItem
        {
            public BuildLayout.Archive source;

            public override int CompareTo(TreeViewItem other, int column)
            {
                var otherItem = other as BundleItem;
                if (otherItem == null)
                    return 1;

                switch (column)
                {
                    case ColumnIDs.name:
                        return string.Compare(source.name, otherItem.source.name, true);

                    case ColumnIDs.size:
                        return source.size.CompareTo(otherItem.source.size);

                    case ColumnIDs.compression:
                        return string.Compare(source.compression, otherItem.source.compression, true);

                    case ColumnIDs.dependencies:
                        return source.bundleDependencies.Count.CompareTo(otherItem.source.bundleDependencies.Count);
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column)
            {
                switch(column)
                {
                    case ColumnIDs.name:
                        EditorGUI.LabelField(position, source.name);
                        break;

                    case ColumnIDs.size:
                        EditorGUI.LabelField(position, $"{EditorUtility.FormatBytes(source.size)}");
                        break;

                    case ColumnIDs.compression:
                        EditorGUI.LabelField(position, source.compression);
                        break;

                    case ColumnIDs.dependencies:
                        EditorGUI.LabelField(position, $"{source.bundleDependencies.Count}");
                        break;
                }
            }
        }


        [System.Serializable]
        class DependencyItem : BaseItem
        {
            public string source;

            public override int CompareTo(TreeViewItem other, int column)
            {
                var otherItem = other as DependencyItem;
                if (otherItem == null)
                    return 1;

                switch (column)
                {
                    case ColumnIDs.name:
                        return string.Compare(source, otherItem.source, true);
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        EditorGUI.LabelField(position, source);
                        break;
                }
            }
        }

        [System.Serializable]
        class AssetItem : BaseItem
        {
            public BuildLayout.ExplicitAsset source;

            public override int CompareTo(TreeViewItem other, int column)
            {
                var otherItem = other as AssetItem;
                if (otherItem == null)
                    return 1;

                switch (column)
                {
                    case ColumnIDs.name:
                        return string.Compare(source.name, otherItem.source.name, true);
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        EditorGUI.LabelField(position, source.name);
                        break;
                }
            }
        }
    }
}
