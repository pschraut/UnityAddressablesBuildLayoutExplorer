//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Oddworm.EditorFramework
{
    public static class Settings
    {
        const string k_EditorPrefsPrefix = "BuildLayoutExplorer";

        static bool s_StripHashFromName = false;
        static bool s_StripExtensionFromName = false;
        static bool s_StripDirectoryFromName = false;
        static bool s_DebugViewMenu = false;

        public static System.Action changed
        {
            get;
            set;
        }

        public static bool stripHashFromName
        {
            get => s_StripHashFromName;
            set
            {
                if (value == s_StripHashFromName)
                    return;
                s_StripHashFromName = value;
                changed?.Invoke();
            }
        }

        public static bool stripExtensionFromName
        {
            get => s_StripExtensionFromName;
            set
            {
                if (value == s_StripExtensionFromName)
                    return;
                s_StripExtensionFromName = value;
                changed?.Invoke();
            }
        }

        public static bool stripDirectoryFromName
        {
            get => s_StripDirectoryFromName;
            set
            {
                if (value == s_StripDirectoryFromName)
                    return;
                s_StripDirectoryFromName = value;
                changed?.Invoke();
            }
        }

        public static bool debugViewMenu
        {
            get => s_DebugViewMenu;
            set
            {
                if (value == s_DebugViewMenu)
                    return;
                s_DebugViewMenu = value;
                changed?.Invoke();
            }
        }


        public static void LoadSettings()
        {
            var callback = changed;
            changed = null; // avoid to fire the callback with every setting change

            stripHashFromName = GetBool(nameof(stripHashFromName), stripHashFromName);
            stripExtensionFromName = GetBool(nameof(stripExtensionFromName), stripExtensionFromName);
            debugViewMenu = GetBool(nameof(debugViewMenu), debugViewMenu);
            stripDirectoryFromName = GetBool(nameof(stripDirectoryFromName), stripDirectoryFromName);

            changed = callback;
            changed?.Invoke();
        }

        public static void SaveSettings()
        {
            SetBool(nameof(stripHashFromName), stripHashFromName);
            SetBool(nameof(stripExtensionFromName), stripExtensionFromName);
            SetBool(nameof(debugViewMenu), debugViewMenu);
            SetBool(nameof(stripDirectoryFromName), stripDirectoryFromName);
        }

        public static float GetFloat(string key, float defaultValue)
        {
            return EditorPrefs.GetFloat($"{k_EditorPrefsPrefix}.{key}", defaultValue);
        }

        public static int GetInt(string key, int defaultValue)
        {
            return EditorPrefs.GetInt($"{k_EditorPrefsPrefix}.{key}", defaultValue);
        }

        public static bool GetBool(string key, bool defaultValue)
        {
            return EditorPrefs.GetBool($"{k_EditorPrefsPrefix}.{key}", defaultValue);
        }

        public static string GetString(string key, string defaultValue)
        {
            return EditorPrefs.GetString($"{k_EditorPrefsPrefix}.{key}", defaultValue);
        }

        public static void SetFloat(string key, float value)
        {
            EditorPrefs.SetFloat($"{k_EditorPrefsPrefix}.{key}", value);
        }

        public static void SetInt(string key, int value)
        {
            EditorPrefs.SetInt($"{k_EditorPrefsPrefix}.{key}", value);
        }

        public static void SetBool(string key, bool value)
        {
            EditorPrefs.SetBool($"{k_EditorPrefsPrefix}.{key}", value);
        }

        public static void SetString(string key, string value)
        {
            EditorPrefs.SetString($"{k_EditorPrefsPrefix}.{key}", value);
        }
    }
}
