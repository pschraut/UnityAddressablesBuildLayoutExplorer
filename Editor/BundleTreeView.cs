//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Oddworm.EditorFramework
{

    public class BundleTreeView : BuildLayoutTreeView
    {
        public BundleTreeView(BuildLayoutWindow window)
                   : base(window, new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[] {
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Name"), width = 250, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Size"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Dependencies"), width = 80, autoResize = true },
                            })))
        {
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

                    var child = new GroupItem
                    {
                        source = bundle,
                        id = m_UniqueId++,
                        depth = 0,
                        displayName = bundle.name
                    };

                    rootItem.AddChild(child);
                }
            }
        }


        [System.Serializable]
        class GroupItem : BaseItem
        {
            public BuildLayout.Archive source;

            public override void OnGUI(Rect position, int column)
            {
                switch(column)
                {
                    case 0:
                        EditorGUI.LabelField(position, source.name);
                        break;

                    case 1:
                        EditorGUI.LabelField(position, $"{EditorUtility.FormatBytes(source.size)}");
                        break;

                    case 2:
                        EditorGUI.LabelField(position, $"{source.bundleDependencies.Count}");
                        break;
                }
            }
        }
    }
}
