using UnityEngine;
using UnityEngine.Events;

namespace ubco.ovilab.ViconUnityStream.Utils
{
    /// <summary>
    /// Repositions the MergerOffsetTransform transform such that the difference
    /// between the global position and rotation difference between the
    /// xrHWD object and viconHWD object are below distanceThreshold
    /// and angleThreshold respectively.
    /// </summary>
    public class HWDViconMerger : ViconSubjectMerger
    {
        [Tooltip("The transform to which the offset is applied to. Ideally, this is a GameObject under the CameraOffset. The main camera transform in xrHWD should be in a child GameObject of this."), SerializeField]
        private Transform _mergerOffsetTransform;

        /// <summary>
        /// The XR Origin transform
        /// </summary>
        public Transform mergerOffsetTransform => _mergerOffsetTransform;

        [Tooltip("The Vicon HWD. Should be the transform that has the CustomHWDScript. Make sure to have the Vicon XR Devices disabled when using this component."), SerializeField]
        private Transform _viconHWD;

        /// <summary>
        /// The Vicon HWD. Should be the transform that has the CustomHWDScript. Make sure to have the Vicon XR Devices disabled when using this component.
        /// </summary>
        public Transform viconHWD => _viconHWD;

        [Tooltip("The transform with the camera component that follows the HWD position and rotation. Generally, this the Main Camera under the XR Origin."), SerializeField]
        private Transform _xrHWD;

        /// <summary>
        /// The transform with the camera component that follows the HWD position and rotation. Generally, this the Main Camera under the XR Origin.
        /// </summary>
        public Transform xrHWD => _xrHWD;


        /// <inheritdoc />
        protected void OnEnable()
        {
            Debug.Assert(xrHWD.parent == mergerOffsetTransform, "mergerOffsetTransform's should be the parent of xrHWD");
        }

        /// <summary>
        /// Move the XR origin such that the Vicon HWD and XR HWD transforms are within specified thresholds.
        /// </summary>
        public override void MergeSubject()
        {
            bool success = false;
            for (int i = 0; i < 5; ++i)
            {
                mergerOffsetTransform.localPosition = Vector3.zero;
                mergerOffsetTransform.localRotation = Quaternion.identity;

                Transform parent = mergerOffsetTransform.parent;

                Quaternion localXRRotRelToParent = Quaternion.Inverse(parent.rotation) * xrHWD.rotation;
                Quaternion localViconRotRelToParent = Quaternion.Inverse(parent.rotation) * viconHWD.rotation;
                mergerOffsetTransform.localRotation = localViconRotRelToParent * Quaternion.Inverse(localXRRotRelToParent);

                Vector3 localXRPosRelToParent = parent.InverseTransformPoint(xrHWD.position);
                Vector3 localViconPosRelToParent = parent.InverseTransformPoint(viconHWD.position);
                mergerOffsetTransform.localPosition = localViconPosRelToParent - localXRPosRelToParent;

                if (IsBelowThreshold())
                {
                    success = true;
                    OnMergeSuccess.Invoke();
                    break;
                }
            }
            if (!success)
            {
                OnMergeFail.Invoke();
                Debug.LogError($"Failed to merge vicon and xr");
            }
        }

    }
}
