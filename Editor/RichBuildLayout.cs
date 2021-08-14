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
    public class RichBuildLayout
    {
        public BuildLayout lowlevel; // a reference to the underlaying object
        public string unityVersion = "";
        public string addressablesVersion = "";
        public List<Group> groups = new List<Group>();
        public List<Archive> bundles = new List<Archive>();
        public List<Asset> assets = new List<Asset>();

        public class Group
        {
            public string name = "";
            public long size;
            public List<Archive> bundles = new List<Archive>();
            public BuildLayout.Group lowlevel; // a reference to the underlaying object
        }

        public class Archive
        {
            public string name = "";
            public long size;
            public string compression = "";
            public long assetBundleObjectSize;
            public List<Archive> bundleDependencies = new List<Archive>();
            public List<Archive> expandedBundleDependencies = new List<Archive>();
            public List<Asset> explicitAssets = new List<Asset>();
            public BuildLayout.Archive lowlevel; // a reference to the underlaying object
            public List<Group> referencedByGroups = new List<Group>();
        }

        public class Asset
        {
            public string name;
            public long size;
            public long sizeFromObjects;
            public long sizeFromStreamedData;
            public string address;
            public List<string> externalReferences = new List<string>();
            public List<string> internalReferences = new List<string>();
            public BuildLayout.ExplicitAsset lowlevel; // a reference to the underlaying object
            public List<Archive> referencedByBundle = new List<Archive>();
        }

        public RichBuildLayout()
        {
        }

        public RichBuildLayout(BuildLayout buildLayout)
        {
            lowlevel = buildLayout;
            unityVersion = buildLayout.unityVersion;
            addressablesVersion = buildLayout.addressablesVersion;

            // collect all bundles
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
                        size = baseBundle.size,
                        compression = baseBundle.compression,
                        assetBundleObjectSize = baseBundle.assetBundleObjectSize
                    };
                    bundles.Add(bundle);
                }
            }

            // resolve bundle dependencies
            foreach(var bundle in bundles)
            {
                foreach (var baseBundle in bundle.lowlevel.bundleDependencies)
                {
                    var bundleDependency = FindBundle(baseBundle);
                    if (bundleDependency == null)
                    {
                        //Debug.LogError($"Cannot resolve bundle dependency to '{baseBundle}' in bundle '{bundle.name}'.");
                        continue;
                    }
                    bundle.bundleDependencies.Add(bundleDependency);
                }

                foreach (var baseBundle in bundle.lowlevel.expandedBundleDependencies)
                {
                    var bundleDependency = FindBundle(baseBundle);
                    if (bundleDependency == null)
                    {
                        //Debug.LogError($"Cannot resolve bundle dependency to '{baseBundle}' in bundle '{bundle.name}'.");
                        continue;
                    }
                    bundle.expandedBundleDependencies.Add(bundleDependency);
                }
            }

            // collect all assets
            foreach (var bundle in bundles)
            {
                foreach (var baseAsset in bundle.lowlevel.explicitAssets)
                {
                    var asset = FindAsset(baseAsset.name);
                    if (asset == null)
                    {
                        asset = new Asset()
                        {
                            lowlevel = baseAsset,
                            name = baseAsset.name,
                            size = baseAsset.size,
                            sizeFromObjects = baseAsset.sizeFromObjects,
                            sizeFromStreamedData = baseAsset.sizeFromStreamedData,
                            address = baseAsset.address,
                            externalReferences = new List<string>(baseAsset.externalReferences),
                            internalReferences = new List<string>(baseAsset.internalReferences)
                        };
                        assets.Add(asset);
                    }
                    bundle.explicitAssets.Add(asset);
                    asset.referencedByBundle.Add(bundle);
                }
            }


            // fill groups with earlier collected bundles
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
                    if (bundle == null)
                        continue;

                    bundle.referencedByGroups.Add(group);
                    group.bundles.Add(bundle);
                }
            }
        }

        public Asset FindAsset(string assetName)
        {
            foreach (var asset in assets)
            {
                if (string.Equals(asset.name, assetName, System.StringComparison.OrdinalIgnoreCase))
                    return asset;
            }

            return null;
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
