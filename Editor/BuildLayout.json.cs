//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

#if ADDRESSABLES_JSON_SUPPORT
using UnityEditor.AddressableAssets.Build.Layout;
#endif

namespace Oddworm.EditorFramework
{
#if ADDRESSABLES_JSON_SUPPORT
    public partial class BuildLayout
    {
        public static BuildLayout ParseJson(string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
                throw new System.ArgumentException($"argument '{nameof(jsonText)}' must not be empty.");

            var json = JsonUtility.FromJson<UnityEditor.AddressableAssets.Build.Layout.BuildLayout>(jsonText);
            var layout = new BuildLayout();
            var allbundles = new Dictionary<UnityEditor.AddressableAssets.Build.Layout.BuildLayout.Bundle, Archive>();

            layout.unityVersion = json.UnityVersion;
            layout.addressablesVersion = json.PackageVersion;

            foreach (var b in json.BuiltInBundles)
                layout.builtinBundles.Add(ToArchive(b));

            foreach(var g in json.Groups)
                layout.groups.Add(ToGroup(g));


            return layout;


            Archive ToArchive(UnityEditor.AddressableAssets.Build.Layout.BuildLayout.Bundle b)
            {
                var archive = new Archive();
                archive.name = b.Name;
                archive.size = (long)b.FileSize;
                archive.compression = b.Compression;
                archive.assetBundleObjectSize = (long)b.FileSize;

                foreach(var d in b.Dependencies)
                    archive.bundleDependencies.Add(d.Name);

                foreach (var d in b.ExpandedDependencies)
                    archive.expandedBundleDependencies.Add(d.Name);

                var lookup = new Dictionary<string, bool>();
                foreach (var f in b.Files)
                {
                    var file = ToFile(f);
                    archive.files.Add(file);

                    foreach (var ff in file.assets)
                    {
                        if (lookup.ContainsKey(ff.name))
                            continue;
                        lookup.Add(ff.name, true);
                        archive.explicitAssets.Add(ff);
                    }
                }

                return archive;
            }

            File ToFile(UnityEditor.AddressableAssets.Build.Layout.BuildLayout.File f)
            {
                var file = new File();
                file.name = f.Name;
                file.monoScriptCount = f.MonoScriptCount;
                file.monoScriptSize = (long)f.MonoScriptSize;

                foreach (var a in f.Assets)
                    file.assets.Add(ToExplicitAsset(a));

                foreach (var a in f.OtherAssets)
                    file.assets.Add(ToExplicitOtherAsset(a));

                return file;
            }

            ExplicitAsset ToExplicitOtherAsset(UnityEditor.AddressableAssets.Build.Layout.BuildLayout.DataFromOtherAsset a)
            {
                var asset = new ExplicitAsset();
                asset.name = a.AssetPath;
                asset.address = "";
                asset.size = (long)(a.StreamedSize + a.SerializedSize);
                asset.sizeFromStreamedData = (long)a.StreamedSize;
                asset.sizeFromObjects = (long)a.SerializedSize;

                //foreach (var s in a.ReferencingAssets)
                //    asset.externalReferences.Add(s.AssetPath);

                return asset;
            }

            ExplicitAsset ToExplicitAsset(UnityEditor.AddressableAssets.Build.Layout.BuildLayout.ExplicitAsset a)
            {
                var asset = new ExplicitAsset();
                asset.name = a.AssetPath;
                asset.address = a.AddressableName;
                asset.size = (long)a.SerializedSize;
                asset.sizeFromStreamedData = (long)a.StreamedSize;
                asset.sizeFromObjects = (long)a.SerializedSize;

                foreach (var s in a.ExternallyReferencedAssets)
                    asset.externalReferences.Add(s.AssetPath);

                foreach (var s in a.InternalReferencedExplicitAssets)
                    asset.internalReferences.Add(s.AssetPath);

                foreach (var s in a.InternalReferencedOtherAssets)
                    asset.internalReferences.Add(s.AssetPath);

                return asset;
            }


            Group ToGroup(UnityEditor.AddressableAssets.Build.Layout.BuildLayout.Group g)
            {
                var group = new Group();
                group.name = g.Name;

                foreach (var b in g.Bundles)
                {
                    group.size += (long)b.FileSize;
                    group.bundles.Add(ToArchive(b));
                }

                return group;
            }
        }
    }
#else // ADDRESSABLES_JSON_SUPPORT
    public partial class BuildLayout { }
#endif
}
