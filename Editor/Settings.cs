//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections.Generic;
using UnityEngine;

namespace Oddworm.EditorFramework
{
    public static class Settings
    {
        const string k_Prefix = "BuildLayoutExplorer";

        public static bool stripHashFromBundleName
        {
            get;
            set;
        } = true;

        public static bool stripExtensionFromBundleName
        {
            get;
            set;
        } = true;

        public static void LoadSettings()
        {
            stripHashFromBundleName = UnityEditor.EditorPrefs.GetBool($"{k_Prefix}.{nameof(stripHashFromBundleName)}", stripHashFromBundleName);
            stripExtensionFromBundleName = UnityEditor.EditorPrefs.GetBool($"{k_Prefix}.{nameof(stripExtensionFromBundleName)}", stripExtensionFromBundleName);
        }

        public static void SaveSettings()
        {
            UnityEditor.EditorPrefs.SetBool($"{k_Prefix}.{nameof(stripHashFromBundleName)}", stripHashFromBundleName);
            UnityEditor.EditorPrefs.SetBool($"{k_Prefix}.{nameof(stripExtensionFromBundleName)}", stripExtensionFromBundleName);
        }
    }
}
