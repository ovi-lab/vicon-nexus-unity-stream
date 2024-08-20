using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ubco.ovilab.ViconUnityStream.Editor
{
    class ViconAssetPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (didDomainReload)
            {
                if (!ViconXRSettingsProvider.IsSettingsAvailable() || !ViconXRSettingsProvider.IsLoaderAvailable())
                {
                    ViconXRSettingsProvider.EnsureViconXRSettingsAndLoaderAreLoaded();
                }
            }
        }
    }
}
