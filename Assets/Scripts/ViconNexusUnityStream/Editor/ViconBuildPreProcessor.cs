using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ubco.ovilab.ViconUnityStream.Editor
{
    class ViconBuildPreProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("Running Vicon build pre-process");
            ViconXRSettingsProvider.EnsureViconXRSettingsAndLoaderAreLoaded();
        }
    }
}
