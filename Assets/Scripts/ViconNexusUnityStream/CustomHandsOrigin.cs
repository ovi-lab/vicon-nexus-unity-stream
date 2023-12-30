using UnityEngine;

namespace ubco.ovilab.ViconUnityStream
{
    public class CustomHandsOrigin: MonoBehaviour
    {
        public static Transform origin;

        /// <inheritdoc />
        protected void OnEnable()
        {
            if (origin != null)
            {
                Debug.LogWarning($"CustomHandsOrigin is already set to {origin.name}. Disabling this.");
                gameObject.SetActive(false);
            }
            else
            {
                origin = transform;
            }
        }

        /// <inheritdoc />
        protected void OnDisable()
        {
            if (origin == this.transform)
            {
                origin = null;
            }
        }
    }
}
