using System.Reflection;
using UnityEditor;

namespace TLNexus.ProjectAssetData
{
    internal sealed class ProjectAssetIndexPostprocessor : AssetPostprocessor
    {
        [Obfuscation(Exclude = true)]
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            ProjectAssetReferenceIndex.NoteAssetsChanged(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
        }
    }
}
