//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    /// <summary>
    /// Can be used to read a package.json file.
    /// </summary>
    [System.Serializable]
    public class Package
    {
        // The asset guid of the package.json file
        public const string k_PackageAssetGUID = "e16fc46ee80b02e41bcc2e8044634698";

        public string name = "";
        public string version = "0.0.0";
        public string displayName = "Addressables BuildLayout Explorer";
        public string description = "";
        public string unity = "";
        public string documentationUrl = "https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer";
        public List<string> keywords = new List<string>();
        public Author author = new Author();
        public Repository repository = new Repository();

        [System.Serializable]
        public class Author
        {
            public string name = "";
            public string url = "";
        }

        [System.Serializable]
        public class Repository
        {
            public string type = "git";
            public string url = "https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer.git";
        }

        public static Package Load()
        {
            Package value = null;

            try
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(k_PackageAssetGUID);
                var json = System.IO.File.ReadAllText(assetPath);
                value = JsonUtility.FromJson<Package>(json);
            }
            finally
            {
                if (value == null)
                    value = new Package();
            }

            return value;
        }
    }
}
