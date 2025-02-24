using UnityEngine;

namespace ubco.ovilab.ViconUnityStream.Utils
{
    [CreateAssetMenu(fileName = "HandProperties", menuName = "ViconNexusUnityStream/HandProperties", order = 0)]
    public class HandProperties : ScriptableObject
    {
        [Tooltip("Base normal offset in unity units to place hand from marker positions")]
        [SerializeField] private float baseNormalOffset = 0.001f;

        [Tooltip("Increasing or decreasing the normal offset value by a certain percentage.")] [Range(-100, 100)]
        [SerializeField] private float indexNormalOffset = 0f;
        [Tooltip("Increasing or decreasing the normal offset value by a certain percentage.")] [Range(-100, 100)]
        [SerializeField] private float middleNormalOffset = 0f;
        [Tooltip("Increasing or decreasing the normal offset value by a certain percentage.")] [Range(-100, 100)]
        [SerializeField] private float ringNormalOffset = 0f;
        [Tooltip("Increasing or decreasing the normal offset value by a certain percentage.")] [Range(-100, 100)]
        [SerializeField] private float littleNormalOffset = 0f;
        [Tooltip("Increasing or decreasing the normal offset value by a certain percentage.")] [Range(-100, 100)]
        [SerializeField] private float thumbNormalOffset = 0f;

        /// <summary>
        /// Base normal offset in unity units to place hand from marker positions
        /// </summary>
        public float BaseNormalOffset
        {
            get => baseNormalOffset;
            set => baseNormalOffset = value;
        }

        /// <summary>
        /// Increasing or decreasing the normal offset value of the thumb by a certain percentage.
        /// </summary>
        public float ThumbNormalOffset
        {
            get => thumbNormalOffset;
            set => thumbNormalOffset = value;
        }

        /// <summary>
        /// Increasing or decreasing the normal offset value of the index finger by a certain percentage.
        /// </summary>
        public float IndexNormalOffset
        {
            get => indexNormalOffset;
            set => indexNormalOffset = value;
        }

        /// <summary>
        /// Increasing or decreasing the normal offset value of the middle finger by a certain percentage.
        /// </summary>
        public float MiddleNormalOffset
        {
            get => middleNormalOffset;
            set => middleNormalOffset = value;
        }

        /// <summary>
        /// Increasing or decreasing the normal offset value of the ring finger by a certain percentage.
        /// </summary>
        public float RingNormalOffset
        {
            get => ringNormalOffset;
            set => ringNormalOffset = value;
        }

        /// <summary>
        /// Increasing or decreasing the normal offset value of the little finger by a certain percentage.
        /// </summary>
        public float LittleNormalOffset
        {
            get => littleNormalOffset;
            set => littleNormalOffset = value;
        }
    }
}