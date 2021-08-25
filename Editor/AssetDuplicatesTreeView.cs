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
            var lut = new Dictionary<string, List<RichBuildLayout.Asset>>();
            foreach (var asset in buildLayout.assets)
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
                    id = m_UniqueId++,
                    depth = rootItem.depth + 1,
                    displayName = pair.Key,
                    icon = Styles.assetIcon
                };
                rootItem.AddChild(categoryItem);

                foreach (var asset in list)
                {
                    categoryItem.size += asset.size;

                    var assetItem = new AssetItem
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
                        return string.Compare(asset.includedInBundle.name, otherItem.asset.includedInBundle.name, true);
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column)
            {
                switch(column)
                {
                    case ColumnIDs.name:
                        if (GUI.Button(ButtonSpaceR(ref position), CachedGUIContent(Styles.navigateIcon, "Navigate to bundle"), Styles.iconButtonStyle))
                            NavigateTo(asset.includedInBundle);
                        EditorGUI.LabelField(position, displayName);
                        break;
                }
            }
        }

        [System.Serializable]
        class CategoryItem : BaseItem
        {
            public List<RichBuildLayout.Asset> assets;
            public long size;

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

            public override void OnGUI(Rect position, int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        EditorGUI.LabelField(position, displayName);
                        break;

                    case ColumnIDs.count:
                        EditorGUI.LabelField(position, $"{assets.Count}");
                        break;

                    case ColumnIDs.size:
                        EditorGUI.LabelField(position, EditorUtility.FormatBytes(size));
                        break;
                }
            }
        }
    }
}
