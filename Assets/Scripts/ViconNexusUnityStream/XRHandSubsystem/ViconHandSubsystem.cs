using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace ubco.ovilab.ViconUnityStream
{
    public class ViconHandSubsystem: XRHandSubsystem
    {
        internal static string id = "vicon-hands-subsystem";

        public static ViconHandSubsystem subsystem;

        private ViconHandProvider handsProvider => provider as ViconHandProvider;

        // This method registers the subsystem descriptor with the SubsystemManager
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            var handsSubsystemCinfo = new XRHandSubsystemDescriptor.Cinfo
            {
                id = id,
                providerType = typeof(ViconHandProvider),
                subsystemTypeOverride = typeof(ViconHandSubsystem)
            };
            XRHandSubsystemDescriptor.Register(handsSubsystemCinfo);
        }

        /// <summary>
        /// Initilize the hand subsystem
        /// </summary>
        public static void MaybeInitializeHandSubsystem()
        {
            if (subsystem == null)
            {
                // FIXME: Do we have to disable other hand subsytems? get from a scriptable object.
                // {
                //     List<XRHandSubsystem> currentHandSubsystems = new List<XRHandSubsystem>();
                //     SubsystemManager.GetSubsystems(currentHandSubsystems);
                //     foreach (XRHandSubsystem handSubsystem in currentHandSubsystems)
                //     {
                //         if (handSubsystem.running)
                //             handSubsystem.Stop();
                //     }
                // }

                List<XRHandSubsystemDescriptor> descriptors = new List<XRHandSubsystemDescriptor>();
                SubsystemManager.GetSubsystemDescriptors(descriptors);
                for (var i = 0; i < descriptors.Count; ++i)
                {
                    var descriptor = descriptors[i];
                    if (descriptor.id == id)
                    {
                        subsystem = descriptor.Create() as ViconHandSubsystem;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Set the hand poses to provide through the subsystem.
        /// </summary>
        public void SetHandPoses(Handedness handedness, Dictionary<XRHandJointID, Pose> poses)
        {
            handsProvider.SetHandPoses(handedness, poses);
        }
    }
}
