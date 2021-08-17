//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using UnityEditor;
using UnityEngine;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    internal static class Styles
    {
        public static Texture2D navigateBackwardsIcon
        {
            get;
            private set;
        }

        public static Texture2D navigateForwardsIcon
        {
            get;
            private set;
        }

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

        public static Texture2D builtinBundleIcon
        {
            get;
            private set;
        }

        public static Texture2D referencesToIcon
        {
            get;
            private set;
        }

        public static Texture2D referencedByIcon
        {
            get;
            private set;
        }

        public static Texture2D assetIcon
        {
            get;
            private set;
        }

        public static Texture2D navigateIcon
        {
            get;
            private set;
        }

        public static Texture2D settingsIcon
        {
            get;
            private set;
        }

        public static GUIStyle iconStyle
        {
            get;
            private set;
        }

        public static GUIStyle ghostLabelStyle
        {
            get;
            private set;
        }

        public static GUIStyle iconButtonStyle
        {
            get;
            private set;
        }

        public static GUIStyle iconToolbarButtonStyle
        {
            get;
            private set;
        }

        public static GUIStyle viewStyle
        {
            get;
            private set;
        }

        static Styles()
        {
            groupIcon = FindBuiltinTexture("ScriptableObject Icon");
            bundleIcon = FindBuiltinTexture("LODGroup Icon");
            referencesToIcon = FindBuiltinTexture("Animator Icon");
            //bundleExpandedDependenciesIcon = FindBuiltinTexture("d_NetworkAnimator Icon");
            referencedByIcon = FindBuiltinTexture("VisualEffectSubgraphOperator Icon");
            navigateBackwardsIcon = FindBuiltinTexture("back@2x");
            navigateForwardsIcon = FindBuiltinTexture("forward@2x");
            assetIcon = FindBuiltinTexture("DefaultAsset Icon");
            builtinBundleIcon = FindBuiltinTexture("CanvasGroup Icon");
            navigateIcon = FindBuiltinTexture("PlayButton@2x");
            settingsIcon = FindBuiltinTexture("Settings@2x");
            // d_UnityEditor.Graphs.AnimatorControllerTool@2x

            iconButtonStyle = new GUIStyle(GUI.skin.button);
            iconButtonStyle.fixedWidth = 18;
            iconButtonStyle.fixedHeight = 18;
            iconButtonStyle.padding = new RectOffset();
            iconButtonStyle.contentOffset = new Vector2(0, 0);

            iconToolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
            //iconToolbarButtonStyle.fixedWidth = 18;
            //iconToolbarButtonStyle.fixedHeight = 18;
            iconToolbarButtonStyle.padding = new RectOffset(1,1,1,1);
            iconToolbarButtonStyle.contentOffset = new Vector2(0, 0);

            iconStyle = new GUIStyle(EditorStyles.label);
            iconStyle.fixedWidth = 18;
            iconStyle.fixedHeight = 18;
            iconStyle.padding = new RectOffset();
            iconStyle.contentOffset = new Vector2(0, 0);

            ghostLabelStyle = new GUIStyle(EditorStyles.label);
            var tc = ghostLabelStyle.normal.textColor;
            tc.a *= 0.6f;
            ghostLabelStyle.normal.textColor = tc;

            viewStyle = new GUIStyle(EditorStyles.helpBox);
            viewStyle.margin = new RectOffset(0, 0, 0, 0);
            viewStyle.padding = new RectOffset(4, 4, 4, 4);
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

        public static Texture2D GetBuildLayoutObjectIcon(object o)
        {
            if (o is RichBuildLayout.Archive)
            {
                var b = o as RichBuildLayout.Archive;
                if (b.isBuiltin)
                    return builtinBundleIcon;
                return bundleIcon;
            }

            if (o is RichBuildLayout.Group)
            {
                return groupIcon;
            }

            if (o is RichBuildLayout.Asset)
            {
                return assetIcon;
            }

            return null;
        }
    }
}
