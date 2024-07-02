using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace ubco.ovilab.ViconUnityStream
{
    public class ViconHandSubsystem: XRHandSubsystem
    {
        private ViconHandProvider handsProvider => provider as ViconHandProvider;

        // This method registers the subsystem descriptor with the SubsystemManager
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            var handsSubsystemCinfo = new XRHandSubsystemDescriptor.Cinfo
            {
                id = ViconXRConstants.handSubsystemId,
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
