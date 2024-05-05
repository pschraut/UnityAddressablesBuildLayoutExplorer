//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2024 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Build.Layout;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    internal static class Utility
    {
        public static List<object> GetReferencedBy(BuildLayout layout, object obj)
        {
            var hashset = new HashSet<object>();

            var archive = obj as BuildLayout.Bundle;
            if (archive != null)
                hashset.UnionWith(GetReferencedBy(layout, archive));

            var group = obj as BuildLayout.Group;
            if (group != null)
                hashset.UnionWith(GetReferencedBy(layout, group));

            var asset = obj as BuildLayout.ExplicitAsset;
            if (asset != null)
                hashset.UnionWith(GetReferencedBy(layout, asset));

            var other = obj as BuildLayout.DataFromOtherAsset;
            if (other != null)
                hashset.UnionWith(GetReferencedBy(layout, other));

            return new List<object>(hashset);
        }

        public static List<object> GetReferencesTo(BuildLayout layout, object obj)
        {
            var hashset = new HashSet<object>();

            var archive = obj as BuildLayout.Bundle;
            if (archive != null)
                hashset.UnionWith(GetReferencesTo(layout, archive));

            var group = obj as BuildLayout.Group;
            if (group != null)
                hashset.UnionWith(GetReferencesTo(layout, group));

            var asset = obj as BuildLayout.ExplicitAsset;
            if (asset != null)
                hashset.UnionWith(GetReferencesTo(layout, asset));

            return new List<object>(hashset);
        }

        static HashSet<object> GetReferencedBy(BuildLayout layout, BuildLayout.Bundle archive)
        {
            var result = new HashSet<object>();
            if (archive == null)
                return result;

            foreach (var b in archive.DependentBundles)
                result.Add(b);

            foreach (var g in layout.Groups)
            {
                if (g.Bundles.Contains(archive))
                    result.Add(g);
            }

            return result;
        }

        static HashSet<object> GetReferencesTo(BuildLayout layout, BuildLayout.Bundle archive)
        {
            var result = new HashSet<object>();
            if (archive == null)
                return result;

            foreach (var b in archive.Dependencies)
                result.Add(b);

            foreach (var b in archive.ExpandedDependencies)
                result.Add(b);

            foreach (var f in archive.Files)
            {
                foreach(var a in f.Assets)
                {
                    result.Add(a);
                }
            }

            return result;
        }

        static HashSet<object> GetReferencedBy(BuildLayout layout, BuildLayout.Group group)
        {
            var result = new HashSet<object>();
            if (group == null)
                return result;

            // TODO
            return result;
        }

        static HashSet<object> GetReferencesTo(BuildLayout layout, BuildLayout.Group group)
        {
            var result = new HashSet<object>();
            if (group == null)
                return result;

            foreach (var b in group.Bundles)
                result.Add(b);

            return result;
        }

        static HashSet<object> GetReferencedBy(BuildLayout layout, BuildLayout.ExplicitAsset asset)
        {
            var result = new HashSet<object>();
            if (asset == null)
                return result;

            result.Add(asset.Bundle);

            return result;
        }

        static HashSet<object> GetReferencesTo(BuildLayout layout, BuildLayout.ExplicitAsset asset)
        {
            var result = new HashSet<object>();
            if (asset == null)
                return result;

            
            foreach (var b in asset.ExternallyReferencedAssets)
            {
                result.Add(b);
                result.Add(b.Bundle);
            }

            return result;
        }

        static HashSet<object> GetReferencedBy(BuildLayout layout, BuildLayout.DataFromOtherAsset other)
        {
            var result = new HashSet<object>();
            if (other == null)
                return result;


            foreach (var b in other.ReferencingAssets)
            {
                result.Add(b);
                result.Add(b.Bundle);
            }

            return result;
        }

        /// <summary>
        /// Transforms a bundle name according to the current <see cref="Settings"/>.
        /// </summary>
        /// <param name="bundleName">The bundle name</param>
        /// <returns>The bundle name perhaps modified according to the current settings.</returns>
        public static string TransformBundleName(string bundleName)
        {
            if (Settings.stripHashFromName)
            {
                TryStripHashFromBundleName(out var s, bundleName, false);
                bundleName = s;
            }

            if (Settings.stripExtensionFromName)
            {
                var i = bundleName.LastIndexOf('.');
                if (i != -1)
                    bundleName = bundleName.Substring(0, i);
            }

            if (Settings.stripDirectoryFromName)
            {
                var i = bundleName.LastIndexOf('/');
                if (i != -1)
                    bundleName = bundleName.Substring(i + 1, bundleName.Length - i - 1);
            }

            return bundleName;
        }

        public static bool TryStripHashFromBundleName(out string result, string bundleName, bool stripFileExtension = false)
        {
            // defaultlocalgroup_monoscripts_dc5ec83866547c5a3123179af400036e.bundle
            var bundleHashLength = "_dc5ec83866547c5a3123179af400036e.bundle".Length;
            if (bundleName.Length <= bundleHashLength)
            {
                MakeFailResult(out result);
                return false;
            }

            // Does it end with .bundle?
            var bundleLength = ".bundle".Length;
            if (bundleName[bundleName.Length - bundleLength] != '.')
            {
                MakeFailResult(out result);
                return false;
            }

            // Does it have an underscore at the position we assume?
            if (bundleName[bundleName.Length - bundleHashLength] != '_')
            {
                MakeFailResult(out result);
                return false;
            }

            // Check if any character in what we assume is the hash, is betwee A-F and 0-9,
            // because only those characters are actually used for bundle hashes
            var hashLength = "dc5ec83866547c5a3123179af400036e".Length;
            for (int n = bundleName.Length - bundleLength - 1, i = hashLength; n >= 0 && i > 0; --n, --i)
            {
                switch (bundleName[n])
                {
                    case 'a':
                    case 'A':
                    case 'b':
                    case 'B':
                    case 'c':
                    case 'C':
                    case 'd':
                    case 'D':
                    case 'e':
                    case 'E':
                    case 'f':
                    case 'F':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        break;

                    default: // letter not part of a hash, so it can't be a bundleName with hash
                        MakeFailResult(out result);
                        return false;
                }
            }

            if (stripFileExtension)
            {
                result = bundleName.Substring(0, bundleName.Length - bundleHashLength);
                return true;
            }

            result = bundleName.Substring(0, bundleName.Length - bundleHashLength) + ".bundle";
            return true;


            void MakeFailResult(out string ret)
            {
                ret = bundleName;
                if (stripFileExtension)
                {
                    var i = ret.LastIndexOf('.');
                    if (i != -1)
                        ret = ret.Substring(0, i);
                }
            }
        }
    }
}
