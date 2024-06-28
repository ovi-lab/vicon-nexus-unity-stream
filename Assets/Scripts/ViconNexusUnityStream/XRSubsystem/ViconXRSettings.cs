using UnityEngine;
using UnityEngine.XR.Management;

namespace ubco.ovilab.ViconUnityStream
{
    /// <summary>
    /// Simple sample settings showing how to create custom configuration data for your package.
    /// </summary>
    [XRConfigurationData("Vicon Settings", ViconXRConstants.settingsKey)]
    [System.Serializable]
    public class ViconXRSettings : ScriptableObject
    {
#if !UNITY_EDITOR
        /// <summary>Static instance that will hold the runtime asset instance we created in our build process.</summary>
        /// <see cref="SampleBuildProcessor"/>
        public static ViconXRSettings s_RuntimeInstance = null;
#endif

        [SerializeField, Tooltip("Offset to be applied to the XRDevice (HMD) data before setting the centerEyePosition.")]
        private Vector3 _HMDPositionOffset;

        /// <summary>
        /// Offset to be applied to the XRDevice (HMD) data before setting the centerEyePosition.
        /// </summary>
        public Vector3 HMDPositionOffset { get => _HMDPositionOffset; set => _HMDPositionOffset = value; }

        void Awake()
        {
            #if !UNITY_EDITOR
            s_RuntimeInstance = this;
            #endif
        }
    }
}
