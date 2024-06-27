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
    [XRSupportedBuildTarget(BuildTargetGroup.Standalone, new BuildTarget[]{ BuildTarget.StandaloneWindows, BuildTarget.StandaloneWindows64})]
    [XRSupportedBuildTarget(BuildTargetGroup.Android)]
#endif
    public class ViconXRLoader : XRLoaderHelper
    {
        static List<XRInputSubsystemDescriptor> inputSubsystemDescriptors = new ();
        static List<XRHandSubsystemDescriptor> xrHandsSubsystemDescriptors = new ();

        /// <summary>Return the currently active Input Subsystem intance, if any.</summary>
        public XRInputSubsystem inputSubsystem
        {
            get { return GetLoadedSubsystem<XRInputSubsystem>(); }
        }

        /// <summary>Return the currently active XR Hand Subsystem intance, if any.</summary>
        public XRHandSubsystem handSubsystem
        {
            get { return GetLoadedSubsystem<XRHandSubsystem>(); }
        }

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
            ViconXRSettings settings = GetSettings();
            // if (settings != null)
            // {
            //     // TODO: Pass settings off to plugin prior to subsystem init.
            // }

            // CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(s_InputSubsystemDescriptors, "InputSubsystemDescriptor");
            CreateSubsystem<XRHandSubsystemDescriptor, XRHandSubsystem>(xrHandsSubsystemDescriptors, ViconHandSubsystem.id);

            ViconHandSubsystem.subsystem = GetLoadedSubsystem<XRHandSubsystem>() as ViconHandSubsystem;

            if (ViconHandSubsystem.subsystem == null)
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
            return base.Deinitialize();
        }

#endregion
    }
}
