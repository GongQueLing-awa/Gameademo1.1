using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TLNexus.ProjectAssetData
{
    public enum ProjectAssetAutomaticRebuildOwner
    {
        None = 0,
        AssetClip = 1,
        TraceWeave = 2
    }

    public enum ProjectAssetAutomaticRebuildMode
    {
        OnSave = 0,
        Interval = 1,
        Idle = 2
    }

    /// <summary>Project-wide automatic rebuild settings. Exactly one product may control the scheduler.</summary>
    public static class ProjectAssetAutomaticRebuildSettings
    {        private const int DefaultIntervalMinutes = 30;
        private const int DefaultIdleMinutes = 15;
        private const int MinimumMinutes = 1;

        [Serializable]
        private sealed class StateData
        {
            public bool enabled;
            public ProjectAssetAutomaticRebuildOwner owner;
            public ProjectAssetAutomaticRebuildMode mode = ProjectAssetAutomaticRebuildMode.OnSave;
            public int intervalMinutes = DefaultIntervalMinutes;
            public int idleMinutes = DefaultIdleMinutes;
        }

        private static readonly string StateFilePath = GetStateFilePath();
        private static StateData state;
        private static bool loaded;

        public static bool Enabled { get { EnsureLoaded(); return state.enabled; } }
        public static ProjectAssetAutomaticRebuildOwner Owner { get { EnsureLoaded(); return state.owner; } }
        public static ProjectAssetAutomaticRebuildMode Mode { get { EnsureLoaded(); return state.mode; } }
        public static int IntervalMinutes { get { EnsureLoaded(); return state.intervalMinutes; } }
        public static int IdleMinutes { get { EnsureLoaded(); return state.idleMinutes; } }

        public static bool CanControl(ProjectAssetAutomaticRebuildOwner product)
        {
            EnsureLoaded();
            return !state.enabled || state.owner == product;
        }

        public static bool TrySetEnabled(ProjectAssetAutomaticRebuildOwner product, bool value)
        {
            EnsureLoaded();
            if (value)
            {
                if (state.enabled && state.owner != product) return false;
                state.enabled = true;
                state.owner = product;
            }
            else
            {
                if (state.enabled && state.owner != product) return false;
                state.enabled = false;
                state.owner = ProjectAssetAutomaticRebuildOwner.None;
            }

            Save();
            return true;
        }

        public static bool TrySetMode(ProjectAssetAutomaticRebuildOwner product, ProjectAssetAutomaticRebuildMode value)
        {
            EnsureLoaded();
            if (!CanControl(product)) return false;
            state.mode = value == ProjectAssetAutomaticRebuildMode.Interval || value == ProjectAssetAutomaticRebuildMode.Idle
                ? value : ProjectAssetAutomaticRebuildMode.OnSave;
            Save();
            return true;
        }

        public static bool TrySetIntervalMinutes(ProjectAssetAutomaticRebuildOwner product, int value)
        {
            EnsureLoaded();
            if (!CanControl(product)) return false;
            state.intervalMinutes = value;
            Save();
            return true;
        }

        public static bool TrySetIdleMinutes(ProjectAssetAutomaticRebuildOwner product, int value)
        {
            EnsureLoaded();
            if (!CanControl(product)) return false;
            state.idleMinutes = value;
            Save();
            return true;
        }

        private static void EnsureLoaded()
        {
            if (loaded) return;
            loaded = true;
            state = new StateData();
            try
            {
                if (File.Exists(StateFilePath))
                {
                    state = JsonUtility.FromJson<StateData>(File.ReadAllText(StateFilePath)) ?? new StateData();
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Project automatic rebuild settings load failed: {exception.Message}");
                state = new StateData();
            }
            Normalize();
        }

        private static void Normalize()
        {
            state ??= new StateData();
            if (!state.enabled) state.owner = ProjectAssetAutomaticRebuildOwner.None;
            if (state.owner != ProjectAssetAutomaticRebuildOwner.AssetClip && state.owner != ProjectAssetAutomaticRebuildOwner.TraceWeave)
            {
                state.enabled = false;
                state.owner = ProjectAssetAutomaticRebuildOwner.None;
            }
            if (state.mode != ProjectAssetAutomaticRebuildMode.Interval && state.mode != ProjectAssetAutomaticRebuildMode.Idle)
            {
                state.mode = ProjectAssetAutomaticRebuildMode.OnSave;
            }

            if (state.intervalMinutes < MinimumMinutes) state.intervalMinutes = MinimumMinutes;
            if (state.idleMinutes < MinimumMinutes) state.idleMinutes = MinimumMinutes;
        }

        private static void Save()
        {
            Normalize();
            try
            {
                string directory = Path.GetDirectoryName(StateFilePath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                File.WriteAllText(StateFilePath, JsonUtility.ToJson(state, true));
                ProjectAssetAutomaticRebuild.RefreshSubscriptions();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Project automatic rebuild settings save failed: {exception.Message}");
            }
        }

        private static string GetStateFilePath()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            return string.IsNullOrEmpty(projectRoot)
                ? Path.Combine("Library", "TLNEXUS", "ProjectAssetData", "automatic-rebuild.json")
                : Path.Combine(projectRoot, "Library", "TLNEXUS", "ProjectAssetData", "automatic-rebuild.json");
        }
    }

    [InitializeOnLoad]
    internal sealed class ProjectAssetAutomaticRebuild : AssetModificationProcessor
    {
        private const double CheckIntervalSeconds = 60d;
        private const double SaveDelaySeconds = 0.5d;
        private const double StartupCooldownSeconds = 30d;
        private static double lastScheduleTime;
        private static double lastUserActivityTime;
        private static double nextCheckTime;
        private static double saveDueTime = -1d;
        private static double startupTime;
        private static bool rebuildQueued;
        private static bool updateHooked;
        private static bool idleActivityHooked;

        static ProjectAssetAutomaticRebuild()
        {
            double now = EditorApplication.timeSinceStartup;
            startupTime = now;
            lastScheduleTime = lastUserActivityTime = nextCheckTime = now;
            EditorSceneManager.sceneSaved += OnSceneSaved;
            RefreshSubscriptions();
        }

        /// <summary>
        /// 按当前设置增删常驻回调：未启用自动重建时不保留任何每帧/UI 回调，
        /// 闲时模式才需要 GUI 活动探测。设置变化后由 Settings.Save() 调用。
        /// </summary>
        internal static void RefreshSubscriptions()
        {
            bool enabled = ProjectAssetAutomaticRebuildSettings.Enabled;
            bool needsIdleActivity = enabled && ProjectAssetAutomaticRebuildSettings.Mode == ProjectAssetAutomaticRebuildMode.Idle;

            if (enabled && !updateHooked)
            {
                EditorApplication.update += Update;
                updateHooked = true;
            }
            else if (!enabled && updateHooked)
            {
                EditorApplication.update -= Update;
                updateHooked = false;
            }

            if (needsIdleActivity && !idleActivityHooked)
            {
                EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGui;
                EditorApplication.projectWindowItemOnGUI += OnProjectItemGui;
                SceneView.duringSceneGui += OnSceneGui;
                Selection.selectionChanged += MarkUserActivity;
                idleActivityHooked = true;
            }
            else if (!needsIdleActivity && idleActivityHooked)
            {
                EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyItemGui;
                EditorApplication.projectWindowItemOnGUI -= OnProjectItemGui;
                SceneView.duringSceneGui -= OnSceneGui;
                Selection.selectionChanged -= MarkUserActivity;
                idleActivityHooked = false;
            }
        }

        private static void OnSceneSaved(Scene scene) => ScheduleSaveRebuild();
        private static void OnHierarchyItemGui(int instanceId, Rect selectionRect) => MarkGuiActivity();
        private static void OnProjectItemGui(string guid, Rect selectionRect) => MarkGuiActivity();
        private static void OnSceneGui(SceneView sceneView) => MarkGuiActivity();

        private static string[] OnWillSaveAssets(string[] paths)
        {
            MarkUserActivity();
            ProjectAssetReferenceIndex.NoteChangedPaths(paths);
            ScheduleSaveRebuild();
            return paths;
        }

        private static void MarkGuiActivity()
        {
            Event evt = Event.current;
            if (evt != null && evt.type != EventType.Repaint && evt.type != EventType.Layout) MarkUserActivity();
        }

        private static void MarkUserActivity() => lastUserActivityTime = EditorApplication.timeSinceStartup;

        private static void ScheduleSaveRebuild()
        {
            if (ProjectAssetAutomaticRebuildSettings.Enabled && ProjectAssetAutomaticRebuildSettings.Mode == ProjectAssetAutomaticRebuildMode.OnSave)
            {
                saveDueTime = EditorApplication.timeSinceStartup + SaveDelaySeconds;
            }
        }

        private static void Update()
        {
            if (!ProjectAssetAutomaticRebuildSettings.Enabled || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            double now = EditorApplication.timeSinceStartup;

            // 启动冷却期：域重载后 Unity 还在导入资产，此时既不该扫描（会与导入管线争锁），
            // 也不该把导入产生的 projectChanged/save 事件当成用户改动来排重建。
            // 冷却期内只丢弃待办，不产生重建；冷却结束后的触发条件与平时一致。
            if (now - startupTime < StartupCooldownSeconds)
            {
                saveDueTime = -1d;
                rebuildQueued = false;
                nextCheckTime = Math.Max(nextCheckTime, startupTime + StartupCooldownSeconds);
                return;
            }

            if (saveDueTime > 0d && now >= saveDueTime) { saveDueTime = -1d; rebuildQueued = true; }
            if (rebuildQueued && !ProjectAssetReferenceIndex.IsBuilding)
            {
                rebuildQueued = false;
                if (ProjectAssetReferenceIndex.HasPendingChanges) ProjectAssetReferenceIndex.RequestRebuild();
                lastScheduleTime = now;
            }
            if (now < nextCheckTime || !ProjectAssetReferenceIndex.HasPendingChanges || ProjectAssetReferenceIndex.IsBuilding) return;
            nextCheckTime = now + CheckIntervalSeconds;
            if (ProjectAssetAutomaticRebuildSettings.Mode == ProjectAssetAutomaticRebuildMode.Interval && now - lastScheduleTime >= ProjectAssetAutomaticRebuildSettings.IntervalMinutes * 60d)
            {
                rebuildQueued = true;
            }
            else if (ProjectAssetAutomaticRebuildSettings.Mode == ProjectAssetAutomaticRebuildMode.Idle && now - lastUserActivityTime >= ProjectAssetAutomaticRebuildSettings.IdleMinutes * 60d && now - lastScheduleTime >= ProjectAssetAutomaticRebuildSettings.IdleMinutes * 60d)
            {
                rebuildQueued = true;
            }
        }
    }
}
