//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq.Expressions;
using System;
using UnityEditor.IMGUI.Controls;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    public class GroupsView : BuildLayoutView
    {
        [SerializeField] BuildLayoutTreeView m_TreeView;
        SearchField m_SearchField;
        string m_StatusLabel;

        public override void Awake()
        {
            base.Awake();

            m_TreeView = new GroupTreeView(window);
            m_TreeView.selectedItemChanged += OnSelectedItemChanged;
            m_SearchField = new SearchField(window);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            m_TreeView.selectedItemChanged -= OnSelectedItemChanged;
        }

        void OnSelectedItemChanged(TreeViewItem selectedItem)
        {
            if (selectedItem == null)
                return;

            var name = selectedItem.displayName;
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>($"Assets/AddressableAssetsData/AssetGroups/{name}.asset");
            if (asset != null)
                Selection.activeObject = asset;
        }

        public override void Rebuild(BuildLayout buildLayout)
        {
            base.Rebuild(buildLayout);

            m_TreeView.SetBuildLayout(buildLayout);

            var size = 0L;
            var count = 0;
            foreach (var group in buildLayout.groups)
            {
                size += group.size;
                count++;
            }
            m_StatusLabel = $"{count} groups making up {EditorUtility.FormatBytes(size)}";
        }

        public override void OnGUI()
        {
            var rect = GUILayoutUtility.GetRect(10, 10, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            m_TreeView.OnGUI(rect);
        }

        public override void OnToolbarGUI()
        {
            base.OnToolbarGUI();

            GUILayout.Space(10);

            if (m_SearchField.OnToolbarGUI(GUILayout.ExpandWidth(true)))
                m_TreeView.searchString = m_SearchField.text;
        }

        public override void OnStatusbarGUI()
        {
            base.OnStatusbarGUI();

            GUILayout.Label(m_StatusLabel);
        }
    }
}
