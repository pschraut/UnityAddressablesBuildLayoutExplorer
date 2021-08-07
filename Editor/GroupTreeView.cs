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

    public class GroupTreeView : BuildLayoutTreeView
    {
        public GroupTreeView(BuildLayoutWindow window)
                   : base(window, new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[] {
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Name"), width = 250, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Size"), width = 80, autoResize = true },
                            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Bundles"), width = 80, autoResize = true },
                            })))
        {
        }


        protected override void OnBuildTree(TreeViewItem rootItem, BuildLayout buildLayout)
        {
            foreach (var group in buildLayout.groups)
            {
                var child = new GroupItem
                {
                    source = group,
                    id = m_UniqueId++,
                    depth = 0,
                    displayName = group.name
                };

                rootItem.AddChild(child);
            }
        }


        [System.Serializable]
        class GroupItem : BaseItem
        {
            public BuildLayout.Group source;

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
                        EditorGUI.LabelField(position, $"{source.bundles.Count}");
                        break;
                }
            }
        }
    }
}
