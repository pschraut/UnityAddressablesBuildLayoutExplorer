//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    public class AssetsTreeView : BuildLayoutTreeView
    {
        static class ColumnIDs
        {
            public const int name = 0;
            public const int size = 1;
            public const int sizeFromObjects = 2;
            public const int sizeFromStreamedData = 3;
            public const int address = 4;
        }

        public AssetsTreeView(BuildLayoutWindow window)
                   : base(window, new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[] {
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Name"), width = 250, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Size"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Size from Objects"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Size from StreamedData"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Address"), width = 80, autoResize = true },
                            })))
        {
            multiColumnHeader.sortedColumnIndex = ColumnIDs.size;
        }

        protected override void OnBuildTree(TreeViewItem rootItem, RichBuildLayout buildLayout)
        {
            foreach (var asset in buildLayout.assets)
            {
                var assetItem = new AssetItem
                {
                    treeView = this,
                    asset = asset,
                    id = m_UniqueId++,
                    depth = 0,
                    displayName = asset.name,
                    icon = Styles.explicitAssetsIcon
                };
                rootItem.AddChild(assetItem);

                if (asset.referencedByBundle.Count > 0)
                {
                    var categoryItem = new CategoryItem()
                    {
                        treeView = this,
                        id = m_UniqueId++,
                        depth = assetItem.depth + 1,
                        displayName = "Referenced by Bundle",
                        icon = Styles.bundleDependenciesIcon,
                        sortValue = 1
                    };
                    assetItem.AddChild(categoryItem);

                    foreach(var rbundle in asset.referencedByBundle)
                    {
                        var bundleItem = new BundleItem()
                        {
                            bundle = rbundle,
                            treeView = this,
                            id = m_UniqueId++,
                            depth = categoryItem.depth + 1,
                            displayName = Utility.TransformBundleName(rbundle.name)
                        };
                        categoryItem.AddChild(bundleItem);
                    }
                }
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

                    case ColumnIDs.address:
                        return string.Compare(asset.address, otherItem.asset.address, true);

                    case ColumnIDs.sizeFromObjects:
                        return asset.sizeFromObjects.CompareTo(otherItem.asset.sizeFromObjects);

                    case ColumnIDs.sizeFromStreamedData:
                        return asset.sizeFromStreamedData.CompareTo(otherItem.asset.sizeFromStreamedData);

                }

                return 0;
            }

            public override void OnGUI(Rect position, int column)
            {
                switch(column)
                {
                    case ColumnIDs.name:
                        EditorGUI.LabelField(position, asset.name);
                        break;

                    case ColumnIDs.size:
                        DrawSize(position, asset.size);
                        break;

                    case ColumnIDs.address:
                        EditorGUI.LabelField(position, asset.address);
                        break;

                    case ColumnIDs.sizeFromObjects:
                        DrawSize(position, asset.sizeFromObjects);
                        break;

                    case ColumnIDs.sizeFromStreamedData:
                        DrawSize(position, asset.sizeFromStreamedData);
                        break;
                }

                void DrawSize(Rect r, long size)
                {
                    if (size == 0)
                        EditorGUI.LabelField(position, EditorUtility.FormatBytes(size), Styles.ghostLabelStyle);
                    else
                        EditorGUI.LabelField(position, EditorUtility.FormatBytes(size));
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
                        EditorGUI.LabelField(position, displayName);
                        break;

                    case ColumnIDs.size:
                        EditorGUI.LabelField(position, EditorUtility.FormatBytes(bundle.size), Styles.ghostLabelStyle);
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
    }
}
