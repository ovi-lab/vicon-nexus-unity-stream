using UnityEngine;

namespace ubco.ovilab.ViconUnityStream
{
    /// <summary>
    /// If this component is placed on any game object, the pose of the <see cref="CustomHandsScript"/> would be relative to this.
    /// </summary>
    public class CustomHandsOrigin: MonoBehaviour
    {
        public static CustomHandsOrigin handsOrigin;
        // i.e., akin to making the object child of `handsOrigin`, and then making `otherOrigin` as the parent while keeping the local pose. 
        [Tooltip("The hands will be made relative to this transform.")]
        [SerializeField] protected Transform otherOrigin;

        /// <summary>
        /// Transform position such that the returned position is relative to the
        /// otherOrigin is the same as the position relative to the handsOrigin
        /// </summary>
        public static Vector3 TransformPosition(Vector3 position)
        {
            if (handsOrigin.otherOrigin == null)
            {
                return position;
            }
            return handsOrigin.otherOrigin.TransformPoint(handsOrigin.transform.InverseTransformPoint(position));
        }

        public static Quaternion TransformRotation(Quaternion rotation)
        {
            if (handsOrigin.otherOrigin == null)
            {
                return rotation;
            }
            return handsOrigin.otherOrigin.rotation * Quaternion.Inverse(handsOrigin.transform.rotation) * rotation;
        }

        /// <summary>
        /// Get pose inverse transformed by the otherOrigin
        /// </summary>
        public static Pose InverseTransformPose(Pose pose)
        {
            if (handsOrigin.otherOrigin == null)
            {
                return pose;
            }
            return new Pose(handsOrigin.otherOrigin.parent.parent.InverseTransformPoint(pose.position), Quaternion.Inverse(handsOrigin.otherOrigin.parent.parent.rotation) * pose.rotation);
        }

        /// <inheritdoc />
        protected void OnEnable()
        {
            if (handsOrigin != null)
            {
                Debug.LogWarning($"CustomHandsOrigin is already set to {handsOrigin.name}. Disabling this.");
                gameObject.SetActive(false);
            }
            else
            {
                handsOrigin = this;
            }
        }

        /// <inheritdoc />
        protected void OnDisable()
        {
            if (handsOrigin == this)
            {
                handsOrigin = null;
            }
        }
    }
}
