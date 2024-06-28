using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace ubco.ovilab.ViconUnityStream
{
    public class ViconHandSubsystem: XRHandSubsystem
    {
        internal static string id = "vicon-hands-subsystem";

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
        /// Set the hand poses to provide through the subsystem.
        /// </summary>
        public void SetHandPoses(Handedness handedness, Dictionary<XRHandJointID, Pose> poses)
        {
            handsProvider.SetHandPoses(handedness, poses);
        }

        /// <inheritdoc />
        public new void Start()
        {
            base.Start();
            handsProvider.Setup(this);
        }
    }
}
