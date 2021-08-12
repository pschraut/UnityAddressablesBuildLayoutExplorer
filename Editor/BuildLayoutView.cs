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
    public class BuildLayoutView
    {
        /// <summary>
        /// The titleContent is shown in toolbar View menu.
        /// </summary>
        public GUIContent titleContent
        {
            get;
            set;
        }

        /// <summary>
        /// Lets you control the menu item order in the View menu.
        /// Specify a negative value to not create an item in the View menu.
        /// If two items are 100 units apart it inserts a seperator item.
        /// </summary>
        public int viewMenuOrder
        {
            get;
            set;
        }

        /// <summary>
        /// The window in which the view is rendered to.
        /// </summary>
        public BuildLayoutWindow window
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets whether the view is currently active.
        /// </summary>
        public bool isVisible
        {
            get;
            private set;
        }

        public BuildLayout buildLayout
        {
            get;
            private set;
        }


        /// <summary>
        /// The key-prefix to load and save EditorPrefs.
        /// </summary>
        public string editorPrefsKey
        {
            get;
            set;
        }

        List<BuildLayoutView> m_Views = new List<BuildLayoutView>();

        public BuildLayoutView()
        {
            editorPrefsKey = GetType().Name;
            viewMenuOrder = int.MaxValue - 1;
        }

        public virtual void Awake()
        {
            titleContent = new GUIContent(ObjectNames.NicifyVariableName(GetType().Name.Replace("View", "")));
        }

        public virtual void OnDestroy()
        {
            if (isVisible)
                Hide();
            m_Views = new List<BuildLayoutView>();
            buildLayout = null;
            window = null;
        }

        public virtual void Rebuild(BuildLayout buildLayout)
        {
            this.buildLayout = buildLayout;

            foreach (var v in m_Views)
                v.Rebuild(buildLayout);
        }

        public virtual void Show()
        {
            // Show any views that might have been added during OnShow()
            foreach (var v in m_Views)
                v.Show();

            isVisible = true;
            window.Repaint();
        }

        public virtual void Hide()
        {
            foreach (var v in m_Views)
                v.Hide();

            isVisible = false;
            window.Repaint();
        }

        /// <summary>
        /// Implement your own editor GUI here.
        /// </summary>
        public virtual void OnGUI()
        {
        }

        /// <summary>
        /// Implement the OnToolbarGUI method to draw your own GUI in toolbar menu.
        /// </summary>
        public virtual void OnToolbarGUI()
        {
        }

        /// <summary>
        /// Implement the OnStatusbarGUI method to draw your own GUI in statusbar menu.
        /// </summary>
        public virtual void OnStatusbarGUI()
        {
        }

        // This allows to pass a member variable whose name is converted to a string.
        protected string GetPrefsKey(Expression<Func<object>> exp)
        {
            var body = exp.Body as MemberExpression;
            if (body == null)
            {
                var ubody = (UnaryExpression)exp.Body;
                body = ubody.Operand as MemberExpression;
            }

            return $"BuildLayoutExplorer.{editorPrefsKey}.{body.Member.Name}";
        }

        protected T CreateView<T>() where T : BuildLayoutView, new()
        {
            var view = new T();
            view.window = window;
            view.Awake();
            m_Views.Add(view);
            return view;
        }
    }
}
