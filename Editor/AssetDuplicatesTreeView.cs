//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System.Collections.Generic;

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

        public TreeViewItem FindItem(RichBuildLayout.Asset asset)
        {
            TreeViewItem result = null;

            IterateItems(delegate (TreeViewItem i)
            {
                var b = i as BundleItem;
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
            var lut = new Dictionary<string, List<RichBuildLayout.Asset>>();
            foreach (var asset in buildLayout.assets.Values)
            {
                if (!lut.TryGetValue(asset.name, out var list))
                {
                    list = new List<RichBuildLayout.Asset>();
                    lut.Add(asset.name, list);
                }

                list.Add(asset);
            }

            foreach (var pair in lut)
            {
                var list = pair.Value;
                if (list.Count < 2)
                    continue;

                var categoryItem = new CategoryItem()
                {
                    treeView = this,
                    assets = list,
                    path = pair.Key,
                    id = m_UniqueId++,
                    depth = rootItem.depth + 1,
                    displayName = pair.Key,
                    icon = Styles.assetIcon
                };
                rootItem.AddChild(categoryItem);

                foreach (var asset in list)
                {
                    categoryItem.size += asset.size;

                    var assetItem = new BundleItem
                    {
                        treeView = this,
                        asset = asset,
                        id = m_UniqueId++,
                        depth = categoryItem.depth + 1,
                        displayName = Utility.TransformBundleName(asset.includedInBundle.name),
                        icon = Styles.GetBuildLayoutObjectIcon(asset.includedInBundle)
                    };
                    categoryItem.AddChild(assetItem);
                }
            }
        }


        [System.Serializable]
        class BundleItem : BaseItem
        {
            public RichBuildLayout.Asset asset;

            public BundleItem()
            {
                supportsSearch = true;
            }

            public override object GetObject()
            {
                return asset;
            }

            public override int CompareTo(TreeViewItem other, int column)
            {
                var otherItem = other as BundleItem;
                if (otherItem == null)
                    return 1;

                switch (column)
                {
                    case ColumnIDs.name:
                        return string.Compare(asset.includedInBundle.name, otherItem.asset.includedInBundle.name, true);
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column, bool selected)
            {
                switch(column)
                {
                    case ColumnIDs.name:
                        if (GUI.Button(ButtonSpaceR(ref position), CachedGUIContent(Styles.navigateIcon, "Navigate to bundle"), Styles.iconButtonStyle))
                            NavigateTo(asset.includedInBundle);

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
            public List<RichBuildLayout.Asset> assets;
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
                        return assets.Count.CompareTo(otherItem.assets.Count);
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
                        return $"{assets.Count}";

                    case ColumnIDs.size:
                        return EditorUtility.FormatBytes(size);
                }

                return base.ToString(column);
            }
        }
    }
}
