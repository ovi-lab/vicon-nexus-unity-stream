using System.Collections.Generic;

using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.Hands;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.Management;
#endif

namespace ubco.ovilab.ViconUnityStream
{
#if UNITY_EDITOR
    [XRSupportedBuildTarget(BuildTargetGroup.Standalone, new BuildTarget[] { BuildTarget.StandaloneWindows, BuildTarget.StandaloneWindows64 })]
    [XRSupportedBuildTarget(BuildTargetGroup.Android)]
#endif
    public class ViconXRLoader : XRLoaderHelper
    {
        static List<XRInputSubsystemDescriptor> inputSubsystemDescriptors = new();
        static List<XRHandSubsystemDescriptor> xrHandsSubsystemDescriptors = new();
        private ViconXRSettings settings;
        private static ViconXRLoader loader;

        /// <summary>
        /// Return the currently active Input Subsystem intance, if any.
        /// </summary>
        public XRInputSubsystem inputSubsystem
        {
            get { return GetLoadedSubsystem<XRInputSubsystem>(); }
        }

        /// <summary>
        /// Return the currently active XR Hand Subsystem intance, if any.
        /// </summary>
        public ViconHandSubsystem HandSubsystem { get; private set; }

        /// <summary>
        /// The associated vicon device the loader is managing.
        /// </summary>
        public ViconXRDevice XRDevice { get; private set; }

        ViconXRSettings GetSettings()
        {
            ViconXRSettings settings = null;
            // When running in the Unity Editor, we have to load user's customization of configuration data directly from
            // EditorBuildSettings. At runtime, we need to grab it from the static instance field instead.
#if UNITY_EDITOR
            UnityEditor.EditorBuildSettings.TryGetConfigObject(ViconXRConstants.settingsKey, out settings);
#else
            settings = ViconXRSettings.s_RuntimeInstance;
#endif
            return settings;
        }

        #region XRLoader API Implementation
        /// <summary>Implementaion of <see cref="XRLoader.Initialize"/></summary>
        /// <returns>True if successful, false otherwise</returns>
        public override bool Initialize()
        {
            settings = GetSettings();
            loader = this;

            // CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(s_InputSubsystemDescriptors, "InputSubsystemDescriptor");
            CreateSubsystem<XRHandSubsystemDescriptor, XRHandSubsystem>(xrHandsSubsystemDescriptors, ViconHandSubsystem.id);

            HandSubsystem = GetLoadedSubsystem<XRHandSubsystem>() as ViconHandSubsystem;
            XRDevice = ViconXRDevice.SetupDevice();

            if (HandSubsystem == null)
            {
                Debug.LogError($"{typeof(ViconHandSubsystem).Name} failed to configure!");
            }
            else
            {
                Debug.Log($"{typeof(ViconHandSubsystem).Name} configured!");
            }

            return true;
        }

        /// <summary>Implementaion of <see cref="XRLoader.Start"/></summary>
        /// <returns>True if successful, false otherwise</returns>
        public override bool Start()
        {
            StartSubsystem<XRInputSubsystem>();
            StartSubsystem<XRHandSubsystem>();
            return true;
        }

        /// <summary>Implementaion of <see cref="XRLoader.Stop"/></summary>
        /// <returns>True if successful, false otherwise</returns>
        public override bool Stop()
        {
            StopSubsystem<XRInputSubsystem>();
            StopSubsystem<XRHandSubsystem>();
            return true;
        }

        /// <summary>Implementaion of <see cref="XRLoader.Deinitialize"/></summary>
        /// <returns>True if successful, false otherwise</returns>
        public override bool Deinitialize()
        {
            DestroySubsystem<XRInputSubsystem>();
            DestroySubsystem<XRHandSubsystem>();
            XRDevice.DestroyDevice();
            loader = null;
            return base.Deinitialize();
        }
        #endregion

        #region Passing data to subsystems
        /// <summary>
        /// Set hand subsystem data.
        /// </summary>
        public static void TrySetHandSbsystemData(Handedness handedness, Dictionary<XRHandJointID, Pose> poses)
        {
            if (loader != null)
            {
                loader.HandSubsystem.SetHandPoses(handedness, poses);
            }
        }

        /// <summary>
        /// If the loader is setup and configured, set the hwd data in the HMD device.
        /// </summary>
        public static void TrySetXRDeviceData(Vector3 pos, Quaternion rot)
        {
            if (loader != null)
            {
                pos += loader.settings.HMDPositionOffset;
                loader.XRDevice.SetDeviceData(pos, rot);
            }
        }
        #endregion
    }
}
