//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets.Build.Layout;
using System.Collections.Generic;
using System;

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
            if (o is BuildLayout.Bundle)
            {
                return bundleIcon;
            }

            if (o is BuildLayout.Group)
            {
                return groupIcon;
            }

            if (o is BuildLayout.ExplicitAsset asset)
            {
                return GetBuildLayoutObjectIconFromPath(asset.AssetPath);
            }

            if (o is BuildLayout.DataFromOtherAsset otherAsset)
            {
                return GetBuildLayoutObjectIconFromPath(otherAsset.AssetPath);
            }

            return null;
        }

        public static Texture2D GetBuildLayoutObjectIconFromPath(string assetPath)
        {
            var extension = System.IO.Path.GetExtension(assetPath);

            if (s_FileExtensionTypeLookup.TryGetValue(extension, out var type))
                return AssetPreview.GetMiniTypeThumbnail(type);

            return assetIcon;
        }

        static readonly Dictionary<string, Type> s_FileExtensionTypeLookup = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { ".unity", typeof(SceneAsset) },
            { ".fbx", typeof(ModelImporter) },
            { ".blender", typeof(ModelImporter) },
            { ".ma", typeof(ModelImporter) },
            { ".mb", typeof(ModelImporter) },
            { ".png", typeof(TextureImporter) },
            { ".tga", typeof(TextureImporter) },
            { ".nmp", typeof(TextureImporter) },
            { ".jpg", typeof(TextureImporter) },
            { ".jpeg", typeof(TextureImporter) },
            { ".gif", typeof(TextureImporter) },
            { ".wav", typeof(AudioImporter) },
            { ".mp3", typeof(AudioImporter) },
            { ".ogg", typeof(AudioImporter) },
            { ".otf", typeof(TrueTypeFontImporter) },
            { ".ttf", typeof(TrueTypeFontImporter) },
            { ".anim", typeof(AnimationClip) },
            { ".asset", typeof(ScriptableObject) },
            { ".shader", typeof(ShaderImporter) },
            { ".shadergraph", typeof(ShaderImporter) },
            { ".mp4", typeof(VideoClipImporter) },
            { ".mov", typeof(VideoClipImporter) },
            { ".renderTexture", typeof(RenderTexture) },
            { ".lighting", typeof(LightingSettings) },
            { ".mat", typeof(Material) },
            { ".controller", typeof(UnityEditor.Animations.AnimatorController) }
        };
    }
}
