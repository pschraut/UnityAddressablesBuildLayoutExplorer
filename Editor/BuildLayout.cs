//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Oddworm.EditorFramework
{
    [System.Serializable]
    public class BuildLayout
    {
        public string unityVersion;
        public string addressablesVersion;
        public List<Group> groups = new List<Group>();

        [System.Serializable]
        public class Group
        {
            public string name;
            public long size;
            public List<Archive> bundles = new List<Archive>();
        }

        [System.Serializable]
        public class Archive
        {
            public string name;
            public long size;
            public List<string> bundleDependencies = new List<string>();
        }

        public static BuildLayout Load(string path)
        {
            var text = System.IO.File.ReadAllText(path);
            return Parse(text);
        }

        public static BuildLayout Parse(string text)
        {
            var layout = new BuildLayout();

            // Convert text to lines for easier processing
            var lines = new List<string>();
            foreach (var line in text.Split(new[] { '\n' }))
            {
                if (!string.IsNullOrWhiteSpace(line))
                    lines.Add(line);
            }

            for (var n = 0; n < lines.Count; ++n)
            {
                var line = lines[n];

                if (line.StartsWith("Unity Version:", System.StringComparison.Ordinal))
                {
                    layout.unityVersion = line.Substring("Unity Version:".Length).Trim();
                    continue;
                }

                if (line.StartsWith("com.unity.addressables:", System.StringComparison.Ordinal))
                {
                    layout.addressablesVersion = line.Substring("com.unity.addressables:".Length).Trim();
                    continue;
                }

                if (line.StartsWith("Group ", System.StringComparison.Ordinal))
                {
                    var group = ReadGroup(ref n);
                    layout.groups.Add(group);
                    continue;
                }
            }

            // Sort groups alphabetically
            layout.groups.Sort(delegate (Group a, Group b)
            {
                return a.name.CompareTo(b.name);
            });

            return layout;

            Group ReadGroup(ref int index)
            {
                // Group LR-globals (Bundles: 16, Total Size: 128.43KB, Explicit Asset Count: 280)
                var group = new Group();
                var groupLine = lines[index];
                var groupIndend = GetIndend(groupLine);

                // Read the group name
                var groupName = groupLine;
                groupName = groupName.Substring(groupName.IndexOf("Group ") + "Group ".Length); // Remove everything before and including "Group"
                groupName = groupName.Substring(0, groupName.IndexOf(" (Bundles:")); // Remove everything after and including " (Bundles:"
                groupName = groupName.Trim();
                group.name = groupName;

                // Read the group size
                var groupSize = groupLine;
                groupSize = groupSize.Substring(groupSize.IndexOf("Total Size: ") + "Total Size: ".Length); // Remove everything before and including "Total Size: "
                groupSize = groupSize.Substring(0, groupSize.IndexOf(", ")); // Remove everything after ", "
                group.size = ParseSize(groupSize);

                // Iterate over each group line
                var loopguard = 0;
                index++;
                for (; index < lines.Count; ++index)
                {
                loop:
                    if (++loopguard > 30000)
                    {
                        Debug.LogError($"loopguard");
                        break;
                    }

                    var line = lines[index];
                    var lineIndend = GetIndend(line);
                    if (lineIndend <= groupIndend)
                    {
                        index--;
                        return group;
                    }

                    if (line.StartsWith("\tSchemas"))
                    {
                        SkipSchemas(ref index);
                        goto loop;
                    }

                    if (line.StartsWith("\tArchive"))
                    {
                        var archive = ReadArchive(ref index);
                        group.bundles.Add(archive);
                        goto loop;
                    }
                }

                return group;
            }

            void SkipSchemas(ref int index)
            {
                var schemasLevel = GetIndend(lines[index]);

                for (index++; index < lines.Count; ++index)
                {
                    if (GetIndend(lines[index]) <= schemasLevel)
                        break;
                }
            }

            Archive ReadArchive(ref int index)
            {
                var archive = new Archive();

                // Archive lr-globals_assets_assets/configs/balancing_eea0e08713731a51c6aac94795f26fc8.bundle (Size: 10.07KB, Compression: Lz4HC, Asset Bundle Object Size: 1.76KB)
                var archiveLine = lines[index];
                var archiveIndend = GetIndend(archiveLine);

                // Read the archive name
                var archiveName = archiveLine;
                archiveName = archiveName.Substring(archiveName.IndexOf("Archive") + "Archive".Length); // Remove everything before and including "Archive"
                archiveName = archiveName.Substring(0, archiveName.IndexOf("(Size:")); // Remove everything after and including "(Size:"
                archiveName = archiveName.Trim();
                archive.name = archiveName;

                // Read the archive size
                var archiveSize = archiveLine;
                archiveSize = archiveSize.Substring(archiveSize.IndexOf("(Size: ") + "(Size: ".Length); // Remove everything before and including "(Size: "
                archiveSize = archiveSize.Substring(0, archiveSize.IndexOf(", ")); // Remove everything after ", "
                archive.size = ParseSize(archiveSize);


                var loopguard = 0;

                for (index++; index < lines.Count - 1; ++index)
                {
                    if (++loopguard > 30000)
                    {
                        Debug.LogError($"loopguard");
                        break;
                    }

                    if (GetIndend(lines[index + 1]) <= archiveIndend)
                        break;

                    var trimmedLine = lines[index].Trim();
                    if (trimmedLine.StartsWith("Bundle Dependencies", System.StringComparison.OrdinalIgnoreCase))
                    {
                        archive.bundleDependencies.AddRange(ReadBundleDependencies(ref index));
                        continue;
                    }

                    if (trimmedLine.StartsWith("Expanded Bundle Dependencies", System.StringComparison.OrdinalIgnoreCase))
                    {

                    }

                    if (trimmedLine.StartsWith("Explicit Assets", System.StringComparison.OrdinalIgnoreCase))
                    {

                    }

                    if (trimmedLine.StartsWith("Files", System.StringComparison.OrdinalIgnoreCase))
                    {

                    }
                }

                return archive;
            }

            List<string> ReadBundleDependencies(ref int index)
            {
                var bundlesLine = lines[index];
                bundlesLine = bundlesLine.Substring(bundlesLine.IndexOf("Bundle Dependencies:") + "Bundle Dependencies:".Length); // Remove everything before and including "Bundle Dependencies:"
                bundlesLine = bundlesLine.Trim();

                var bundles = new List<string>();
                foreach (var b in bundlesLine.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
                    bundles.Add(b.Trim());

                bundles.Sort();
                return bundles;
            }

            long ParseSize(string size)
            {
                if (size.EndsWith("GB", System.StringComparison.OrdinalIgnoreCase))
                {
                    var s = size.Substring(0, size.Length - 2);
                    return (long)(float.Parse(s) * 1024 * 1024 * 1024);
                }

                if (size.EndsWith("MB", System.StringComparison.OrdinalIgnoreCase))
                {
                    var s = size.Substring(0, size.Length - 2);
                    return (long)(float.Parse(s) * 1024 * 1024);
                }

                if (size.EndsWith("KB", System.StringComparison.OrdinalIgnoreCase))
                {
                    var s = size.Substring(0, size.Length - 2);
                    return (long)(float.Parse(s) * 1024);
                }

                if (size.EndsWith("B", System.StringComparison.OrdinalIgnoreCase))
                {
                    var s = size.Substring(0, size.Length - 1);
                    return long.Parse(s);
                }

                return -1;
            }

            int GetIndend(string s)
            {
                int count = 0;
                for (var n = 0; n < s.Length; ++n)
                {
                    if (s[n] == '\t')
                        count++;
                    else
                        break;
                }
                return count;
            }
        }
    }

}
