//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    public abstract class BuildLayoutView
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
        /// Use a negative value to hide an item from the View menu.
        /// If two items are 100 units apart a seperator item is inserted inbetween.
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

        /// <summary>
        /// Gets the buildLayout.
        /// </summary>
        public RichBuildLayout buildLayout
        {
            get;
            private set;
        }

        List<BuildLayoutView> m_Views = new List<BuildLayoutView>();

        public BuildLayoutView()
        {
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

        public virtual void Rebuild(RichBuildLayout buildLayout)
        {
            this.buildLayout = buildLayout;

            foreach (var v in m_Views)
                v.Rebuild(buildLayout);

            window.Repaint();
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
        /// Implement the OnStatusbarGUI method to draw your own GUI in statusbar menu.
        /// </summary>
        public virtual void OnStatusbarGUI()
        {
        }

        public virtual bool CanNavigateTo(object target)
        {
            return false;
        }

        public virtual void NavigateTo(object target)
        {
        }

        /// <summary>
        /// Returns an object that can be used to restore the current view.
        /// </summary>
        public abstract NavigationBookmark GetBookmark();

        /// <summary>
        /// Restores the view using the specified <paramref name="bookmark"/>.
        /// </summary>
        public abstract void SetBookmark(NavigationBookmark bookmark);

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
