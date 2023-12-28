using UnityEngine;
using UnityEngine.XR.Hands;

namespace ubco.ovilab.ViconUnityStream
{
    public class ViconHandSubsystem: XRHandSubsystem
    {
        // This method registers the subsystem descriptor with the SubsystemManager
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            var handsSubsystemCinfo = new XRHandSubsystemDescriptor.Cinfo
            {
                id = "Vicon-hands",
                providerType = typeof(ViconHandProvider),
                subsystemTypeOverride = typeof(ViconHandSubsystem)
            };
            XRHandSubsystemDescriptor.Register(handsSubsystemCinfo);
        }
    }
}
