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

        const string kAssetSizeTooltip = "Uncompressed asset size";

        public AssetsTreeView(BuildLayoutWindow window)
                   : base(window, new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[] {
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Name"), width = 250, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Size"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Size from Objects"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Size from StreamedData"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Address"), width = 80, autoResize = true },
                            })))
        {
            multiColumnHeader.SetSortDirection(ColumnIDs.name, true);
            multiColumnHeader.SetSortDirection(ColumnIDs.size, false);
            multiColumnHeader.SetSortDirection(ColumnIDs.sizeFromObjects, false);
            multiColumnHeader.SetSortDirection(ColumnIDs.sizeFromStreamedData, false);
            multiColumnHeader.SetSortDirection(ColumnIDs.address, true);
            multiColumnHeader.sortedColumnIndex = ColumnIDs.size;
        }

        public TreeViewItem FindItem(RichBuildLayout.Asset asset)
        {
            TreeViewItem result = null;

            IterateItems(delegate (TreeViewItem i)
            {
                var b = i as AssetItem;
                if (b == null)
                    return false;

                if (b.asset != asset)
                    return false;

                result = b;
                return true;
            });

            return result;
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
                    displayName = Utility.TransformBundleName(asset.name),
                    icon = Styles.GetBuildLayoutObjectIcon(asset)
                };
                rootItem.AddChild(assetItem);

                foreach (var internalReference in asset.internalReferences)
                {
                    var assetReference = new AssetItem()
                    {
                        treeView = this,
                        asset = internalReference,
                        ghosted = true,
                        id = m_UniqueId++,
                        depth = assetItem.depth + 1,
                        displayName = Utility.TransformBundleName(internalReference.name),
                        //icon = Styles.GetBuildLayoutObjectIcon(internalReference)
                    };
                    assetItem.AddChild(assetReference);
                }
            }
        }


        [System.Serializable]
        class AssetItem : BaseItem
        {
            public RichBuildLayout.Asset asset;
            public bool ghosted;

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
                var style = ghosted ? Styles.ghostLabelStyle : EditorStyles.label;
                switch(column)
                {
                    case ColumnIDs.name:
                        EditorGUI.LabelField(position, displayName);
                        break;

                    case ColumnIDs.size:
                        EditorGUI.LabelField(position, CachedGUIContent(EditorUtility.FormatBytes(asset.size), kAssetSizeTooltip), style);
                        break;

                    case ColumnIDs.address:
                        EditorGUI.LabelField(position, asset.address, style);
                        break;

                    case ColumnIDs.sizeFromObjects:
                        EditorGUI.LabelField(position, EditorUtility.FormatBytes(asset.sizeFromObjects), style);
                        break;

                    case ColumnIDs.sizeFromStreamedData:
                        EditorGUI.LabelField(position, EditorUtility.FormatBytes(asset.sizeFromStreamedData), style);
                        break;
                }
            }
        }
    }
}
