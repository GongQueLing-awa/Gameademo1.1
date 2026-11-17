using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TLNexus.ProjectAssetData
{
    /// <summary>
    /// The sole project-wide dependency and reverse-reference index.
    /// It is Editor-only and has no dependency on any product UI.
    /// The snapshot is not loaded at domain reload; it loads lazily on first
    /// call to any public member (documented: for startup performance).
    /// </summary>
    public static class ProjectAssetReferenceIndex
    {
        private const int BatchSize = 256;
        private const double BuildTimeBudgetSeconds = 0.02d;
        private static readonly string[] ScanRoots = { "Assets" };
        private static readonly ProfilerMarker FindAssetsMarker = new ProfilerMarker("PADI_FindAssets");
        private static readonly ProfilerMarker ClassifyPathsMarker = new ProfilerMarker("PADI_ClassifyPaths");
        private static readonly ProfilerMarker IncrementalScanMarker = new ProfilerMarker("PADI_IncrementalScan");
        private static readonly ProfilerMarker AssetProcessingMarker = new ProfilerMarker("PADI_ProcessBatch");
        private static readonly ProfilerMarker SnapshotSaveMarker = new ProfilerMarker("PADI_SaveSnapshot");
        private static readonly HashSet<string> ExcludedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".meta", ".cs", ".js", ".dll", ".asmdef", ".asmref", ".md", ".txt", ".pdf", ".uxml", ".uss"
        };
        private static readonly HashSet<string> LeafAssetExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".tga", ".jpg", ".jpeg", ".exr", ".psd", ".psb", ".tiff", ".tif", ".bmp", ".pict", ".hdr", ".iff", ".dds", ".pvr", ".astc", ".cr2", ".nef", ".xcf",
            ".wav", ".mp3", ".ogg", ".aiff", ".aif", ".xm", ".mod", ".it", ".s3m",
            ".mov", ".mp4", ".avi", ".webm", ".m4v", ".mpg", ".mpeg", ".ttf", ".otf", ".dfont", ".woff", ".woff2"
        };

        private sealed class CachedDependencies
        {
            public long writeTimeTicks;
            public string[] dependencies;
        }

        private static readonly Dictionary<string, int> referenceCountsByGuid = new Dictionary<string, int>(4096);
        private static readonly Dictionary<string, int> buildingReferenceCountsByGuid = new Dictionary<string, int>(4096);
        private static readonly Dictionary<string, List<string>> pathToReferencingPaths = new Dictionary<string, List<string>>(8192, StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, List<string>> buildingPathToReferencingPaths = new Dictionary<string, List<string>>(8192, StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, List<string>> pathToDependencyPaths = new Dictionary<string, List<string>>(8192, StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, List<string>> buildingPathToDependencyPaths = new Dictionary<string, List<string>>(8192, StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> guidByPath = new Dictionary<string, string>(8192, StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, CachedDependencies> dependencyCache = new Dictionary<string, CachedDependencies>(8192, StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> pendingChanged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> pendingDeleted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<string> journalChanged = new List<string>();
        private static readonly List<string> journalDeleted = new List<string>();
        private static readonly List<string> indexedPaths = new List<string>(4096);
        private static readonly Dictionary<string, int> indexedPathOrder = new Dictionary<string, int>(8192, StringComparer.OrdinalIgnoreCase);
        private static List<string> cachedIndexedPaths;
        private static bool isBuilding;
        private static bool isIncrementalBuild;
        private static bool hasCommittedSnapshot;
        private static bool hasBuildStatistics;
        private static bool hasPendingChanges;
        private static bool rebuildRequestedWhileBuilding;
        private static bool snapshotDirty;
        private static bool forceFullSnapshot;
        private static bool compactHooked;
        private static double compactDueTime = -1d;
        private const double CompactDelaySeconds = 5d;
        private static bool deferredRebuildHooked;
        private static int buildIndex;
        private static string[] classifyGuids;
        private static int classifyIndex;
        private static int prefabAssetCount;
        private static int sceneAssetCount;
        private static int otherAssetCount;
        private static double buildStartTime;
        private static int revision;
        private static bool s_initialized;

        public static bool IsBuilding => isBuilding;

        /// <summary>索引是否已加载。不触发加载——供 UI 绘制路径探测，避免重绘把惰性加载提前。</summary>
        public static bool IsLoaded => s_initialized;

        /// <summary>按需激活索引（首次调用时读取持久化快照）。UI 消费者在真正启用时调用。</summary>
        public static void Activate() => EnsureLoaded();
        public static bool HasCommittedSnapshot { get { EnsureLoaded(); return hasCommittedSnapshot; } }
        public static bool HasBuildStatistics { get { EnsureLoaded(); return hasBuildStatistics; } }
        public static bool HasPendingChanges { get { EnsureLoaded(); return hasPendingChanges; } }
        public static bool IsIndexFresh { get { EnsureLoaded(); return hasCommittedSnapshot && !hasPendingChanges && !isBuilding; } }
        public static int ProcessedAssetCount => buildIndex;
        public static int IndexedAssetCount { get { EnsureLoaded(); return indexedPaths.Count; } }
        public static int PrefabAssetCount { get { EnsureLoaded(); return prefabAssetCount; } }
        public static int SceneAssetCount { get { EnsureLoaded(); return sceneAssetCount; } }
        public static int OtherAssetCount { get { EnsureLoaded(); return otherAssetCount; } }
        public static float BuildProgress => isBuilding && indexedPaths.Count > 0 ? buildIndex / (float)indexedPaths.Count : 0f;
        public static int Revision => revision;

        private static void EnsureLoaded()
        {
            if (s_initialized) return;
            s_initialized = true;
            revision = 1;

            hasCommittedSnapshot = ProjectAssetReferenceIndexStore.TryLoad(
                pathToReferencingPaths,
                referenceCountsByGuid,
                pathToDependencyPaths,
                out Dictionary<string, long> writeTimeTicks,
                out cachedIndexedPaths);

            if (hasCommittedSnapshot && writeTimeTicks != null)
            {
                foreach (var pair in pathToDependencyPaths)
                {
                    string path = pair.Key;
                    if (writeTimeTicks.TryGetValue(path, out long ticks) && ticks > 0)
                    {
                        dependencyCache[path] = new CachedDependencies
                        {
                            writeTimeTicks = ticks,
                            dependencies = pair.Value.ToArray()
                        };
                    }
                }

                CountLoadedPaths();
                RebuildIndexedOrder();
                if (cachedIndexedPaths != null)
                {
                    indexedPaths.Clear();
                    indexedPaths.AddRange(cachedIndexedPaths);
                }
            }
            else
            {
                hasPendingChanges = !hasCommittedSnapshot;
                cachedIndexedPaths = null;
                indexedPathOrder.Clear();
            }

            EditorApplication.projectChanged += MarkDirty;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Undo.undoRedoPerformed += MarkDirty;
            EditorSceneManager.sceneSaved += scene => NoteChangedPath(scene.path);
        }

        private static void CountLoadedPaths()
        {
            prefabAssetCount = 0;
            sceneAssetCount = 0;
            otherAssetCount = 0;
            // 快照条目含被引用的贴图等叶子资源。括号里的分类必须只数索引源，
            // 否则「共 N 个」对不上「预制体 + 场景 + 其他」。
            IEnumerable<string> paths = cachedIndexedPaths != null && cachedIndexedPaths.Count > 0
                ? cachedIndexedPaths
                : pathToDependencyPaths.Keys;
            foreach (string path in paths)
            {
                if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) prefabAssetCount++;
                else if (path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)) sceneAssetCount++;
                else otherAssetCount++;
            }
            hasBuildStatistics = true;
        }

        public static bool TryGetReferenceCount(string guid, out int count)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(guid))
            {
                count = 0;
                return false;
            }

            return referenceCountsByGuid.TryGetValue(guid, out count);
        }

        public static bool TryGetReferencingPaths(string path, List<string> outPaths)
        {
            EnsureLoaded();
            outPaths.Clear();
            if (string.IsNullOrEmpty(path) || !hasCommittedSnapshot || isBuilding)
            {
                return false;
            }

            if (pathToReferencingPaths.TryGetValue(path, out List<string> paths))
            {
                for (int i = 0; i < paths.Count; i++)
                {
                    if (!string.Equals(paths[i], path, StringComparison.OrdinalIgnoreCase))
                    {
                        outPaths.Add(paths[i]);
                    }
                }
            }

            return true;
        }

        public static bool TryGetReferencedPaths(string path, List<string> outPaths)
        {
            EnsureLoaded();
            outPaths.Clear();
            if (string.IsNullOrEmpty(path) || !hasCommittedSnapshot || isBuilding)
            {
                return false;
            }

            if (pathToDependencyPaths.TryGetValue(path, out List<string> paths))
            {
                outPaths.AddRange(paths);
            }

            return true;
        }

        public static bool RequestIncrementalRebuildForTesting()
        {
            EnsureLoaded();
            if (isBuilding || !hasCommittedSnapshot || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isUpdating)
                return false;

            const string testPath = "Assets/ProjectAssetDataSyntheticTest/Prefabs/SyntheticPrefab_0000.prefab";
            if (!File.Exists(testPath) || !indexedPathOrder.ContainsKey(testPath))
                return false;

            pendingChanged.Clear();
            pendingDeleted.Clear();
            dependencyCache.Remove(testPath);
            NoteChangedPath(testPath);
            StartIncrementalRebuild();
            return isBuilding || !hasPendingChanges;
        }

        public static bool RequestFullRebuild()
        {
            EnsureLoaded();
            if (isBuilding || EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying || EditorApplication.isUpdating)
                return false;

            StartFullRebuild();
            return true;
        }

        public static void RequestRebuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying)
            {
                hasPendingChanges = true;
                return;
            }

            // While the AssetDatabase is refreshing/importing, GetDependencies() contends
            // with the refresh pipeline on internal locks and a single call can stall for
            // seconds (watchdog "Hold on"). Defer the rebuild until the refresh finishes.
            if (EditorApplication.isUpdating)
            {
                hasPendingChanges = true;
                ScheduleDeferredRebuild();
                return;
            }

            if (isBuilding)
            {
                rebuildRequestedWhileBuilding = true;
                hasPendingChanges = true;
                return;
            }

            // 有快照时只消化导入/删除/移动记下来的路径，不再扫描整个 Assets。
            if (hasCommittedSnapshot && cachedIndexedPaths != null && cachedIndexedPaths.Count > 0)
            {
                if (pendingChanged.Count == 0 && pendingDeleted.Count == 0)
                {
                    hasPendingChanges = false;
                    if (snapshotDirty)
                        SaveSnapshot();
                    return;
                }

                StartIncrementalRebuild();
            }
            else
            {
                StartFullRebuild();
            }
        }

        private static void ScheduleDeferredRebuild()
        {
            if (deferredRebuildHooked) return;
            deferredRebuildHooked = true;
            EditorApplication.update += ProcessDeferredRebuild;
        }

        private static void ProcessDeferredRebuild()
        {
            if (EditorApplication.isUpdating) return;
            EditorApplication.update -= ProcessDeferredRebuild;
            deferredRebuildHooked = false;
            RequestRebuild();
        }

        // ─── Full rebuild (original logic, unchanged) ───────────────────────

        private static void StartFullRebuild()
        {
            indexedPaths.Clear();
            buildingReferenceCountsByGuid.Clear();
            buildingPathToReferencingPaths.Clear();
            buildingPathToDependencyPaths.Clear();
            buildIndex = 0;
            prefabAssetCount = 0;
            sceneAssetCount = 0;
            otherAssetCount = 0;
            isIncrementalBuild = false;
            pendingChanged.Clear();
            pendingDeleted.Clear();
            journalChanged.Clear();
            journalDeleted.Clear();
            forceFullSnapshot = true;

            using (FindAssetsMarker.Auto())
                classifyGuids = AssetDatabase.FindAssets(string.Empty, ScanRoots);
            classifyIndex = 0;

            isBuilding = true;
            hasBuildStatistics = true;
            rebuildRequestedWhileBuilding = false;
            hasPendingChanges = false;
            buildStartTime = EditorApplication.timeSinceStartup;
            EditorApplication.update -= ProcessBuildStep;
            EditorApplication.update += ProcessBuildStep;
        }

        // ─── Incremental rebuild (Plans 1+2+4) ────────────────────────────

        private static void StartIncrementalRebuild()
        {
            var changed = new string[pendingChanged.Count];
            pendingChanged.CopyTo(changed);
            pendingChanged.Clear();
            var deleted = new string[pendingDeleted.Count];
            pendingDeleted.CopyTo(deleted);
            pendingDeleted.Clear();

            buildingReferenceCountsByGuid.Clear();
            buildingPathToReferencingPaths.Clear();
            buildingPathToDependencyPaths.Clear();
            buildIndex = 0;
            classifyGuids = null;
            isIncrementalBuild = true;
            hasBuildStatistics = true;

            IncrementalScanMarker.Begin();
            var pathsToProcess = new List<string>(changed.Length);
            bool mutated = false;

            for (int i = 0; i < deleted.Length; i++)
            {
                if (Unindex(deleted[i]))
                    mutated = true;
            }

            for (int i = 0; i < changed.Length; i++)
            {
                string path = changed[i];
                if (!File.Exists(path))
                {
                    if (Unindex(path))
                        mutated = true;
                    continue;
                }

                if (indexedPathOrder.ContainsKey(path))
                {
                    RemoveFromCommittedData(path, isDeleted: false);
                    dependencyCache.Remove(path);
                }
                else
                {
                    cachedIndexedPaths.Add(path);
                    indexedPathOrder[path] = cachedIndexedPaths.Count - 1;
                    AdjustIndexedCount(path, 1);
                }

                pathsToProcess.Add(path);
                RememberChanged(path);
                mutated = true;
            }

            indexedPaths.Clear();
            indexedPaths.AddRange(pathsToProcess);
            IncrementalScanMarker.End();

            if (mutated)
                snapshotDirty = true;

            if (pathsToProcess.Count == 0)
            {
                // 本轮没有要重算的文件。把就绪摘要用的列表放回全量，避免显示成 0。
                PublishFullIndexedPaths();
                hasPendingChanges = pendingChanged.Count > 0 || pendingDeleted.Count > 0;
                isIncrementalBuild = false;
                FinishSnapshotWrite();
                return;
            }

            isBuilding = true;
            rebuildRequestedWhileBuilding = false;
            hasPendingChanges = false;
            buildStartTime = EditorApplication.timeSinceStartup;
            Debug.Log($"[ProjectAssetData] Incremental rebuild: {pathsToProcess.Count} changed/new of {cachedIndexedPaths.Count} total assets");

            EditorApplication.update -= ProcessBuildStep;
            EditorApplication.update += ProcessBuildStep;
        }

        private static bool Unindex(string path)
        {
            if (cachedIndexedPaths == null || !indexedPathOrder.TryGetValue(path, out int index))
                return false;

            int last = cachedIndexedPaths.Count - 1;
            if (index < last)
            {
                string moved = cachedIndexedPaths[last];
                cachedIndexedPaths[index] = moved;
                indexedPathOrder[moved] = index;
            }

            cachedIndexedPaths.RemoveAt(last);
            indexedPathOrder.Remove(path);
            AdjustIndexedCount(path, -1);
            RemoveFromCommittedData(path, isDeleted: true);
            dependencyCache.Remove(path);
            RememberDeleted(path);
            return true;
        }

        private static void RebuildIndexedOrder()
        {
            indexedPathOrder.Clear();
            if (cachedIndexedPaths == null)
                return;

            for (int i = 0; i < cachedIndexedPaths.Count; i++)
                indexedPathOrder[cachedIndexedPaths[i]] = i;
        }

        private static void AdjustIndexedCount(string path, int delta)
        {
            if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) prefabAssetCount += delta;
            else if (path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)) sceneAssetCount += delta;
            else otherAssetCount += delta;
        }

        /// <summary>
        /// Remove a file's old dependency data from the committed dictionaries.
        /// Called during incremental rebuild for deleted and changed files.
        /// </summary>
        /// <param name="isDeleted">True if the file no longer exists (also clears incoming references).</param>
        private static void RemoveFromCommittedData(string path, bool isDeleted)
        {
            if (!pathToDependencyPaths.TryGetValue(path, out var oldDeps))
                return;

            // Remove reverse references: for each old dependency, remove 'path' from its referencing list
            foreach (string oldDep in oldDeps)
            {
                if (pathToReferencingPaths.TryGetValue(oldDep, out var refs))
                {
                    for (int i = refs.Count - 1; i >= 0; i--)
                    {
                        if (string.Equals(refs[i], path, StringComparison.OrdinalIgnoreCase))
                        {
                            refs.RemoveAt(i);
                            break;
                        }
                    }

                    if (refs.Count == 0)
                        pathToReferencingPaths.Remove(oldDep);
                }

                // 快照不存依赖路径到 guid。这里必须现查，否则旧次数减不掉，结束时的合并只会再加一次。
                string oldGuid = AssetDatabase.AssetPathToGUID(oldDep);
                if (!string.IsNullOrEmpty(oldGuid) && referenceCountsByGuid.TryGetValue(oldGuid, out int count))
                {
                    if (count <= 1)
                        referenceCountsByGuid.Remove(oldGuid);
                    else
                        referenceCountsByGuid[oldGuid] = count - 1;
                }
            }

            pathToDependencyPaths.Remove(path);

            // For deleted files only: remove the file from the "who references this" map
            if (isDeleted)
            {
                pathToReferencingPaths.Remove(path);
            }
        }

        // ─── Time-sliced processing (shared between full and incremental) ──

        private static bool AdvanceClassify()
        {
            double deadline = EditorApplication.timeSinceStartup + BuildTimeBudgetSeconds;
            int processed = 0;
            using (ClassifyPathsMarker.Auto())
            {
                while (classifyIndex < classifyGuids.Length && processed < BatchSize && (processed < 4 || EditorApplication.timeSinceStartup < deadline))
                {
                    string path = AssetDatabase.GUIDToAssetPath(classifyGuids[classifyIndex++]);
                    processed++;
                    if (!ShouldIndexPath(path))
                        continue;

                    indexedPaths.Add(path);
                    if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) prefabAssetCount++;
                    else if (path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)) sceneAssetCount++;
                    else otherAssetCount++;
                }
            }

            return classifyIndex >= classifyGuids.Length;
        }

        private static void FinishEmptyFullRebuild()
        {
            isBuilding = false;
            EditorApplication.update -= ProcessBuildStep;
            referenceCountsByGuid.Clear();
            pathToReferencingPaths.Clear();
            pathToDependencyPaths.Clear();
            hasCommittedSnapshot = false;
            revision++;
            cachedIndexedPaths = null;
            indexedPathOrder.Clear();
            ProjectAssetReferenceIndexStore.Delete();
        }

        private static void ProcessBuildStep()
        {
            // AssetDatabase refresh in flight: pause this tick so GetDependencies()
            // does not contend with the refresh pipeline. The build resumes next tick.
            if (EditorApplication.isUpdating)
            {
                return;
            }

            if (classifyGuids != null)
            {
                if (!AdvanceClassify())
                    return;

                classifyGuids = null;
                if (indexedPaths.Count == 0)
                {
                    FinishEmptyFullRebuild();
                    return;
                }

                cachedIndexedPaths = new List<string>(indexedPaths);
                RebuildIndexedOrder();
                return;
            }

            double deadline = EditorApplication.timeSinceStartup + BuildTimeBudgetSeconds;
            int processed = 0;
            using (AssetProcessingMarker.Auto())
            {
                while (buildIndex < indexedPaths.Count && processed < BatchSize && (processed < 4 || EditorApplication.timeSinceStartup < deadline))
                {
                    ProcessAsset(indexedPaths[buildIndex++]);
                    processed++;
                }
            }

            EditorApplication.RepaintProjectWindow();
            if (buildIndex < indexedPaths.Count)
            {
                return;
            }

            // Rebuild complete. 增量时 indexedPaths 只含本轮变更，进度用它做分母。
            // 结束后改回全量列表，就绪摘要里的「共 N 个资源」才和预制体/场景/其他对得上。
            int processedCount = indexedPaths.Count;
            if (isIncrementalBuild)
                PublishFullIndexedPaths();

            isBuilding = false;
            EditorApplication.update -= ProcessBuildStep;

            if (isIncrementalBuild)
            {
                // MERGE mode for incremental rebuild:
                //   - reference counts: add (building counts are contributions from processed files)
                //   - referencing paths: append (multiple files can reference the same dependency)
                //   - dependency paths: overwrite (each file has exactly one entry)
                foreach (var pair in buildingReferenceCountsByGuid)
                {
                    if (referenceCountsByGuid.TryGetValue(pair.Key, out int existing))
                        referenceCountsByGuid[pair.Key] = existing + pair.Value;
                    else
                        referenceCountsByGuid[pair.Key] = pair.Value;
                }

                foreach (var pair in buildingPathToReferencingPaths)
                {
                    if (pathToReferencingPaths.TryGetValue(pair.Key, out var existing))
                        existing.AddRange(pair.Value);
                    else
                        pathToReferencingPaths[pair.Key] = pair.Value;
                }

                foreach (var pair in buildingPathToDependencyPaths)
                    pathToDependencyPaths[pair.Key] = pair.Value;
            }
            else
            {
                // Full build: swap as before
                referenceCountsByGuid.Clear();
                foreach (var pair in buildingReferenceCountsByGuid) referenceCountsByGuid[pair.Key] = pair.Value;
                pathToReferencingPaths.Clear();
                foreach (var pair in buildingPathToReferencingPaths) pathToReferencingPaths[pair.Key] = pair.Value;
                pathToDependencyPaths.Clear();
                foreach (var pair in buildingPathToDependencyPaths) pathToDependencyPaths[pair.Key] = pair.Value;
            }

            buildingReferenceCountsByGuid.Clear();
            buildingPathToReferencingPaths.Clear();
            buildingPathToDependencyPaths.Clear();

            hasCommittedSnapshot = true;
            revision++;
            snapshotDirty = true;

            string label = isIncrementalBuild ? "incremental" : "full";
            Debug.Log($"[ProjectAssetData] Reference index complete ({label}): {processedCount} assets processed in {EditorApplication.timeSinceStartup - buildStartTime:F2}s");
            FinishSnapshotWrite();
        }

        private static void PublishFullIndexedPaths()
        {
            if (cachedIndexedPaths == null)
                return;

            indexedPaths.Clear();
            indexedPaths.AddRange(cachedIndexedPaths);
        }

        private static void FinishSnapshotWrite()
        {
            if (rebuildRequestedWhileBuilding)
            {
                snapshotDirty = true;
                rebuildRequestedWhileBuilding = false;
                RequestRebuild();
                return;
            }

            if (snapshotDirty)
                SaveSnapshot();
        }

        private static void ScheduleSnapshotCompact()
        {
            compactDueTime = EditorApplication.timeSinceStartup + CompactDelaySeconds;
            if (compactHooked)
                return;

            EditorApplication.update += CompactSnapshotWhenIdle;
            compactHooked = true;
        }

        private static void CompactSnapshotWhenIdle()
        {
            if (isBuilding || EditorApplication.isUpdating || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (EditorApplication.timeSinceStartup < compactDueTime)
                return;

            EditorApplication.update -= CompactSnapshotWhenIdle;
            compactHooked = false;
            compactDueTime = -1d;
            forceFullSnapshot = true;
            snapshotDirty = true;
            SaveSnapshot();
        }

        private static void RememberChanged(string path)
        {
            journalDeleted.Remove(path);
            if (!journalChanged.Contains(path))
                journalChanged.Add(path);
        }

        private static void RememberDeleted(string path)
        {
            journalChanged.Remove(path);
            if (!journalDeleted.Contains(path))
                journalDeleted.Add(path);
        }

        private static void SaveSnapshot()
        {
            using (SnapshotSaveMarker.Auto())
            {
                if (!forceFullSnapshot && (journalChanged.Count > 0 || journalDeleted.Count > 0))
                {
                    var journalTimes = new Dictionary<string, long>(journalChanged.Count, StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < journalChanged.Count; i++)
                    {
                        string path = journalChanged[i];
                        if (dependencyCache.TryGetValue(path, out CachedDependencies cached))
                            journalTimes[path] = cached.writeTimeTicks;
                    }

                    if (ProjectAssetReferenceIndexStore.TryAppend(journalChanged, journalDeleted, pathToDependencyPaths, journalTimes, out bool journalExceeded))
                    {
                        journalChanged.Clear();
                        journalDeleted.Clear();
                        snapshotDirty = false;
                        if (journalExceeded)
                            ScheduleSnapshotCompact();
                        return;
                    }
                }

                var writeTimeTicks = new Dictionary<string, long>(dependencyCache.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var pair in dependencyCache)
                    writeTimeTicks[pair.Key] = pair.Value.writeTimeTicks;

                if (ProjectAssetReferenceIndexStore.Save(
                    referenceCountsByGuid,
                    pathToDependencyPaths,
                    writeTimeTicks,
                    cachedIndexedPaths ?? new List<string>(pathToDependencyPaths.Keys)))
                {
                    journalChanged.Clear();
                    journalDeleted.Clear();
                    snapshotDirty = false;
                    forceFullSnapshot = false;
                }
            }
        }

        internal static void NoteAssetsChanged(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            NoteDeletedPaths(deletedAssets);
            NoteDeletedPaths(movedFromAssetPaths);
            NoteChangedPaths(importedAssets);
            NoteChangedPaths(movedAssets);
        }

        internal static void NoteChangedPaths(string[] paths)
        {
            if (paths == null)
                return;

            for (int i = 0; i < paths.Length; i++)
                NoteChangedPath(paths[i]);
        }

        private static void NoteDeletedPaths(string[] paths)
        {
            if (paths == null)
                return;

            for (int i = 0; i < paths.Length; i++)
                NoteDeletedPath(paths[i]);
        }

        private static void NoteChangedPath(string path)
        {
            if (!CanIndexPath(path))
                return;

            pendingDeleted.Remove(path);
            if (!pendingChanged.Add(path))
                return;

            MarkDirty();
        }

        private static void NoteDeletedPath(string path)
        {
            if (!CanIndexPath(path))
                return;

            pendingChanged.Remove(path);
            if (!pendingDeleted.Add(path))
                return;

            MarkDirty();
        }

        private static bool CanIndexPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            string extension = Path.GetExtension(path);
            return !string.IsNullOrEmpty(extension) && !ExcludedExtensions.Contains(extension) && !LeafAssetExtensions.Contains(extension);
        }

        private static void ProcessAsset(string assetPath)
        {
            try
            {
                string[] dependencies = GetDependencies(assetPath);
                var directDependencies = new List<string>(dependencies.Length);
                for (int i = 0; i < dependencies.Length; i++)
                {
                    string dependencyPath = dependencies[i];
                    if (string.Equals(dependencyPath, assetPath, StringComparison.OrdinalIgnoreCase) || !ShouldCountDependency(dependencyPath))
                    {
                        continue;
                    }

                    directDependencies.Add(dependencyPath);

                    if (!buildingPathToReferencingPaths.TryGetValue(dependencyPath, out List<string> references))
                    {
                        references = new List<string>(4);
                        buildingPathToReferencingPaths[dependencyPath] = references;
                    }
                    references.Add(assetPath);

                    if (!guidByPath.TryGetValue(dependencyPath, out string guid))
                    {
                        guid = AssetDatabase.AssetPathToGUID(dependencyPath);
                        guidByPath[dependencyPath] = guid;
                    }
                    if (!string.IsNullOrEmpty(guid))
                    {
                        buildingReferenceCountsByGuid[guid] = buildingReferenceCountsByGuid.TryGetValue(guid, out int count) ? count + 1 : 1;
                    }
                }

                buildingPathToDependencyPaths[assetPath] = directDependencies;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Project asset reference index skipped '{assetPath}': {exception.Message}");
            }
        }

        private static string[] GetDependencies(string path)
        {
            try
            {
                long writeTime = File.GetLastWriteTimeUtc(path).Ticks;
                if (dependencyCache.TryGetValue(path, out CachedDependencies cached) && cached.writeTimeTicks == writeTime)
                {
                    return cached.dependencies;
                }

                string[] dependencies = AssetDatabase.GetDependencies(path, false);
                dependencyCache[path] = new CachedDependencies { writeTimeTicks = writeTime, dependencies = dependencies };
                return dependencies;
            }
            catch
            {
                return AssetDatabase.GetDependencies(path, false);
            }
        }

        private static bool ShouldIndexPath(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path)) return false;
            string extension = Path.GetExtension(path);
            return !string.IsNullOrEmpty(extension) && !ExcludedExtensions.Contains(extension) && !LeafAssetExtensions.Contains(extension);
        }

        private static bool ShouldCountDependency(string path)
        {
            string extension = Path.GetExtension(path);
            return !string.IsNullOrEmpty(extension) && !ExcludedExtensions.Contains(extension);
        }

        private static void MarkDirty()
        {
            hasPendingChanges = true;
            if (isBuilding) rebuildRequestedWhileBuilding = true;
            EditorApplication.RepaintProjectWindow();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            hasPendingChanges = true;
        }
    }
}