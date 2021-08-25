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
        const string k_Prefix = "BuildLayoutExplorer";

        public static bool stripHashFromName
        {
            get;
            set;
        } = false;

        public static bool stripExtensionFromName
        {
            get;
            set;
        } = false;

        public static bool stripDirectoryFromName
        {
            get;
            set;
        } = false;

        public static bool debugViewMenu
        {
            get;
            set;
        } = false;


        public static void LoadSettings()
        {
            stripHashFromName = GetBool(nameof(stripHashFromName), stripHashFromName);
            stripExtensionFromName = GetBool(nameof(stripExtensionFromName), stripExtensionFromName);
            debugViewMenu = GetBool(nameof(debugViewMenu), debugViewMenu);
            stripDirectoryFromName = GetBool(nameof(stripDirectoryFromName), stripDirectoryFromName);
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
            return EditorPrefs.GetFloat($"{k_Prefix}.{key}", defaultValue);
        }

        public static int GetInt(string key, int defaultValue)
        {
            return EditorPrefs.GetInt($"{k_Prefix}.{key}", defaultValue);
        }

        public static bool GetBool(string key, bool defaultValue)
        {
            return EditorPrefs.GetBool($"{k_Prefix}.{key}", defaultValue);
        }

        public static string GetString(string key, string defaultValue)
        {
            return EditorPrefs.GetString($"{k_Prefix}.{key}", defaultValue);
        }

        public static void SetFloat(string key, float value)
        {
            EditorPrefs.SetFloat($"{k_Prefix}.{key}", value);
        }

        public static void SetInt(string key, int value)
        {
            EditorPrefs.SetInt($"{k_Prefix}.{key}", value);
        }

        public static void SetBool(string key, bool value)
        {
            EditorPrefs.SetBool($"{k_Prefix}.{key}", value);
        }

        public static void SetString(string key, string value)
        {
            EditorPrefs.SetString($"{k_Prefix}.{key}", value);
        }
    }
}
