using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TLNexus.ProjectAssetData
{
    /// <summary>
    /// 引用索引快照，写在 Library 下。全量重写基础段；增量只在文件末尾追加变更。
    /// 反向引用由依赖表在读取时重建，不单独落盘。
    /// </summary>
    public static class ProjectAssetReferenceIndexStore
    {
        private const int Version = 3;
        private const int HeaderSize = 16;
        private const byte OpUpsert = 1;
        private const byte OpDelete = 2;
        private const long JournalLimit = 3L * 1024L * 1024L;
        private static readonly byte[] Magic = { (byte)'P', (byte)'A', (byte)'D', (byte)'3' };

        private static readonly string SnapshotFilePath = GetSnapshotFilePath();

        public static bool TryLoad(
            Dictionary<string, List<string>> pathTarget,
            Dictionary<string, int> guidCountTarget,
            Dictionary<string, List<string>> dependencyTarget,
            out Dictionary<string, long> writeTimeTicksOut,
            out List<string> indexedPathsOut)
        {
            writeTimeTicksOut = null;
            indexedPathsOut = null;

            if (pathTarget == null || guidCountTarget == null || dependencyTarget == null)
                return false;

            pathTarget.Clear();
            guidCountTarget.Clear();
            dependencyTarget.Clear();

            if (!File.Exists(SnapshotFilePath))
                return false;

            try
            {
                using (var stream = new FileStream(SnapshotFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536))
                using (var reader = new BinaryReader(stream))
                {
                    if (!MagicMatches(reader.ReadBytes(Magic.Length)) || reader.ReadInt32() != Version)
                        return false;

                    long baseLength = reader.ReadInt64();
                    if (baseLength < HeaderSize || baseLength > stream.Length)
                        return false;

                    int pathCount = reader.ReadInt32();
                    if (pathCount < 0 || pathCount > 5000000)
                        return false;

                    var paths = new string[pathCount];
                    for (int i = 0; i < pathCount; i++)
                        paths[i] = reader.ReadString();

                    var writeTimes = new Dictionary<string, long>(pathCount, StringComparer.OrdinalIgnoreCase);
                    if (!ReadDependencies(reader, paths, dependencyTarget, writeTimes))
                        return Fail(pathTarget, guidCountTarget, dependencyTarget);

                    int guidCount = reader.ReadInt32();
                    if (guidCount < 0 || guidCount > 5000000)
                        return Fail(pathTarget, guidCountTarget, dependencyTarget);
                    for (int i = 0; i < guidCount; i++)
                    {
                        string guid = reader.ReadString();
                        int count = reader.ReadInt32();
                        if (!string.IsNullOrEmpty(guid) && count > 0)
                            guidCountTarget[guid] = count;
                    }

                    int indexedCount = reader.ReadInt32();
                    if (indexedCount < 0 || indexedCount > 5000000)
                        return Fail(pathTarget, guidCountTarget, dependencyTarget);
                    var indexedPaths = new List<string>(indexedCount);
                    var indexedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < indexedCount; i++)
                    {
                        int index = reader.ReadInt32();
                        if (index < 0 || index >= paths.Length || string.IsNullOrEmpty(paths[index]))
                            return Fail(pathTarget, guidCountTarget, dependencyTarget);
                        if (indexedSet.Add(paths[index]))
                            indexedPaths.Add(paths[index]);
                    }

                    if (!ApplyJournal(baseLength, dependencyTarget, guidCountTarget, writeTimes, indexedPaths, indexedSet))
                        return Fail(pathTarget, guidCountTarget, dependencyTarget);

                    RebuildReverse(dependencyTarget, pathTarget);
                    writeTimeTicksOut = writeTimes;
                    indexedPathsOut = indexedPaths;
                    return dependencyTarget.Count > 0 || indexedPaths.Count > 0;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Project asset reference index load failed: {exception.Message}");
                return Fail(pathTarget, guidCountTarget, dependencyTarget);
            }
        }

        public static bool TryAppend(
            List<string> changed,
            List<string> deleted,
            Dictionary<string, List<string>> dependencySource,
            Dictionary<string, long> writeTimeTicksSource,
            out bool journalExceeded)
        {
            journalExceeded = false;
            if ((changed == null || changed.Count == 0) && (deleted == null || deleted.Count == 0))
                return false;
            if (!File.Exists(SnapshotFilePath))
                return false;

            try
            {
                long baseLength;
                using (var read = new FileStream(SnapshotFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 16))
                using (var reader = new BinaryReader(read))
                {
                    if (!MagicMatches(reader.ReadBytes(Magic.Length)) || reader.ReadInt32() != Version)
                        return false;
                    baseLength = reader.ReadInt64();
                    if (baseLength < HeaderSize || baseLength > read.Length)
                        return false;
                }

                using (var stream = new FileStream(SnapshotFilePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096))
                using (var writer = new BinaryWriter(stream))
                {
                    if (deleted != null)
                    {
                        for (int i = 0; i < deleted.Count; i++)
                        {
                            writer.Write(OpDelete);
                            writer.Write(deleted[i] ?? string.Empty);
                        }
                    }

                    if (changed != null)
                    {
                        for (int i = 0; i < changed.Count; i++)
                        {
                            string path = changed[i];
                            writer.Write(OpUpsert);
                            writer.Write(path ?? string.Empty);
                            long writeTime = 0;
                            if (writeTimeTicksSource != null && path != null)
                                writeTimeTicksSource.TryGetValue(path, out writeTime);
                            writer.Write(writeTime);

                            List<string> deps = null;
                            if (dependencySource != null && path != null)
                                dependencySource.TryGetValue(path, out deps);
                            int depCount = deps != null ? deps.Count : 0;
                            writer.Write(depCount);
                            for (int d = 0; d < depCount; d++)
                                writer.Write(deps[d] ?? string.Empty);
                        }
                    }
                }

                long journalLength = new FileInfo(SnapshotFilePath).Length - baseLength;
                journalExceeded = journalLength > JournalLimit;
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Project asset reference index append failed: {exception.Message}");
                return false;
            }
        }

        public static bool Save(
            Dictionary<string, int> guidCountSource,
            Dictionary<string, List<string>> dependencySource,
            Dictionary<string, long> writeTimeTicksSource,
            List<string> indexedPathsSource)
        {
            string tempPath = SnapshotFilePath + ".tmp";
            try
            {
                string directory = Path.GetDirectoryName(SnapshotFilePath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                var pathToIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var paths = new List<string>(8192);
                CollectPaths(dependencySource, pathToIndex, paths);
                if (indexedPathsSource != null)
                {
                    for (int i = 0; i < indexedPathsSource.Count; i++)
                        InternPath(indexedPathsSource[i], pathToIndex, paths);
                }

                using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 65536))
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(Magic);
                    writer.Write(Version);
                    writer.Write(0L);
                    writer.Write(paths.Count);
                    for (int i = 0; i < paths.Count; i++)
                        writer.Write(paths[i]);

                    WriteDependencies(writer, dependencySource, pathToIndex, writeTimeTicksSource);

                    int guidCount = guidCountSource != null ? guidCountSource.Count : 0;
                    writer.Write(guidCount);
                    if (guidCountSource != null)
                    {
                        foreach (var pair in guidCountSource)
                        {
                            writer.Write(pair.Key ?? string.Empty);
                            writer.Write(pair.Value);
                        }
                    }

                    int indexedCount = indexedPathsSource != null ? indexedPathsSource.Count : 0;
                    writer.Write(indexedCount);
                    if (indexedPathsSource != null)
                    {
                        for (int i = 0; i < indexedPathsSource.Count; i++)
                        {
                            pathToIndex.TryGetValue(indexedPathsSource[i], out int index);
                            writer.Write(index);
                        }
                    }

                    writer.Flush();
                    long baseLength = stream.Length;
                    stream.Position = 8;
                    stream.Write(BitConverter.GetBytes(baseLength), 0, 8);
                }

                if (File.Exists(SnapshotFilePath))
                    File.Replace(tempPath, SnapshotFilePath, null);
                else
                    File.Move(tempPath, SnapshotFilePath);

                string jsonPath = Path.Combine(directory ?? string.Empty, "reference-index.json");
                if (File.Exists(jsonPath))
                    File.Delete(jsonPath);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Project asset reference index save failed: {exception.Message}");
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
                return false;
            }
        }

        public static void Delete()
        {
            TryDelete(SnapshotFilePath);
            string directory = Path.GetDirectoryName(SnapshotFilePath);
            TryDelete(Path.Combine(directory ?? string.Empty, "reference-index.json"));
        }

        private static bool ApplyJournal(
            long baseLength,
            Dictionary<string, List<string>> dependencyTarget,
            Dictionary<string, int> guidCountTarget,
            Dictionary<string, long> writeTimes,
            List<string> indexedPaths,
            HashSet<string> indexedSet)
        {
            var info = new FileInfo(SnapshotFilePath);
            if (info.Length <= baseLength)
                return true;

            long journalLength = info.Length - baseLength;
            if (journalLength > int.MaxValue)
                return false;

            byte[] data = new byte[journalLength];
            using (var stream = new FileStream(SnapshotFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536))
            {
                stream.Position = baseLength;
                int got = 0;
                while (got < data.Length)
                {
                    int read = stream.Read(data, got, data.Length - got);
                    if (read <= 0)
                        return false;
                    got += read;
                }
            }

            using (var memory = new MemoryStream(data, false))
            using (var reader = new BinaryReader(memory))
            {
                while (true)
                {
                    byte op;
                    try
                    {
                        op = reader.ReadByte();
                    }
                    catch (EndOfStreamException)
                    {
                        return true;
                    }

                    string path = reader.ReadString();
                    if (string.IsNullOrEmpty(path))
                        return false;

                    if (op == OpDelete)
                    {
                        RemoveDependency(dependencyTarget, guidCountTarget, writeTimes, path);
                        RemoveIndexed(indexedPaths, indexedSet, path);
                        continue;
                    }

                    if (op != OpUpsert)
                        return false;

                    long writeTime = reader.ReadInt64();
                    int depCount = reader.ReadInt32();
                    if (depCount < 0 || depCount > 5000000)
                        return false;
                    var deps = new List<string>(depCount);
                    for (int i = 0; i < depCount; i++)
                        deps.Add(reader.ReadString());

                    RemoveDependency(dependencyTarget, guidCountTarget, writeTimes, path);
                    dependencyTarget[path] = deps;
                    AddCounts(guidCountTarget, deps, 1);
                    if (writeTime > 0)
                        writeTimes[path] = writeTime;
                    if (indexedSet.Add(path))
                        indexedPaths.Add(path);
                }
            }
        }

        private static void RebuildReverse(
            Dictionary<string, List<string>> dependencySource,
            Dictionary<string, List<string>> pathTarget)
        {
            foreach (var pair in dependencySource)
            {
                List<string> deps = pair.Value;
                if (deps == null)
                    continue;
                for (int i = 0; i < deps.Count; i++)
                {
                    string dep = deps[i];
                    if (string.IsNullOrEmpty(dep))
                        continue;
                    if (!pathTarget.TryGetValue(dep, out List<string> refs))
                    {
                        refs = new List<string>(4);
                        pathTarget[dep] = refs;
                    }
                    refs.Add(pair.Key);
                }
            }
        }

        private static void RemoveDependency(
            Dictionary<string, List<string>> dependencyTarget,
            Dictionary<string, int> guidCountTarget,
            Dictionary<string, long> writeTimes,
            string path)
        {
            if (!dependencyTarget.TryGetValue(path, out List<string> old))
                return;

            AddCounts(guidCountTarget, old, -1);
            dependencyTarget.Remove(path);
            writeTimes.Remove(path);
        }

        private static void AddCounts(Dictionary<string, int> guidCountTarget, List<string> deps, int delta)
        {
            if (deps == null)
                return;

            for (int i = 0; i < deps.Count; i++)
            {
                string guid = AssetDatabase.AssetPathToGUID(deps[i]);
                if (string.IsNullOrEmpty(guid))
                    continue;

                guidCountTarget.TryGetValue(guid, out int count);
                count += delta;
                if (count <= 0)
                    guidCountTarget.Remove(guid);
                else
                    guidCountTarget[guid] = count;
            }
        }

        private static void RemoveIndexed(List<string> indexedPaths, HashSet<string> indexedSet, string path)
        {
            if (!indexedSet.Remove(path))
                return;

            for (int i = indexedPaths.Count - 1; i >= 0; i--)
            {
                if (string.Equals(indexedPaths[i], path, StringComparison.OrdinalIgnoreCase))
                {
                    indexedPaths.RemoveAt(i);
                    return;
                }
            }
        }

        private static bool ReadDependencies(
            BinaryReader reader,
            string[] paths,
            Dictionary<string, List<string>> target,
            Dictionary<string, long> writeTimes)
        {
            int entryCount = reader.ReadInt32();
            if (entryCount < 0 || entryCount > 5000000)
                return false;

            for (int i = 0; i < entryCount; i++)
            {
                int pathIndex = reader.ReadInt32();
                int edgeCount = reader.ReadInt32();
                if (pathIndex < 0 || pathIndex >= paths.Length || edgeCount < 0 || edgeCount > 5000000)
                    return false;

                var edges = new List<string>(edgeCount);
                for (int e = 0; e < edgeCount; e++)
                {
                    int edgeIndex = reader.ReadInt32();
                    if (edgeIndex < 0 || edgeIndex >= paths.Length)
                        return false;
                    edges.Add(paths[edgeIndex]);
                }

                long writeTime = reader.ReadInt64();
                string path = paths[pathIndex];
                if (string.IsNullOrEmpty(path))
                    return false;
                target[path] = edges;
                if (writeTime > 0)
                    writeTimes[path] = writeTime;
            }

            return true;
        }

        private static void WriteDependencies(
            BinaryWriter writer,
            Dictionary<string, List<string>> source,
            Dictionary<string, int> pathToIndex,
            Dictionary<string, long> writeTimeTicksSource)
        {
            int entryCount = source != null ? source.Count : 0;
            writer.Write(entryCount);
            if (source == null)
                return;

            foreach (var pair in source)
            {
                pathToIndex.TryGetValue(pair.Key, out int pathIndex);
                writer.Write(pathIndex);
                List<string> edges = pair.Value;
                int edgeCount = edges != null ? edges.Count : 0;
                writer.Write(edgeCount);
                for (int i = 0; i < edgeCount; i++)
                {
                    pathToIndex.TryGetValue(edges[i], out int edgeIndex);
                    writer.Write(edgeIndex);
                }

                long writeTime = 0;
                if (writeTimeTicksSource != null)
                    writeTimeTicksSource.TryGetValue(pair.Key, out writeTime);
                writer.Write(writeTime);
            }
        }

        private static void CollectPaths(
            Dictionary<string, List<string>> source,
            Dictionary<string, int> pathToIndex,
            List<string> paths)
        {
            if (source == null)
                return;

            foreach (var pair in source)
            {
                InternPath(pair.Key, pathToIndex, paths);
                List<string> edges = pair.Value;
                if (edges == null)
                    continue;
                for (int i = 0; i < edges.Count; i++)
                    InternPath(edges[i], pathToIndex, paths);
            }
        }

        private static void InternPath(string path, Dictionary<string, int> pathToIndex, List<string> paths)
        {
            if (string.IsNullOrEmpty(path) || pathToIndex.ContainsKey(path))
                return;

            pathToIndex.Add(path, paths.Count);
            paths.Add(path);
        }

        private static bool MagicMatches(byte[] magic)
        {
            return magic != null
                && magic.Length == Magic.Length
                && magic[0] == Magic[0]
                && magic[1] == Magic[1]
                && magic[2] == Magic[2]
                && magic[3] == Magic[3];
        }

        private static bool Fail(
            Dictionary<string, List<string>> pathTarget,
            Dictionary<string, int> guidCountTarget,
            Dictionary<string, List<string>> dependencyTarget)
        {
            pathTarget.Clear();
            guidCountTarget.Clear();
            dependencyTarget.Clear();
            return false;
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Project asset reference index delete failed: {exception.Message}");
            }
        }

        private static string GetSnapshotFilePath()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            return string.IsNullOrEmpty(projectRoot)
                ? Path.Combine("Library", "TLNEXUS", "ProjectAssetData", "reference-index.bin")
                : Path.Combine(projectRoot, "Library", "TLNEXUS", "ProjectAssetData", "reference-index.bin");
        }
    }
}
