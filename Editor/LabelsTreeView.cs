using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
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

        protected override void OnBuildTree(TreeViewItem rootItem, RichBuildLayout buildLayout)
        {
            var labelToBundlesMap = new Dictionary<string, HashSet<RichBuildLayout.Archive>>();
            var bundlesToLabelsMap = new Dictionary<RichBuildLayout.Archive, HashSet<string>>();

            foreach (var bundle in buildLayout.bundles.Values)
            {
                foreach (var asset in bundle.allAssets)
                {
                    if (m_AssetNameToLabelsMap.TryGetValue(asset.name, out var labels))
                    {
                        foreach (var label in labels)
                        {
                            if (!labelToBundlesMap.ContainsKey(label))
                                labelToBundlesMap[label] = new HashSet<RichBuildLayout.Archive>() { bundle };
                            else
                                labelToBundlesMap[label].Add(bundle);
                        }

                        bundlesToLabelsMap[bundle] = labels;
                    }
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
                        displayName = Utility.TransformBundleName(bundle.name),
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
            public List<RichBuildLayout.Archive> bundles;

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
                        return bundles.Sum(b => b.size).CompareTo(otherItem.bundles.Sum(b => b.size));

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
                        return EditorUtility.FormatBytes(bundles.Sum(b => b.size));

                    case ColumnIDs.bundles:
                        return $"{bundles.Count}";
                }

                return base.ToString(column);
            }
        }

        [System.Serializable]
        class BundleItem : BaseItem
        {
            public RichBuildLayout.Archive bundle;

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
                        return string.Compare(bundle.name, otherItem.bundle.name, true);

                    case ColumnIDs.otherLabels:
                        return otherLabels.Count.CompareTo(otherItem.otherLabels.Count);

                    case ColumnIDs.size:
                        return bundle.size.CompareTo(otherItem.bundle.size);

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
                        return EditorUtility.FormatBytes(bundle.size);

                    case ColumnIDs.bundles:
                        return "1";
                }

                return base.ToString(column);
            }
        }
    }
}
