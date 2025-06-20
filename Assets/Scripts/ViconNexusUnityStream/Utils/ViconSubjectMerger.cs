using UnityEngine;
using UnityEngine.Events;

namespace ubco.ovilab.ViconUnityStream.Utils
{
    public class ViconSubjectMerger : MonoBehaviour
    {
        /// <summary>
        /// The distance threshold to achieve.
        /// </summary>
        public float DistanceThreshold => distanceThreshold;

        /// <summary>
        /// The angle threshold to achieve.
        /// </summary>
        public float AngleThreshold => angleThreshold;

        /// <summary>
        /// Called when successfully got the differences below the respective thresholds.
        /// </summary>
        public UnityEvent OnMergeSuccess => onMergeSuccess;

        /// <summary>
        /// Called when failed to get the differences below the respective thresholds.
        /// </summary>
        public UnityEvent OnMergeFail => onMergeFail;

        /// <summary>
        /// Called when the differences is above the respective thresholds.
        /// </summary>
        public UnityEvent OnDifferenceAboveThreshold => onDifferenceAboveThreshold;

        [SerializeField] private string targetSubject;

        [Header("Thresholds")]
        [Tooltip("The distance threshold to achieve."), SerializeField]
        protected float distanceThreshold = 0.001f;

        [Tooltip("The angle threshold to achieve"), SerializeField]
        protected float angleThreshold = 0.5f;

        [Header("Merge Events")]
        [Tooltip("Called when the differences is above the respective thresholds."), SerializeField]
        protected UnityEvent onDifferenceAboveThreshold;
        [Tooltip("Called when failed to get the differences below the respective thresholds."), SerializeField]
        protected UnityEvent onMergeFail;
        [Tooltip("Called when successfully got the differences below the respective thresholds."), SerializeField]
        protected UnityEvent onMergeSuccess;

        private Transform Target
        {
            get
            {
                CustomSubjectScript target = ViconCoordinateSystemMerger.Instance.ViconSubjects[targetSubject];
                Debug.Assert(target != null, $"Target `{targetSubject}` not found. Make sure it is in the scene.");
                return target.transform;
            }
        }

        private void Start()
        {
            ViconCoordinateSystemMerger.Instance.RegisterObject(targetSubject, this);
        }

        public virtual void MergeSubject()
        {
            transform.rotation = Target.transform.rotation;
            transform.position =  Target.transform.position;
        }

        /// <summary>
        /// Returns true if differences between the Target Subject and Unity Object are below thresholds.
        /// </summary>
        public bool IsBelowThreshold()
        {
            return Vector3.Angle(transform.forward, Target.forward) < AngleThreshold &&
                   (transform.position - Target.position).magnitude < DistanceThreshold;
        }

        protected virtual void Update()
        {
            if (!IsBelowThreshold())
            {
                Debug.LogWarning($"Target subject `{targetSubject}` is not below threshold");
                OnDifferenceAboveThreshold?.Invoke();
            }
        }
    }
}

