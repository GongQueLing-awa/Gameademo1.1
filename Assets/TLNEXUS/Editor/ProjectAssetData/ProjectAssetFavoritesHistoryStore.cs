using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TLNexus.ProjectAssetData
{
    [Serializable]
    public sealed class ProjectAssetFavoriteEntry
    {
        public string assetPath;
        public string remark;
    }

    [Serializable]
    public sealed class ProjectAssetFavoriteScheme
    {
        public string id;
        public string name;
        public List<ProjectAssetFavoriteEntry> assets = new List<ProjectAssetFavoriteEntry>();
    }

    [Serializable]
    internal sealed class ProjectAssetFavoritesHistoryState
    {
        public int version = 2;
        public List<ProjectAssetFavoriteScheme> favoriteSchemes = new List<ProjectAssetFavoriteScheme>();
        public List<string> recentAssetPaths = new List<string>();
    }

    /// <summary>
    /// Project-wide source of truth for manual asset favorites and recent assets.
    /// Editor products consume this store; they never mirror its state into their own files.
    /// </summary>
    public static class ProjectAssetFavoritesHistoryStore
    {
        private const string DefaultSchemeId = "default";
        private const string SchemeIdPrefix = "scheme-";
        private static readonly string StateFilePath = GetStateFilePath();
        private static ProjectAssetFavoritesHistoryState state;
        private static bool loaded;

        public static event Action Changed;

        public static IReadOnlyList<ProjectAssetFavoriteScheme> FavoriteSchemes
        {
            get
            {
                EnsureLoaded();
                return CloneSchemes(state.favoriteSchemes);
            }
        }

        public static IReadOnlyList<string> RecentAssetPaths
        {
            get
            {
                EnsureLoaded();
                return new List<string>(state.recentAssetPaths);
            }
        }

        public static bool IsFavorite(string schemeId, string assetPath)
        {
            ProjectAssetFavoriteScheme scheme = GetScheme(schemeId);
            string normalizedPath = NormalizeAssetPath(assetPath);
            return scheme != null && !string.IsNullOrEmpty(normalizedPath) &&
                   FindFavoriteEntry(scheme, normalizedPath) != null;
        }

        public static IReadOnlyList<string> GetFavoriteAssetPaths(string schemeId)
        {
            ProjectAssetFavoriteScheme scheme = GetScheme(schemeId);
            return scheme == null ? Array.Empty<string>() : GetAssetPaths(scheme.assets);
        }

        public static IReadOnlyList<ProjectAssetFavoriteEntry> GetFavoriteEntries(string schemeId)
        {
            ProjectAssetFavoriteScheme scheme = GetScheme(schemeId);
            return scheme == null ? Array.Empty<ProjectAssetFavoriteEntry>() : CloneEntries(scheme.assets);
        }

        public static string GetFavoriteRemark(string schemeId, string assetPath)
        {
            ProjectAssetFavoriteScheme scheme = GetScheme(schemeId);
            ProjectAssetFavoriteEntry entry = FindFavoriteEntry(scheme, NormalizeAssetPath(assetPath));
            return entry?.remark ?? string.Empty;
        }

        public static bool SetFavoriteRemark(string schemeId, string assetPath, string remark)
        {
            ProjectAssetFavoriteScheme scheme = GetScheme(schemeId);
            ProjectAssetFavoriteEntry entry = FindFavoriteEntry(scheme, NormalizeAssetPath(assetPath));
            if (entry == null)
            {
                return false;
            }

            string normalizedRemark = (remark ?? string.Empty).Trim();
            if (string.Equals(entry.remark, normalizedRemark, StringComparison.Ordinal))
            {
                return true;
            }

            entry.remark = normalizedRemark;
            SaveAndNotify();
            return true;
        }

        public static void AddFavorite(string schemeId, string assetPath, int maxItems)
        {
            ProjectAssetFavoriteScheme scheme = GetScheme(schemeId);
            string normalizedPath = NormalizeAssetPath(assetPath);
            if (scheme == null || string.IsNullOrEmpty(normalizedPath))
            {
                return;
            }

            ProjectAssetFavoriteEntry entry = FindFavoriteEntry(scheme, normalizedPath);
            if (entry != null)
            {
                scheme.assets.Remove(entry);
            }
            else
            {
                entry = new ProjectAssetFavoriteEntry { assetPath = normalizedPath };
            }

            scheme.assets.Insert(0, entry);
            if (maxItems > 0 && scheme.assets.Count > maxItems)
            {
                scheme.assets.RemoveRange(maxItems, scheme.assets.Count - maxItems);
            }

            SaveAndNotify();
        }

        public static void RemoveFavorite(string schemeId, string assetPath)
        {
            ProjectAssetFavoriteScheme scheme = GetScheme(schemeId);
            string normalizedPath = NormalizeAssetPath(assetPath);
            if (scheme == null || string.IsNullOrEmpty(normalizedPath))
            {
                return;
            }

            int removed = scheme.assets.RemoveAll(entry => entry != null && string.Equals(entry.assetPath, normalizedPath, StringComparison.OrdinalIgnoreCase));
            if (removed > 0)
            {
                SaveAndNotify();
            }
        }

        public static string CreateFavoriteScheme(string schemeName)
        {
            EnsureLoaded();
            string normalizedName = (schemeName ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(normalizedName) || FindSchemeByName(normalizedName) != null)
            {
                return string.Empty;
            }

            string id = GenerateSchemeId();
            state.favoriteSchemes.Add(new ProjectAssetFavoriteScheme { id = id, name = normalizedName });
            SaveAndNotify();
            return id;
        }

        public static bool RemoveFavoriteScheme(string schemeId)
        {
            EnsureLoaded();
            string normalizedId = NormalizeSchemeId(schemeId);
            if (string.Equals(normalizedId, DefaultSchemeId, StringComparison.OrdinalIgnoreCase) || state.favoriteSchemes.Count <= 1)
            {
                return false;
            }

            int index = state.favoriteSchemes.FindIndex(scheme => scheme != null && string.Equals(scheme.id, normalizedId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return false;
            }

            state.favoriteSchemes.RemoveAt(index);
            SaveAndNotify();
            return true;
        }

        public static string GetFavoriteSchemeIdByName(string schemeName)
        {
            ProjectAssetFavoriteScheme scheme = FindSchemeByName(schemeName);
            return scheme?.id ?? string.Empty;
        }

        public static void ReplaceFavoriteSchemeAssets(string schemeId, IReadOnlyList<string> assetPaths)
        {
            ProjectAssetFavoriteScheme scheme = GetScheme(schemeId);
            if (scheme == null)
            {
                return;
            }

            var existingRemarks = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < scheme.assets.Count; i++)
            {
                ProjectAssetFavoriteEntry entry = scheme.assets[i];
                if (entry != null && !string.IsNullOrEmpty(entry.assetPath))
                {
                    existingRemarks[entry.assetPath] = entry.remark ?? string.Empty;
                }
            }

            scheme.assets = CreateEntries(assetPaths, existingRemarks);
            SaveAndNotify();
        }

        public static void AddRecentAsset(string assetPath, int maxItems)
        {
            EnsureLoaded();
            string normalizedPath = NormalizeAssetPath(assetPath);
            if (string.IsNullOrEmpty(normalizedPath))
            {
                return;
            }

            state.recentAssetPaths.RemoveAll(path => string.Equals(path, normalizedPath, StringComparison.OrdinalIgnoreCase));
            state.recentAssetPaths.Insert(0, normalizedPath);
            if (maxItems > 0 && state.recentAssetPaths.Count > maxItems)
            {
                state.recentAssetPaths.RemoveRange(maxItems, state.recentAssetPaths.Count - maxItems);
            }

            SaveAndNotify();
        }

        private static ProjectAssetFavoriteScheme GetScheme(string schemeId)
        {
            EnsureLoaded();
            string normalizedId = NormalizeSchemeId(schemeId);
            if (string.IsNullOrEmpty(normalizedId))
            {
                normalizedId = DefaultSchemeId;
            }

            return state.favoriteSchemes.Find(scheme => scheme != null && string.Equals(scheme.id, normalizedId, StringComparison.OrdinalIgnoreCase));
        }

        private static ProjectAssetFavoriteScheme FindSchemeByName(string schemeName)
        {
            EnsureLoaded();
            string normalizedName = (schemeName ?? string.Empty).Trim();
            return state.favoriteSchemes.Find(scheme => scheme != null && string.Equals(scheme.name, normalizedName, StringComparison.OrdinalIgnoreCase));
        }

        private static void EnsureLoaded()
        {
            if (loaded)
            {
                return;
            }

            loaded = true;
            state = new ProjectAssetFavoritesHistoryState();
            try
            {
                if (File.Exists(StateFilePath))
                {
                    state = JsonUtility.FromJson<ProjectAssetFavoritesHistoryState>(File.ReadAllText(StateFilePath)) ?? state;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Project asset favorites/history load failed: {exception.Message}");
                state = new ProjectAssetFavoritesHistoryState();
            }

            NormalizeState();
        }

        private static void NormalizeState()
        {
            state ??= new ProjectAssetFavoritesHistoryState();
            state.favoriteSchemes ??= new List<ProjectAssetFavoriteScheme>();
            state.recentAssetPaths = NormalizePaths(state.recentAssetPaths);

            var normalizedSchemes = new List<ProjectAssetFavoriteScheme>();
            var usedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < state.favoriteSchemes.Count; i++)
            {
                ProjectAssetFavoriteScheme scheme = state.favoriteSchemes[i];
                if (scheme == null)
                {
                    continue;
                }

                string id = NormalizeSchemeId(scheme.id);
                if (string.IsNullOrEmpty(id) || usedIds.Contains(id))
                {
                    id = GenerateSchemeId(usedIds);
                }

                usedIds.Add(id);
                List<ProjectAssetFavoriteEntry> normalizedEntries = NormalizeEntries(scheme.assets);

                normalizedSchemes.Add(new ProjectAssetFavoriteScheme
                {
                    id = id,
                    name = (scheme.name ?? string.Empty).Trim(),
                    assets = normalizedEntries
                });
            }

            if (!usedIds.Contains(DefaultSchemeId))
            {
                normalizedSchemes.Insert(0, new ProjectAssetFavoriteScheme { id = DefaultSchemeId, name = string.Empty });
            }

            state.favoriteSchemes = normalizedSchemes;
            state.version = 2;
        }

        private static void SaveAndNotify()
        {
            try
            {
                NormalizeState();
                string directory = Path.GetDirectoryName(StateFilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(StateFilePath, JsonUtility.ToJson(state, true));
                Changed?.Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Project asset favorites/history save failed: {exception.Message}");
            }
        }

        private static string GetStateFilePath()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            return string.IsNullOrEmpty(projectRoot)
                ? Path.Combine("Library", "TLNEXUS", "ProjectAssetData", "favorites-history.json")
                : Path.Combine(projectRoot, "Library", "TLNEXUS", "ProjectAssetData", "favorites-history.json");
        }

        private static string GenerateSchemeId(HashSet<string> existingIds = null)
        {
            existingIds ??= new HashSet<string>(state.favoriteSchemes.ConvertAll(scheme => scheme?.id ?? string.Empty), StringComparer.OrdinalIgnoreCase);
            for (int index = 1; ; index++)
            {
                string id = $"{SchemeIdPrefix}{index}";
                if (existingIds.Add(id))
                {
                    return id;
                }
            }
        }

        private static string NormalizeSchemeId(string schemeId) => (schemeId ?? string.Empty).Trim();

        private static string NormalizeAssetPath(string assetPath)
        {
            string normalized = (assetPath ?? string.Empty).Trim().Replace('\\', '/');
            return normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) ||
                   normalized.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase)
                ? normalized
                : string.Empty;
        }

        private static List<string> NormalizePaths(IReadOnlyList<string> source)
        {
            var normalized = new List<string>();
            if (source == null)
            {
                return normalized;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < source.Count; i++)
            {
                string path = NormalizeAssetPath(source[i]);
                if (!string.IsNullOrEmpty(path) && seen.Add(path))
                {
                    normalized.Add(path);
                }
            }

            return normalized;
        }

        private static ProjectAssetFavoriteEntry FindFavoriteEntry(ProjectAssetFavoriteScheme scheme, string assetPath)
        {
            if (scheme?.assets == null || string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            return scheme.assets.Find(entry => entry != null && string.Equals(entry.assetPath, assetPath, StringComparison.OrdinalIgnoreCase));
        }

        private static List<ProjectAssetFavoriteEntry> NormalizeEntries(IReadOnlyList<ProjectAssetFavoriteEntry> source)
        {
            var normalized = new List<ProjectAssetFavoriteEntry>();
            if (source == null)
            {
                return normalized;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < source.Count; i++)
            {
                ProjectAssetFavoriteEntry entry = source[i];
                string assetPath = NormalizeAssetPath(entry?.assetPath);
                if (!string.IsNullOrEmpty(assetPath) && seen.Add(assetPath))
                {
                    normalized.Add(new ProjectAssetFavoriteEntry
                    {
                        assetPath = assetPath,
                        remark = (entry.remark ?? string.Empty).Trim()
                    });
                }
            }

            return normalized;
        }

        private static List<ProjectAssetFavoriteEntry> CreateEntries(IReadOnlyList<string> assetPaths, IReadOnlyDictionary<string, string> remarks)
        {
            List<string> normalizedPaths = NormalizePaths(assetPaths);
            var entries = new List<ProjectAssetFavoriteEntry>(normalizedPaths.Count);
            for (int i = 0; i < normalizedPaths.Count; i++)
            {
                string assetPath = normalizedPaths[i];
                entries.Add(new ProjectAssetFavoriteEntry
                {
                    assetPath = assetPath,
                    remark = remarks != null && remarks.TryGetValue(assetPath, out string remark) ? remark : string.Empty
                });
            }

            return entries;
        }

        private static List<string> GetAssetPaths(IReadOnlyList<ProjectAssetFavoriteEntry> entries)
        {
            var paths = new List<string>(entries?.Count ?? 0);
            if (entries == null)
            {
                return paths;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && !string.IsNullOrEmpty(entries[i].assetPath))
                {
                    paths.Add(entries[i].assetPath);
                }
            }

            return paths;
        }

        private static List<ProjectAssetFavoriteScheme> CloneSchemes(IReadOnlyList<ProjectAssetFavoriteScheme> source)
        {
            var clone = new List<ProjectAssetFavoriteScheme>(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                ProjectAssetFavoriteScheme scheme = source[i];
                clone.Add(new ProjectAssetFavoriteScheme
                {
                    id = scheme.id,
                    name = scheme.name,
                    assets = CloneEntries(scheme.assets)
                });
            }

            return clone;
        }

        private static List<ProjectAssetFavoriteEntry> CloneEntries(IReadOnlyList<ProjectAssetFavoriteEntry> source)
        {
            var clone = new List<ProjectAssetFavoriteEntry>(source?.Count ?? 0);
            if (source == null)
            {
                return clone;
            }

            for (int i = 0; i < source.Count; i++)
            {
                ProjectAssetFavoriteEntry entry = source[i];
                if (entry != null)
                {
                    clone.Add(new ProjectAssetFavoriteEntry { assetPath = entry.assetPath, remark = entry.remark });
                }
            }

            return clone;
        }
    }
}
