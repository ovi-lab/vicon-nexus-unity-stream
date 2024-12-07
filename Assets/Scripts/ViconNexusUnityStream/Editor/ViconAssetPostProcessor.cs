using UnityEditor;

namespace ubco.ovilab.ViconUnityStream.Editor
{
    class ViconAssetPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (didDomainReload)
            {
                ViconXRSettingsProvider.EnsureViconXRSettingsAndLoaderAreLoaded();
            }
        }
    }
}
