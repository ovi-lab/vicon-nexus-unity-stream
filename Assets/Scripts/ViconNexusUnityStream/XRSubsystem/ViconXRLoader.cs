using System.Collections.Generic;

using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine;
using System;

namespace ubco.ovilab.ViconUnityStream
{
    public class ViconXRLoader: ScriptableObject
    {
        static List<XRInputSubsystemDescriptor> inputSubsystemDescriptors = new();
        static List<XRHandSubsystemDescriptor> xrHandsSubsystemDescriptors = new();

        private ViconXRSettings settings;
        private static ViconXRLoader loader;

        /// <summary>
        /// Return the currently active Input Subsystem intance, if any.
        /// </summary>
        public XRInputSubsystem inputSubsystem { get; private set; }

        /// <summary>
        /// Return the currently active XR Hand Subsystem intance, if any.
        /// </summary>
        public ViconHandSubsystem HandSubsystem { get; private set; }

        /// <summary>
        /// The associated vicon device the loader is managing.
        /// </summary>
        public ViconXRDevice XRDevice { get; private set; }

        /// <inheritdoc />
        private void Awake()
        {
            loader = this;
        }

        /// <inheritdoc />
        private void OnEnable()
        {
            // Duplicate because of how Unity handles these calls!
            loader = this;
        }

        /// <inheritdoc />
        public void Start()
        {
            // TODO: Handle the XRDevice
            HandSubsystem?.Start();
        }

        /// <inheritdoc />
        public void Stop()
        {
            // TODO: Handle the XRDevice
            HandSubsystem?.Stop();
        }

        /// <inheritdoc />
        public void OnDestroy()
        {
            HandSubsystem?.Destroy();
            XRDevice?.Destroy();
            loader = null;
        }

        internal static ViconXRSettings GetSettings()
        {
            ViconXRSettings settings = null;
            // When running in the Unity Editor, we have to load user's customization of configuration data directly from
            // EditorBuildSettings. At runtime, we need to grab it from the static instance field instead.
#if UNITY_EDITOR
            UnityEditor.EditorBuildSettings.TryGetConfigObject(ViconXRConstants.settingsKey, out settings);
#else
            settings = ViconXRSettings.runtimeInstance;
#endif
            return settings;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Initialize()
        {
            if (loader == null)
            {
                Debug.LogError($"Loader is not set");
                return;
            }

            loader.settings = GetSettings();

            if (loader.settings.EnableXRHandSubsystem)
            {
                SubsystemManager.GetSubsystemDescriptors<XRHandSubsystemDescriptor>(xrHandsSubsystemDescriptors);

                if (xrHandsSubsystemDescriptors.Count > 0)
                {
                    foreach (var descriptor in xrHandsSubsystemDescriptors)
                    {
                        if (String.Compare(descriptor.id, ViconXRConstants.handSubsystemId, true) == 0)
                        {
                            loader.HandSubsystem = descriptor.Create() as ViconHandSubsystem;
                            break;
                        }
                    }
                }
                if (loader.HandSubsystem == null)
                {
                    Debug.LogError($"{typeof(ViconHandSubsystem).Name} failed to configure!");
                }
                else
                {
                    loader.HandSubsystem?.Start();
                    Debug.Log($"{typeof(ViconHandSubsystem).Name} configured!");
                }
            }

            if (loader.settings.EnableViconXRDevice)
            {
                loader.XRDevice = ViconXRDevice.SetupDevice();
            }
        }

        #region Passing data to subsystems
        /// <summary>
        /// Set hand subsystem data.
        /// </summary>
        public static void TrySetHandSbsystemData(Handedness handedness, Dictionary<XRHandJointID, Pose> poses)
        {
            if (loader != null)
            {
                loader.HandSubsystem?.SetHandPoses(handedness, poses);
            }
        }

        /// <summary>
        /// If the loader is setup and configured, set the hwd data in the HMD device.
        /// </summary>
        public static void TrySetXRDeviceData(Vector3 pos, Quaternion rot)
        {
            if (loader != null && loader.settings != null)
            {
                pos += loader.settings.HMDPositionOffset;
                loader.XRDevice?.SetDeviceData(pos, rot);
            }
        }
        #endregion
    }
}
