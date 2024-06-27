using System.Collections.Generic;

using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.Hands;

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

//         SampleSettings GetSettings()
//         {
//             SampleSettings settings = null;
//             // When running in the Unity Editor, we have to load user's customization of configuration data directly from
//             // EditorBuildSettings. At runtime, we need to grab it from the static instance field instead.
// #if UNITY_EDITOR
//             UnityEditor.EditorBuildSettings.TryGetConfigObject(SampleConstants.k_SettingsKey, out settings);
// #else
//             settings = SampleSettings.s_RuntimeInstance;
// #endif
//             return settings;
//         }

#region XRLoader API Implementation

        /// <summary>Implementaion of <see cref="XRLoader.Initialize"/></summary>
        /// <returns>True if successful, false otherwise</returns>
        public override bool Initialize()
        {
            // // SampleSettings settings = GetSettings();
            // if (settings != null)
            // {
            //     // TODO: Pass settings off to plugin prior to subsystem init.
            // }

            // CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(s_InputSubsystemDescriptors, "InputSubsystemDescriptor");
            CreateSubsystem<XRHandSubsystemDescriptor, XRHandSubsystem>(xrHandsSubsystemDescriptors, ViconHandSubsystem.id);

            return false;
        }

        /// <summary>Implementaion of <see cref="XRLoader.Start"/></summary>
        /// <returns>True if successful, false otherwise</returns>
        public override bool Start()
        {
            StartSubsystem<XRInputSubsystem>();
            return true;
        }

        /// <summary>Implementaion of <see cref="XRLoader.Stop"/></summary>
        /// <returns>True if successful, false otherwise</returns>
        public override bool Stop()
        {
            StopSubsystem<XRInputSubsystem>();
            return true;
        }

        /// <summary>Implementaion of <see cref="XRLoader.Deinitialize"/></summary>
        /// <returns>True if successful, false otherwise</returns>
        public override bool Deinitialize()
        {
            DestroySubsystem<XRInputSubsystem>();
            return base.Deinitialize();
        }

#endregion
    }
}
