using UnityEngine;

namespace ubco.ovilab.ViconUnityStream
{
    /// <summary>
    /// If this component is placed on any game object, the pose of the <see cref="CustomHandsScript"/> would be relative to this.
    /// </summary>
    public class CustomHandsOrigin: MonoBehaviour
    {
        public static Transform viconOrigin;

        /// <inheritdoc />
        protected void OnEnable()
        {
            if (viconOrigin != null)
            {
                Debug.LogWarning($"CustomHandsOrigin is already set to {viconOrigin.name}. Disabling this.");
                gameObject.SetActive(false);
            }
            else
            {
                viconOrigin = transform;
            }
        }

        /// <inheritdoc />
        protected void OnDisable()
        {
            if (viconOrigin == this.transform)
            {
                viconOrigin = null;
            }
        }
    }
}
