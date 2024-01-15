using UnityEngine;

namespace ubco.ovilab.ViconUnityStream
{
    /// <summary>
    /// If this component is placed on any game object, the pose of the <see cref="CustomHandsScript"/> would be relative to this.
    /// </summary>
    public class CustomHandsOrigin: MonoBehaviour
    {
        public static CustomHandsOrigin handsOrigin;
        [Tooltip("The hands will be made relative to this transform.")]
        [SerializeField] protected Transform otherOrigin;

        public static Vector3 TransformPosition(Vector3 position)
        {
            return handsOrigin.otherOrigin.TransformPoint(handsOrigin.transform.InverseTransformPoint(position));
        }

        public static Quaternion TransformRotation(Quaternion rotation)
        {
            return handsOrigin.otherOrigin.rotation * Quaternion.Inverse(handsOrigin.transform.rotation) * rotation;
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
