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

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    public class BundlesView : BuildLayoutView
    {
        [SerializeField] BuildLayoutTreeView m_TreeView;

        public override void Awake()
        {
            base.Awake();

            m_TreeView = new BundleTreeView(window);
        }

        public override void Rebuild(BuildLayout buildLayout)
        {
            base.Rebuild(buildLayout);

            m_TreeView.SetBuildLayout(buildLayout);
        }

        public override void OnGUI()
        {
            var rect = GUILayoutUtility.GetRect(10, 10, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            m_TreeView.OnGUI(rect);
        }
    }
}
