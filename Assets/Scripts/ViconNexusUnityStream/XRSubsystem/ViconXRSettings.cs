using UnityEngine;

namespace ubco.ovilab.ViconUnityStream
{
    /// <summary>
    /// Simple sample settings showing how to create custom configuration data for your package.
    /// </summary>
    // Uncomment below line to have the settings appear in unified settings.
    //[XRConfigurationData("Sample Settings", SampleConstants.k_SettingsKey)]
    [System.Serializable]
    public class ViconXRSettings : ScriptableObject
    {
        #if !UNITY_EDITOR
        /// <summary>Static instance that will hold the runtime asset instance we created in our build process.</summary>
        /// <see cref="SampleBuildProcessor"/>
        public static ViconXRSettings s_RuntimeInstance = null;
        #endif

        void Awake()
        {
            #if !UNITY_EDITOR
            s_RuntimeInstance = this;
            #endif
        }
    }
}
