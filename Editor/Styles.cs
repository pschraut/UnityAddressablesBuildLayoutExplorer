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
        
        public static Texture2D labelIcon
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

        public static Texture2D includedByIcon
        {
            get;
            private set;
        }

        public static Texture2D includedInIcon
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

        public static Texture2D selectAssetIcon
        {
            get;
            private set;
        }

        public static Texture2D openContainingFolderIcon
        {
            get;
            private set;
        }

        public static Texture2D deleteIcon
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

        public static GUIStyle selectedGhostLabelStyle
        {
            get;
            private set;
        }

        public static GUIStyle labelStyle
        {
            get;
            private set;
        }

        public static GUIStyle selectedLabelStyle
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
            labelIcon = FindBuiltinTexture("FilterByLabel@2x");
            bundleIcon = FindBuiltinTexture("LODGroup Icon");
            referencesToIcon = FindBuiltinTexture("Animator Icon");
            referencedByIcon = FindBuiltinTexture("VisualEffectSubgraphOperator Icon");
            navigateBackwardsIcon = FindBuiltinTexture("back@2x");
            navigateForwardsIcon = FindBuiltinTexture("forward@2x");
            assetIcon = FindBuiltinTexture("DefaultAsset Icon");
            builtinBundleIcon = FindBuiltinTexture("CanvasGroup Icon");
            navigateIcon = FindBuiltinTexture("PlayButton@2x");
            settingsIcon = FindBuiltinTexture("Settings@2x");
            openContainingFolderIcon = FindBuiltinTexture("Folder Icon");
            selectAssetIcon = FindBuiltinTexture("Search Icon");
            deleteIcon = FindBuiltinTexture("P4_DeletedLocal");

            includedByIcon = FindBuiltinTexture("AnimatorStateMachine Icon");
            includedInIcon = FindBuiltinTexture("AnimatorController Icon");

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

            labelStyle = new GUIStyle(EditorStyles.label);

            selectedLabelStyle = new GUIStyle(EditorStyles.label);
            selectedLabelStyle.normal.textColor = new Color(0.95f, 0.95f, 0.95f);

            iconStyle = new GUIStyle(EditorStyles.label);
            iconStyle.fixedWidth = 18;
            iconStyle.fixedHeight = 18;
            iconStyle.padding = new RectOffset();
            iconStyle.contentOffset = new Vector2(0, 0);

            ghostLabelStyle = new GUIStyle(labelStyle);
            var tc = ghostLabelStyle.normal.textColor;
            tc.a *= 0.6f;
            ghostLabelStyle.normal.textColor = tc;

            selectedGhostLabelStyle = new GUIStyle(labelStyle);
            tc = selectedLabelStyle.normal.textColor;
            tc.a *= 0.6f;
            selectedGhostLabelStyle.normal.textColor = tc;

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
