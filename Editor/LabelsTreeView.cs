using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.Layout;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    public class LabelsTreeView : BuildLayoutTreeView
    {
        static class ColumnIDs
        {
            public const int name = 0;
            public const int size = 1;
            public const int bundles = 2;
            public const int otherLabels = 3;
        }

        Dictionary<string, HashSet<string>> m_AssetNameToLabelsMap = new Dictionary<string, HashSet<string>>();

        public LabelsTreeView(BuildLayoutWindow window)
            : base(window, new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[] {
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Name"), width = 250, autoResize = true },
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Size"), width = 80, autoResize = true },
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Bundles"), width = 80, autoResize = true },
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("OtherLabels"), width = 100, autoResize = true }
            })))
        {
            multiColumnHeader.SetSortDirection(ColumnIDs.name, true);
            multiColumnHeader.SetSortDirection(ColumnIDs.size, false);
            multiColumnHeader.SetSortDirection(ColumnIDs.bundles, false);
            multiColumnHeader.SetSortDirection(ColumnIDs.otherLabels, false);
            multiColumnHeader.sortedColumnIndex = ColumnIDs.size;

            FillLabels();
        }

        void FillLabels()
        {
            if (AddressableAssetSettingsDefaultObject.Settings == null)
                return;

            foreach (var group in AddressableAssetSettingsDefaultObject.Settings.groups)
            {
                if (group == null)
                    continue;

                foreach (var entry in group.entries)
                {
                    if (entry == null || entry.labels.Count == 0)
                        continue;

                    m_AssetNameToLabelsMap[entry.AssetPath] = entry.labels;
                }
            }
        }

        protected override void OnBuildTree(TreeViewItem rootItem, BuildLayout buildLayout)
        {
            var labelToBundlesMap = new Dictionary<string, HashSet<BuildLayout.Bundle>>();
            var bundlesToLabelsMap = new Dictionary<BuildLayout.Bundle, HashSet<string>>();
            var assets = new HashSet<BuildLayout.ExplicitAsset>();
            
            foreach (var group in buildLayout.Groups)
            foreach (var bundle in group.Bundles)
            foreach (var file in bundle.Files)
            foreach (var asset in file.Assets)
                assets.Add(asset);

            foreach (var asset in assets)
            {
                foreach (var label in asset.Labels)
                {
                    if (!labelToBundlesMap.TryGetValue(label, out var bundleList))
                        labelToBundlesMap[label] = bundleList = new HashSet<BuildLayout.Bundle>();
                    bundleList.Add(asset.Bundle);

                    if (!bundlesToLabelsMap.TryGetValue(asset.Bundle, out var labelsList))
                        bundlesToLabelsMap[asset.Bundle] = labelsList = new ();
                    labelsList.Add(label);
                }
            }

            foreach (var pair in labelToBundlesMap)
            {
                var labelItem = new LabelItem
                {
                    treeView = this,
                    bundles = pair.Value.ToList(),
                    id = m_UniqueId++,
                    depth = 0,
                    displayName = pair.Key,
                    icon = Styles.labelIcon
                };
                rootItem.AddChild(labelItem);

                foreach (var bundle in pair.Value)
                {
                    var bundleItem = new BundleItem
                    {
                        treeView = this,
                        bundle = bundle,
                        otherLabels = bundlesToLabelsMap[bundle].Except(new[] { pair.Key }).ToList(),
                        id = m_UniqueId++,
                        depth = labelItem.depth + 1,
                        displayName = Utility.TransformBundleName(bundle.Name),
                        icon = Styles.GetBuildLayoutObjectIcon(bundle)
                    };
                    labelItem.AddChild(bundleItem);
                }
            }

        }

        [System.Serializable]
        class LabelItem : BaseItem
        {
            public string label;
            public List<BuildLayout.Bundle> bundles;

            public LabelItem()
            {
                supportsSearch = true;
            }

            public override object GetObject()
            {
                return label;
            }

            public override int CompareTo(TreeViewItem other, int column)
            {
                var otherItem = other as LabelItem;
                if (otherItem == null)
                    return 1;

                switch (column)
                {
                    case ColumnIDs.name:
                        return string.Compare(label, otherItem.label, true);

                    case ColumnIDs.size:
                        return bundles.Sum(b => (long)b.FileSize).CompareTo(otherItem.bundles.Sum(b => (long)b.FileSize));

                    case ColumnIDs.bundles:
                        return bundles.Count.CompareTo(otherItem.bundles.Count);
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

                    case ColumnIDs.bundles:
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
                        return EditorUtility.FormatBytes(bundles.Sum(b => (long)b.FileSize));

                    case ColumnIDs.bundles:
                        return $"{bundles.Count}";
                }

                return base.ToString(column);
            }
        }

        [System.Serializable]
        class BundleItem : BaseItem
        {
            public BuildLayout.Bundle bundle;

            public List<string> otherLabels;

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

                    case ColumnIDs.otherLabels:
                        return otherLabels.Count.CompareTo(otherItem.otherLabels.Count);

                    case ColumnIDs.size:
                        return bundle.FileSize.CompareTo(otherItem.bundle.FileSize);

                    case ColumnIDs.bundles:
                        return 0;
                }

                return 0;
            }

            public override void OnGUI(Rect position, int column, bool selected)
            {
                var text = ToString(column);
                switch (column)
                {
                    case ColumnIDs.name:
                        if (GUI.Button(ButtonSpaceR(ref position), CachedGUIContent(Styles.navigateIcon, "Navigate to bundle"), Styles.iconButtonStyle))
                            NavigateTo(bundle);
                        LabelField(position, text);
                        break;

                    case ColumnIDs.otherLabels:
                        LabelField(position, text);
                        break;

                    case ColumnIDs.size:
                        LabelField(position, text);
                        break;

                    case ColumnIDs.bundles:
                        LabelField(position, text, true);
                        break;
                }
            }

            public override string ToString(int column)
            {
                switch (column)
                {
                    case ColumnIDs.name:
                        return displayName;

                    case ColumnIDs.otherLabels:
                        if (otherLabels.Count > 0)
                            return string.Join(",", otherLabels);
                        return string.Empty;

                    case ColumnIDs.size:
                        return EditorUtility.FormatBytes((long)bundle.FileSize);

                    case ColumnIDs.bundles:
                        return "1";
                }

                return base.ToString(column);
            }
        }
    }
}
