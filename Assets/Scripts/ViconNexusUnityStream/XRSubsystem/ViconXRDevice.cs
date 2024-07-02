using System.Collections.Generic;

using UnityEngine.XR;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;

namespace ubco.ovilab.ViconUnityStream
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    [Preserve, InputControlLayout(displayName = "Vicon Tracking", commonUsages = new[] {"XR", "HMD", "Vicon"})]
    public class ViconXRDevice : XRHMD
    {
        public ViconXRDevice viconXRDevice => device as ViconXRDevice;

        /// <summary>
        /// Setup and return a <see cref="ViconXRDevice"/>.
        /// </summary>
        public static ViconXRDevice SetupDevice()
        {
            InputDeviceDescription desc = new InputDeviceDescription
            {
                product = "Vicon Tracking",
                manufacturer = "Vicon",
                capabilities = new XRDeviceDescriptor
                {
                    characteristics = InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.HeadMounted,
                    inputFeatures = new List<XRFeatureDescriptor>
                    {
                        new XRFeatureDescriptor
                        {
                            name = "sampleFeature",
                            featureType = FeatureType.Binary
                        }
                    }

                }.ToJson()
            };

            return InputSystem.AddDevice(desc) as ViconXRDevice;
        }

        /// <summary>
        /// Callback when device and related subsystems are getting destroyed/deinit
        /// </summary>
        public void Destroy()
        {
            InputSystem.RemoveDevice(viconXRDevice);
        }

#if UNITY_EDITOR
        static ViconXRDevice() => RegisterLayout();
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterLayout()
        {
            InputSystem.RegisterLayout<ViconXRDevice>(
                    matches: new InputDeviceMatcher()
                    .WithProduct("Vicon Tracking")
                    .WithManufacturer("Vicon"));
        }

        /// <summary>
        /// Set data to be passed to the subsystem/device.
        /// </summary>
        public void SetDeviceData(Vector3 pos, Quaternion rot)
        {
            InputSystem.QueueDeltaStateEvent(trackingState, InputTrackingState.Position | InputTrackingState.Rotation);
            InputSystem.QueueDeltaStateEvent(isTracked, true);
            InputSystem.QueueDeltaStateEvent(centerEyePosition, pos);
            InputSystem.QueueDeltaStateEvent(centerEyeRotation, rot);
        }
    }
}
