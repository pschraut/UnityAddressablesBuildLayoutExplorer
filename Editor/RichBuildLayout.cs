//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oddworm.EditorFramework
{
    /// <summary>
    /// The <see cref="RichBuildLayout"/> class provides a higher-level abstraction of the <see cref="BuildLayout"/>
    /// and provides additional data.
    /// </summary>
    public class RichBuildLayout
    {
        public BuildLayout lowlevel; // a reference to the underlaying object
        public string unityVersion = "";
        public string addressablesVersion = "";
        public List<Group> groups = new List<Group>();
        public Dictionary<string, Archive> bundles = new Dictionary<string, Archive>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Asset> assets = new Dictionary<string, Asset>(StringComparer.OrdinalIgnoreCase);

        public class Group
        {
            public string name = "";
            public long size;
            public List<Archive> bundles = new List<Archive>();
            public BuildLayout.Group lowlevel; // a reference to the underlaying object
        }

        public class Archive
        {
            public bool isBuiltin;
            public string name = "";
            public long size;
            public string compression = "";
            public long assetBundleObjectSize;
            public List<Archive> bundleDependencies = new List<Archive>();
            public List<Archive> expandedBundleDependencies = new List<Archive>();
            public List<Asset> explicitAssets = new List<Asset>();
            public BuildLayout.Archive lowlevel; // a reference to the underlaying object
            public List<Group> referencedByGroups = new List<Group>();
            public List<Archive> referencedByBundles = new List<Archive>();
            public List<Asset> allAssets = new List<Asset>();
        }

        public class Asset
        {
            public string uid;
            public string name;
            public long size;
            public long sizeFromObjects;
            public long sizeFromStreamedData;
            public string address;
            public List<Asset> externalReferences = new List<Asset>();
            public List<Asset> internalReferences = new List<Asset>();
            public BuildLayout.ExplicitAsset lowlevel; // a reference to the underlaying object
            public List<Archive> referencedByBundle = new List<Archive>();

            public bool isEmbedded;
            public Asset includedByAsset;
            public Archive includedInBundle;
        }

        public RichBuildLayout()
        {
        }

        string GetUID(string bundleName, string assetName)
        {
            return $"{bundleName}###{assetName}";
        }

        public RichBuildLayout(BuildLayout buildLayout)
        {
            lowlevel = buildLayout;
            unityVersion = buildLayout.unityVersion;
            addressablesVersion = buildLayout.addressablesVersion;

            // collect builtin bundles
            foreach (var baseBundle in buildLayout.builtinBundles)
            {
                var bundle = FindBundle(baseBundle.name);
                if (bundle != null)
                    continue;

                bundle = new Archive
                {
                    lowlevel = baseBundle,
                    name = baseBundle.name,
                    size = baseBundle.size,
                    compression = baseBundle.compression.ToUpper(),
                    assetBundleObjectSize = baseBundle.assetBundleObjectSize,
                    isBuiltin = true
                };
                bundles.Add(bundle.name, bundle);
            }

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
                        compression = baseBundle.compression.ToUpper(),
                        assetBundleObjectSize = baseBundle.assetBundleObjectSize
                    };
                    bundles.Add(bundle.name, bundle);
                }
            }

            // resolve bundle dependencies
            foreach(var bundle in bundles.Values)
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
                    bundleDependency.referencedByBundles.Add(bundle);
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
                    bundleDependency.referencedByBundles.Add(bundle);
                }
            }

            // collect all assets
            foreach (var bundle in bundles.Values)
            {
                foreach (var baseAsset in bundle.lowlevel.explicitAssets)
                {
                    var asset = FindAsset(baseAsset.name);
                    if (asset == null)
                    {
                        asset = new Asset()
                        {
                            lowlevel = baseAsset,
                            uid = GetUID(bundle.name, baseAsset.name),
                            name = baseAsset.name,
                            size = baseAsset.size,
                            sizeFromObjects = baseAsset.sizeFromObjects,
                            sizeFromStreamedData = baseAsset.sizeFromStreamedData,
                            address = baseAsset.address,
                            //externalReferences = new List<string>(baseAsset.externalReferences),
                            //internalReferences = new List<string>(baseAsset.internalReferences)
                            includedInBundle = bundle
                        };
                        assets.Add(asset.uid, asset);
                    }
                    bundle.explicitAssets.Add(asset);
                    bundle.allAssets.Add(asset);
                    //asset.referencedByBundle.Add(bundle);

                    foreach (var internalRef in baseAsset.internalReferences)
                    {
                        BuildLayout.ExplicitAsset internalBaseAsset = null;
                        foreach(var file in bundle.lowlevel.files)
                        {
                            foreach(var a in file.assets)
                            {
                                if (string.Equals(a.name, internalRef, System.StringComparison.OrdinalIgnoreCase))
                                {
                                    internalBaseAsset = a;
                                    break;
                                }
                            }
                        }

                        if (internalBaseAsset == null)
                        {
                            Debug.LogError($"Could not find '{internalRef}'");
                            continue;
                        }

                        var internalAsset = FindAsset(GetUID(bundle.name, internalRef));
                        if (internalAsset == null)
                        {
                            internalAsset = new Asset()
                            {
                                lowlevel = internalBaseAsset,
                                uid = GetUID(bundle.name, internalRef),
                                name = internalRef,
                                size = internalBaseAsset.size,
                                sizeFromObjects = internalBaseAsset.sizeFromObjects,
                                sizeFromStreamedData = internalBaseAsset.sizeFromStreamedData,
                                isEmbedded = true,
                                includedByAsset = asset,
                                includedInBundle = bundle
                            };
                            //internalAsset.referencedByBundle.Add(bundle);
                            assets.Add(internalAsset.uid, internalAsset);
                            bundle.allAssets.Add(internalAsset);
                        }
                        asset.internalReferences.Add(internalAsset);
                    }
                }
            }

            // resolve external asset references
            foreach(var asset in assets.Values)
            {
                foreach (var baseReferece in asset.lowlevel.externalReferences)
                {
                    var reference = FindAsset(baseReferece);
                    if (reference == null)
                        continue;

                    asset.externalReferences.Add(reference);
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

        Asset FindAsset(string uid)
        {
            if (assets.TryGetValue(uid, out var asset))
            {
                return asset;
            }

            return null;
        }

        Archive FindBundle(string bundleName)
        {
            if (bundles.TryGetValue(bundleName, out var bundle))
            {
                return bundle;
            }
            
            return null;
        }

        Group FindGroup(string groupName)
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
