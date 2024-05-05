//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2024 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using UnityEditor;
using UnityEditor.AddressableAssets.Build.Layout;
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

        public TreeViewItem FindItem(object asset)
        {
            TreeViewItem result = null;

            IterateItems(delegate (TreeViewItem i)
            {
                var b = i as AssetItem;
                if (b == null)
                    return false;

                if (b.asset != asset && b.other != asset)
                    return false;

                result = b;
                return true;
            });

            return result;
        }

        protected override void OnBuildTree(TreeViewItem rootItem, BuildLayout buildLayout)
        {
            foreach (var group in buildLayout.Groups)
                foreach (var bundle in group.Bundles)
                    foreach (var file in bundle.Files)
                    {
                        foreach (var asset in file.OtherAssets)
                        {
                            var assetItem = new AssetItem
                            {
                                treeView = this,
                                other = asset,
                                id = m_UniqueId++,
                                depth = 0,
                                displayName = Utility.TransformBundleName(asset.AssetPath),
                                icon = Styles.GetBuildLayoutObjectIcon(asset)
                            };
                            rootItem.AddChild(assetItem);
                        }

                        foreach (var asset in file.Assets)
                        {
                            var assetItem = new AssetItem
                            {
                                treeView = this,
                                asset = asset,
                                id = m_UniqueId++,
                                depth = 0,
                                displayName = Utility.TransformBundleName(asset.AssetPath),
                                icon = Styles.GetBuildLayoutObjectIcon(asset)
                            };
                            rootItem.AddChild(assetItem);

                            foreach (var internalReference in asset.InternalReferencedExplicitAssets)
                            {
                                var assetReference = new AssetItem()
                                {
                                    treeView = this,
                                    asset = internalReference,
                                    ghosted = true,
                                    id = m_UniqueId++,
                                    depth = assetItem.depth + 1,
                                    displayName = Utility.TransformBundleName(internalReference.AssetPath),
                                    icon = Styles.GetBuildLayoutObjectIcon(internalReference)
                                };
                                assetItem.AddChild(assetReference);
                            }

                            // foreach (var internalReference in asset.InternalReferencedOtherAssets) // TODO

                        }
                    }
        }


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
                        return string.Compare(assetPath, otherItem.assetPath, true);

                    case ColumnIDs.size:
                        return serializedSize.CompareTo(otherItem.serializedSize);

                    case ColumnIDs.address:
                        return string.Compare(asset.AddressableName, otherItem.asset.AddressableName, System.StringComparison.OrdinalIgnoreCase);

                    case ColumnIDs.sizeFromObjects:
                        return serializedSize.CompareTo(otherItem.serializedSize);

                    case ColumnIDs.sizeFromStreamedData:
                        return streamedSize.CompareTo(otherItem.streamedSize);

                }

                return 0;
            }

            public override void OnGUI(Rect position, int column, bool selected)
            {
                var text = ToString(column);
                switch(column)
                {
                    case ColumnIDs.name:
                        if (GUI.Button(ButtonSpaceR(ref position), CachedGUIContent(Styles.selectAssetIcon, "Select asset in project (double click)"), Styles.iconButtonStyle))
                            TrySelectAsset(assetPath);

                        LabelField(position, text);
                        break;

                    case ColumnIDs.size:
                        LabelField(position, CachedGUIContent(text, kAssetSizeTooltip), ghosted);
                        break;

                    case ColumnIDs.address:
                        LabelField(position, text, ghosted);
                        break;

                    case ColumnIDs.sizeFromObjects:
                        LabelField(position, text, ghosted);
                        break;

                    case ColumnIDs.sizeFromStreamedData:
                        LabelField(position, text, ghosted);
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

                    case ColumnIDs.size:
                        return EditorUtility.FormatBytes((long)serializedSize);

                    case ColumnIDs.address:
                        return address;

                    case ColumnIDs.sizeFromObjects:
                        return EditorUtility.FormatBytes((long)serializedSize);

                    case ColumnIDs.sizeFromStreamedData:
                        return EditorUtility.FormatBytes((long)streamedSize);
                }

                return base.ToString(column);
            }
        }
    }
}
