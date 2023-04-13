using UnityEditor;
using UnityEngine;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    [BuildLayoutView]
    public class LabelsView : BuildLayoutView
    {
        LabelsTreeView m_TreeView;
        SearchField m_SearchField;

        public override void Awake()
        {
            base.Awake();

            viewMenuOrder = 14;
            m_TreeView = new LabelsTreeView(window);
            m_SearchField = new SearchField(window);
        }

        public override void Rebuild(RichBuildLayout buildLayout)
        {
            base.Rebuild(buildLayout);
            m_TreeView.SetBuildLayout(buildLayout);
        }

        public override void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope(Styles.viewStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent(" Labels", Styles.labelIcon), EditorStyles.boldLabel);

                    if (m_SearchField.OnToolbarGUI(GUILayout.ExpandWidth(true)))
                        m_TreeView.Search(m_SearchField.text);
                }

                var rect = GUILayoutUtility.GetRect(10, 10, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                m_TreeView.OnGUI(rect);
            }
        }

        public override NavigationBookmark GetBookmark()
        {
            var bm = new Bookmark();
            bm.labelsState = m_TreeView.GetState();
            return bm;
        }

        public override void SetBookmark(NavigationBookmark bookmark)
        {
            var bm = bookmark as Bookmark;
            if (bm == null)
            {
                Debug.LogError($"Cannot set bookmark, because the argument '{nameof(bookmark)}' is of the wrong type or null.");
                return;
            }

            m_TreeView.SetState(bm.labelsState);
            m_TreeView.SetFocus();
        }

        class Bookmark : NavigationBookmark
        {
            public BuildLayoutTreeViewState labelsState;
        }
    }
}
