using System.Collections.Generic;

using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.Hands;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;

namespace ubco.ovilab.ViconUnityStream
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    [Preserve, InputControlLayout(displayName = "Vicon Tracking", commonUsages = new[] { "devicePosition", "deviceRotation" })]
    public class ViconXRDevice : XRHMD
    {
        public static ViconXRDevice viconDevice { get; private set; }

        protected override void FinishSetup()
        {
            base.FinishSetup();

            XRDeviceDescriptor deviceDescriptor = XRDeviceDescriptor.FromJson(description.capabilities);
            if (deviceDescriptor != null)
            {
                if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.HeadMounted) != 0)
                {
                    // TODO: validate if this usage is correct
                    InputSystem.SetDeviceUsage(this, UnityEngine.XR.CommonUsages.devicePosition.name);
                    InputSystem.SetDeviceUsage(this, UnityEngine.XR.CommonUsages.deviceRotation.name);
                }
            }
        }

        public static void SetupDevice()
        {
            InputDeviceDescription desc = new InputDeviceDescription
            {
                product = "Vicon Tracking",
                manufacturer = "Vicon",
                capabilities = new XRDeviceDescriptor
                {
                    characteristics = InputDeviceCharacteristics.TrackedDevice,
                    inputFeatures = new List<XRFeatureDescriptor>
                    {
                        new XRFeatureDescriptor
                        {
                            name = "HWD_position",
                            featureType = FeatureType.Binary
                        },
                        new XRFeatureDescriptor
                        {
                            name = "HWD_rotation",
                            featureType = FeatureType.Binary
                        }
                    }

                }.ToJson()
            };

            viconDevice = InputSystem.AddDevice(desc) as ViconXRDevice;
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

        public void QueueData(Vector3 pos, Quaternion rot)
        {
            InputSystem.QueueDeltaStateEvent(trackingState, InputTrackingState.Position | InputTrackingState.Rotation);
            InputSystem.QueueDeltaStateEvent(isTracked, true);
            InputSystem.QueueDeltaStateEvent(devicePosition, pos);
            InputSystem.QueueDeltaStateEvent(deviceRotation, rot);
        }
    }
}
