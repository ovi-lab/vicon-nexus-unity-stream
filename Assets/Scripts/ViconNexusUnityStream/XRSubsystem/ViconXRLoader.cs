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
        public static ViconXRLoader Instance => instance;

        private static ViconXRLoader instance;

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

        // FIXME: Is the awake needed?
        /// <inheritdoc />
        private void Awake()
        {
            instance = this;
        }

        /// <inheritdoc />
        private void OnEnable()
        {
            // Duplicate because of how Unity handles these calls!
            instance = this;
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
            instance = null;
        }

        internal static ViconXRSettings GetSettings()
        {
            ViconXRSettings settings = null;
            settings = ViconXRSettings.runtimeInstance;
            return settings;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Initialize()
        {
            if (instance == null)
            {
                Debug.LogError($"Loader is not set");
                return;
            }

            instance.settings = GetSettings();
            if (instance.settings == null)
            {
                Debug.LogError($"Vicon XR Setting not loaded!");
                return;
            }

            if (instance.settings.EnableXRHandSubsystem)
            {
                SubsystemManager.GetSubsystemDescriptors<XRHandSubsystemDescriptor>(xrHandsSubsystemDescriptors);

                if (xrHandsSubsystemDescriptors.Count > 0)
                {
                    foreach (var descriptor in xrHandsSubsystemDescriptors)
                    {
                        if (String.Compare(descriptor.id, ViconXRConstants.handSubsystemId, true) == 0)
                        {
                            instance.HandSubsystem = descriptor.Create() as ViconHandSubsystem;
                            break;
                        }
                    }
                }
                if (instance.HandSubsystem == null)
                {
                    Debug.LogError($"{typeof(ViconHandSubsystem).Name} failed to configure!");
                }
                else
                {
                    instance.HandSubsystem?.Start();
                    Debug.Log($"{typeof(ViconHandSubsystem).Name} configured!");
                }
            }

            if (instance.settings.EnableViconXRDevice)
            {
                instance.XRDevice = ViconXRDevice.SetupDevice();
            }
        }

        #region Passing data to subsystems
        /// <summary>
        /// If the loader is setup and configured, set the hwd data in the HMD device.
        /// </summary>
        public static void TrySetXRDeviceData(Vector3 pos, Quaternion rot)
        {
            if (instance != null && instance.settings != null)
            {
                instance.XRDevice?.SetDeviceData(pos, rot);
            }
        }
        #endregion
    }
}
