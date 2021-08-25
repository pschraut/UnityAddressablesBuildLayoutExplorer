//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    internal static class Utility
    {
        public static List<object> GetReferencedBy(object obj)
        {
            var hashset = new HashSet<object>();

            var archive = obj as RichBuildLayout.Archive;
            if (archive != null)
                hashset.UnionWith(GetReferencedBy(archive));

            var group = obj as RichBuildLayout.Group;
            if (group != null)
                hashset.UnionWith(GetReferencedBy(group));

            var asset = obj as RichBuildLayout.Asset;
            if (asset != null)
                hashset.UnionWith(GetReferencedBy(asset));

            return new List<object>(hashset);
        }

        public static List<object> GetReferencesTo(object obj)
        {
            var hashset = new HashSet<object>();

            var archive = obj as RichBuildLayout.Archive;
            if (archive != null)
                hashset.UnionWith(GetReferencesTo(archive));

            var group = obj as RichBuildLayout.Group;
            if (group != null)
                hashset.UnionWith(GetReferencesTo(group));

            var asset = obj as RichBuildLayout.Asset;
            if (asset != null)
                hashset.UnionWith(GetReferencesTo(asset));

            return new List<object>(hashset);
        }

        static HashSet<object> GetReferencedBy(RichBuildLayout.Archive archive)
        {
            var result = new HashSet<object>();
            if (archive == null)
                return result;

            foreach (var b in archive.referencedByBundles)
                result.Add(b);

            foreach (var b in archive.referencedByGroups)
                result.Add(b);

            return result;
        }

        static HashSet<object> GetReferencesTo(RichBuildLayout.Archive archive)
        {
            var result = new HashSet<object>();
            if (archive == null)
                return result;

            foreach (var b in archive.bundleDependencies)
                result.Add(b);

            foreach (var b in archive.expandedBundleDependencies)
                result.Add(b);

            foreach (var a in archive.explicitAssets)
            {
                foreach (var b in a.externalReferences)
                    result.Add(b);
            }

            return result;
        }

        static HashSet<object> GetReferencedBy(RichBuildLayout.Group group)
        {
            var result = new HashSet<object>();
            if (group == null)
                return result;

            // TODO
            return result;
        }

        static HashSet<object> GetReferencesTo(RichBuildLayout.Group group)
        {
            var result = new HashSet<object>();
            if (group == null)
                return result;

            foreach (var b in group.bundles)
                result.Add(b);

            return result;
        }

        static HashSet<object> GetReferencedBy(RichBuildLayout.Asset asset)
        {
            var result = new HashSet<object>();
            if (asset == null)
                return result;

            foreach (var b in asset.referencedByBundle)
                result.Add(b);

            return result;
        }

        static HashSet<object> GetReferencesTo(RichBuildLayout.Asset asset)
        {
            var result = new HashSet<object>();
            if (asset == null)
                return result;

            foreach (var b in asset.externalReferences)
            {
                result.Add(b);

                foreach (var c in b.referencedByBundle)
                    result.Add(c);
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
