using UnityEngine;
using UnityEngine.Events;

namespace ubco.ovilab.ViconUnityStream.Utils
{
    /// <summary>
    /// Repositions the XROrigin transform such that the difference
    /// between the gloabl position and rotation difference between the
    /// xrHWD object and viconHWD object are below distanceThreshold
    /// and angleThreshold respectively.
    /// </summary>
    public class HWDMerger : MonoBehaviour
    {
        [Tooltip("The XR Origin transform"), SerializeField]
        private Transform _xrOrigin;

        /// <summary>
        /// The XR Origin transform
        /// </summary>
        public Transform xrOrigin => _xrOrigin;

        [Tooltip("The Vicon HWD. Should be the transform that has the CustomHWDScript. Make sure to have the Vicon XR Devices disabled when using this component."), SerializeField]
        private Transform _viconHWD;

        /// <summary>
        /// The Vicon HWD. Should be the transform that has the CustomHWDScript. Make sure to have the Vicon XR Devices disabled when using this component.
        /// </summary>
        public Transform viconHWD => _viconHWD;

        [Tooltip("The transform with the camera component that follows the HWD position and rotation."), SerializeField]
        private Transform _xrHWD;

        /// <summary>
        /// The transform with the camera component that follows the HWD position and rotation.
        /// </summary>
        public Transform xrHWD => _xrHWD;

        [Tooltip("The distance threshold to achieve."), SerializeField]
        private float distanceThreshold = 0.001f;

        /// <summary>
        /// The distance threshold to achieve.
        /// </summary>
        public float DistanceThreshold => distanceThreshold;

        [Tooltip("The angle threshold to achieve"), SerializeField]
        private float angleThreshold = 0.5f;

        /// <summary>
        /// The angle threshold to achieve.
        /// </summary>
        public float AngleThreshold => distanceThreshold;

        [Tooltip("Called when successfully got the differences below the respective thresholds."), SerializeField] private UnityEvent onSuccess;

        /// <summary>
        /// Called when successfully got the differences below the respective thresholds.
        /// </summary>
        public UnityEvent OnSuccess => onSuccess;

        [Tooltip("Called when failed to get the differences below the respective thresholds."), SerializeField] private UnityEvent onFail;

        /// <summary>
        /// Called when failed to get the differences below the respective thresholds.
        /// </summary>
        public UnityEvent OnFail => onFail;

        /// <summary>
        /// Move the XR origin such that the Vicon HWD and XR HWD transforms are within specified thresholds.
        /// </summary>
        public void MergeHWDs()
        {
            bool success = false;
            for (int i = 0; i < 5; ++i)
            {
                Vector3 posDiff = xrHWD.position - viconHWD.position;
                Quaternion rotDiff = xrHWD.rotation * Quaternion.Inverse(viconHWD.rotation);
                xrOrigin.position = xrOrigin.position - posDiff;
                xrOrigin.rotation = xrOrigin.rotation * Quaternion.Inverse(rotDiff);
                if (Vector3.Angle(viconHWD.forward, xrHWD.forward) < AngleThreshold && (viconHWD.position - xrHWD.position).magnitude < DistanceThreshold)
                {
                    success = true;
                    OnSuccess.Invoke();
                    break;
                }
            }
            if (!success)
            {
                OnFail.Invoke();
                Debug.LogError($"Failed to merge vicon and xr");
            }
        }
    }
}
