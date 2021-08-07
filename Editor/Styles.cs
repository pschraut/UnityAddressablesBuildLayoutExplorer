//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    internal static class Styles
    {
        public static Texture2D groupIcon
        {
            get;
            private set;
        }

        public static Texture2D bundleIcon
        {
            get;
            private set;
        }

        public static GUIStyle iconStyle
        {
            get;
            private set;
        }

        static Styles()
        {
            groupIcon = FindBuiltinTexture("ScriptableObject Icon");
            bundleIcon = FindBuiltinTexture("TextAsset Icon");

            iconStyle = new GUIStyle(EditorStyles.label);
            iconStyle.fixedWidth = 18;
            iconStyle.fixedHeight = 18;
            iconStyle.padding = new RectOffset();
            iconStyle.contentOffset = new Vector2(0, 0);
        }

        static Texture2D FindBuiltinTexture(string name)
        {
            var t = EditorGUIUtility.FindTexture(name);
            if (t != null)
                return t;

            var c = EditorGUIUtility.IconContent(name);
            if (c != null && c.image != null)
                return (Texture2D)c.image;

            return null;
        }
    }
}
