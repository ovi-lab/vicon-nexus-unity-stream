using UnityEngine;

namespace ubco.ovilab.ViconUnityStream
{
    /// <summary>
    /// Simple sample settings showing how to create custom configuration data for your package.
    /// </summary>
    [System.Serializable]
    public class ViconXRSettings : ScriptableObject
    {
        /// <summary>Static instance that will hold the asset instance.</summary>
        /// <see cref="SampleBuildProcessor"/>
        public static ViconXRSettings runtimeInstance = null;
        
        [SerializeField, Tooltip("Enable XRHandSubsystem.")]
        private bool enableXRHandSubsystem;

        /// <summary>
        /// Enable <see cref="UnityEngine.XR.Hands.XRHandSubsystem">XRHandsSubsystem</see>.
        /// </summary>
        public bool EnableXRHandSubsystem { get => enableXRHandSubsystem; set => enableXRHandSubsystem = value; }

        [SerializeField, Tooltip("Enable Vicon XR Device which provides HMD positions through Input system.")]
        private bool enableViconXRDevice;

        /// <summary>
        /// Enable the Vicon XR Device (comment <see cref="ViconXRDevice"/>), which provides an <see cref="UnityEngine.InputSystem.XR.XRHMD">XRHMD</see>
        /// <see cref="UnityEngine.InputSystem.InputDevice">InputDevice</see>.
        /// </summary>
        public bool EnableViconXRDevice { get => enableViconXRDevice; set => enableViconXRDevice = value; }

        void Awake()
        {
            runtimeInstance = this;
        }
    }
}
