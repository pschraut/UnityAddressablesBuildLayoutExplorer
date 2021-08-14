//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections.Generic;
using UnityEngine;

namespace Oddworm.EditorFramework
{
    /// <summary>
    /// The <see cref="RichBuildLayout"/> class provides a higher-level abstraction of the <see cref="BuildLayout"/>
    /// that provides additional data.
    /// </summary>
    [System.Serializable]
    public class RichBuildLayout
    {
        public BuildLayout lowlevel; // a reference to the underlaying object
        public string unityVersion = "";
        public string addressablesVersion = "";
        public List<Group> groups = new List<Group>();
        public List<Archive> bundles = new List<Archive>();

        [System.Serializable]
        public class Group
        {
            public string name = "";
            public long size;
            public List<Archive> bundles = new List<Archive>();
            public BuildLayout.Group lowlevel; // a reference to the underlaying object
        }

        [System.Serializable]
        public class Archive
        {
            public string name = "";
            public long size;
            public string compression = "";
            public long assetBundleObjectSize;
            public List<Archive> bundleDependencies = new List<Archive>();
            public List<Archive> expandedBundleDependencies = new List<Archive>();
            public List<ExplicitAsset> explicitAssets = new List<ExplicitAsset>();
            public BuildLayout.Archive lowlevel; // a reference to the underlaying object
            public List<Group> referencedByGroups = new List<Group>();
        }

        [System.Serializable]
        public class ExplicitAsset
        {
            public string name;
            public long size;
            public string address;
            public List<string> externalReferences = new List<string>();
            public List<string> internalReferences = new List<string>();
            public BuildLayout.ExplicitAsset lowlevel; // a reference to the underlaying object
        }

        public RichBuildLayout(BuildLayout buildLayout)
        {
            lowlevel = buildLayout;
            unityVersion = buildLayout.unityVersion;
            addressablesVersion = buildLayout.addressablesVersion;

            // 1st pass is to collect all bundles
            foreach (var baseGroup in buildLayout.groups)
            {
                foreach(var baseBundle in baseGroup.bundles)
                {
                    var bundle = FindBundle(baseBundle.name);
                    if (bundle != null)
                        continue;

                    bundle = new Archive
                    {
                        lowlevel = baseBundle,
                        name = baseBundle.name,
                        compression = baseBundle.compression,
                        assetBundleObjectSize = baseBundle.assetBundleObjectSize
                    };
                    bundles.Add(bundle);
                }
            }

            // 2nd pass on groups to fill groups with earlier collected bundles
            foreach (var baseGroup in buildLayout.groups)
            {
                var group = new Group
                {
                    lowlevel = baseGroup,
                    name = baseGroup.name,
                    size = baseGroup.size
                };
                groups.Add(group);

                foreach (var baseBundle in baseGroup.bundles)
                {
                    var bundle = FindBundle(baseBundle.name);
                    if (bundle != null)
                        continue;

                    bundle.referencedByGroups.Add(group);
                    group.bundles.Add(bundle);
                }
            }
        }

        public Archive FindBundle(string bundleName)
        {
            foreach(var bundle in bundles)
            {
                if (string.Equals(bundle.name, bundleName, System.StringComparison.OrdinalIgnoreCase))
                    return bundle;
            }

            return null;
        }

        public Group FindGroup(string groupName)
        {
            foreach(var group in groups)
            {
                if (string.Equals(group.name, groupName, System.StringComparison.OrdinalIgnoreCase))
                    return group;
            }

            return null;
        }
    }
}
