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

        public static Texture2D bundleDependenciesIcon
        {
            get;
            private set;
        }

        public static Texture2D bundleExpandedDependenciesIcon
        {
            get;
            private set;
        }
        public static Texture2D referencedByBundleIcon
        {
            get;
            private set;
        }

        public static Texture2D explicitAssetsIcon
        {
            get;
            private set;
        }

        public static Texture2D externalAssetReferenceIcon
        {
            get;
            private set;
        }

        public static Texture2D internalAssetReferenceIcon
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

        static Styles()
        {
            groupIcon = FindBuiltinTexture("ScriptableObject Icon");
            bundleIcon = FindBuiltinTexture("LODGroup Icon");
            bundleDependenciesIcon = FindBuiltinTexture("Animator Icon");
            bundleExpandedDependenciesIcon = FindBuiltinTexture("d_NetworkAnimator Icon");
            explicitAssetsIcon = FindBuiltinTexture("d_DefaultAsset Icon");
            internalAssetReferenceIcon = FindBuiltinTexture("d_AnimatorOverrideController On Icon");
            externalAssetReferenceIcon = FindBuiltinTexture("d_AnimatorOverrideController Icon");
            referencedByBundleIcon = FindBuiltinTexture("d_VisualEffectSubgraphOperator Icon");

            iconStyle = new GUIStyle(EditorStyles.label);
            iconStyle.fixedWidth = 18;
            iconStyle.fixedHeight = 18;
            iconStyle.padding = new RectOffset();
            iconStyle.contentOffset = new Vector2(0, 0);

            ghostLabelStyle = new GUIStyle(EditorStyles.label);
            var tc = ghostLabelStyle.normal.textColor;
            tc.a *= 0.6f;
            ghostLabelStyle.normal.textColor = tc;
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
