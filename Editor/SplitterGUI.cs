//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using UnityEditor;
using UnityEngine;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    public static class SplitterGUI
    {
        static bool s_SplitterActive;
        static int s_SplitterActiveId = -1;
        static Vector2 s_SplitterMousePosition;

        public static float VerticalSplitter(int id, float value, float min, float max, EditorWindow editorWindow)
        {
            return Splitter(id, ref value, min, max, editorWindow, true);
        }

        public static float HorizontalSplitter(int id, float value, float min, float max, EditorWindow editorWindow)
        {
            return Splitter(id, ref value, min, max, editorWindow, false);
        }

        static float Splitter(int id, ref float value, float min, float max, EditorWindow editorWindow, bool vertical)
        {
            Rect position;// = new Rect();

            if (vertical)
            {
                position = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true));

                var oldColor = GUI.color;
                GUI.color = new Color(0, 0, 0, 0.25f);// new Color(GUI.color.r, GUI.color.g, GUI.color.b, GUI.color.a * 0.25f);
                GUI.DrawTexture(position, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
                GUI.color = oldColor;

                position.y -= 2;
                position.height += 4;
                EditorGUIUtility.AddCursorRect(position, MouseCursor.SplitResizeUpDown);
            }
            else
            {
                position = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandHeight(true));

                var oldColor = GUI.color;
                GUI.color = new Color(0, 0, 0, 0.25f);// new Color(GUI.color.r, GUI.color.g, GUI.color.b, GUI.color.a * 0.25f);
                GUI.DrawTexture(position, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
                GUI.color = oldColor;

                position.x -= 2;
                position.width += 4;
                EditorGUIUtility.AddCursorRect(position, MouseCursor.SplitResizeLeftRight);
            }

            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    if (position.Contains(Event.current.mousePosition))
                    {
                        s_SplitterActive = true;
                        s_SplitterActiveId = id;
                        s_SplitterMousePosition = Event.current.mousePosition;
                    }
                    break;

                case EventType.MouseUp:
                case EventType.MouseLeaveWindow:
                    s_SplitterActive = false;
                    s_SplitterActiveId = -1;
                    editorWindow.Repaint();
                    break;

                case EventType.MouseDrag:
                    if (s_SplitterActive && s_SplitterActiveId == id)
                    {
                        var delta = Event.current.mousePosition - s_SplitterMousePosition;
                        s_SplitterMousePosition = Event.current.mousePosition;

                        if (vertical)
                            value = Mathf.Clamp(value - delta.y / editorWindow.position.height, min, max);
                        else
                            value = Mathf.Clamp(value - delta.x / editorWindow.position.width, min, max);

                        editorWindow.Repaint();
                    }
                    break;
            }

            return value;
        }
    }
}
