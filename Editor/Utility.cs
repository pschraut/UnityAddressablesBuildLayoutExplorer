//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using UnityEditor;
using UnityEngine;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    internal static class Utility
    {
        /// <summary>
        /// Transforms a bundle name according to the current <see cref="Settings"/>.
        /// </summary>
        /// <param name="bundleName">The bundle name</param>
        /// <returns>The bundle name perhaps modified according to the current settings.</returns>
        public static string TransformBundleName(string bundleName)
        {
            if (Settings.stripHashFromBundleName)
            {
                TryStripHashFromBundleName(out var s, bundleName, Settings.stripExtensionFromBundleName);
                return s;
            }

            if (Settings.stripExtensionFromBundleName)
            {
                var i = bundleName.LastIndexOf('.');
                if (i != -1)
                    bundleName = bundleName.Substring(0, i);
                return bundleName;
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
                switch(bundleName[n])
                {
                    case 'a': case 'A':
                    case 'b': case 'B':
                    case 'c': case 'C':
                    case 'd': case 'D':
                    case 'e': case 'E':
                    case 'f': case 'F':
                    case '0': case '1': case '2': case '3': case '4':
                    case '5': case '6': case '7': case '8': case '9':
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
