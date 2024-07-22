using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.XR.CoreUtils;

namespace ubco.ovilab.ViconUnityStream
{
    public class CustomHWDScript : CustomSubjectScript
    {
        [Header("HWD Settings")]
        [Tooltip("If set or game object has XROrigin will configure the XROrigin.")]
        [SerializeField] private XROrigin xrOrigin;
        [Tooltip("Estimate \"True\" centre of the HMD based on vicon tracker markers. A good value seems to be (0,0,-100) to start with")]
        [SerializeField] private Vector3 HMDCentreOffset;

        [Header("SWD One Euro filter settings")]
        [Tooltip("Enables filter for position")]
        [SerializeField] private bool applyPosFilter = false;
        [Tooltip("Filter min cutoff for position filter")]
        [SerializeField] private float posFilterMinCutoff = 0.1f;
        [Tooltip("Beta value for position filter")]
        [SerializeField] private float posFilterBeta = 50;

        [Space()]
        [Tooltip("Enables filter for rotation")]
        [SerializeField] private bool applyRotFilter = false;
        [Tooltip("Filter min cutoff for rotation filter")]
        [SerializeField] private float rotFilterMinCutoff = 0.1f;
        [Tooltip("Beta value for rotation filter")]
        [SerializeField] private float rotFilterBeta = 50;

        private OneEuroFilter<Quaternion> rotFilter;
        private OneEuroFilter<Vector3> posFilter;
        private Vector3 imaginaryCentre;
        protected override void Start()
        {
            base.Start();

            if (xrOrigin == null)
            {
                xrOrigin = GetComponent<XROrigin>();
            }

            if (xrOrigin != null)
            {
                xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            }
        }

        /// <inheritdoc />
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                rotFilter = new OneEuroFilter<Quaternion>(90, rotFilterMinCutoff, rotFilterBeta);
                posFilter = new OneEuroFilter<Vector3>(90, posFilterMinCutoff, posFilterBeta);
            }
        }

        /// <inheritdoc />
        protected override Dictionary<string, Vector3> ProcessSegments(Dictionary<string, Vector3> segments, Data data)
        {
            Vector3 forward = segments["base2"] - segments["base1"];
            Vector3 right = segments["base3"] - segments["base4"];
            Vector3 up = Vector3.Cross(forward, right);
            if (forward == Vector3.zero || up == Vector3.zero) return segments;
            Quaternion rotation = Quaternion.LookRotation(forward, up);

            if (applyPosFilter)
            {
                segments["base1"] = posFilter.Filter(segments["base1"], Time.realtimeSinceStartup);
            }

            if (applyRotFilter)
            {
                rotation = rotFilter.Filter(rotation, Time.realtimeSinceStartup);
            }

            foreach (var key in segmentsRotation.Keys.ToArray())
            {
                segmentsRotation[key] = rotation;
            }
            imaginaryCentre = segments["base1"] + (forward.normalized * HMDCentreOffset.z + up.normalized * HMDCentreOffset.y + right.normalized * HMDCentreOffset.x);
            ViconXRLoader.TrySetXRDeviceData(imaginaryCentre * viconUnitsToUnityUnits, rotation);
            return segments;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(imaginaryCentre * viconUnitsToUnityUnits, 0.1f);
            Gizmos.DrawWireSphere(segments["base1"] * viconUnitsToUnityUnits, 0.1f);
        }
    }
}
