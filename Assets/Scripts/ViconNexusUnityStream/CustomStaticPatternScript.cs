using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ubco.ovilab.ViconUnityStream
{
    public class CustomStaticPatternScript : CustomSubjectScript
    {
        [Serializable]
        struct SegmentMarkerPattern
        {
            public string name;
            public List<string> markerNames;
        }

        [Tooltip("The patterns used. The markers names should match the names in Nexus. The segment name should match the values set in the forward/right segment and also the objects of the seleton")]
        [SerializeField] private List<SegmentMarkerPattern> patterns;
        [Tooltip("The first segment to use to compute the forward vector")]
        [SerializeField] private string forwardSegment1;
        [Tooltip("The second segment to use to compute the forward vector")]
        [SerializeField] private string forwardSegment2;

        [Tooltip("If set use right segment 1 and 2 to compute right. Else use the up segments.")]
        [SerializeField] private bool specifyRight = true;

        [Tooltip("The first segment to use to compute the right vector")]
        [SerializeField] private string rightSegment1;
        [Tooltip("The second segment to use to compute the right vector")]
        [SerializeField] private string rightSegment2;

        [Tooltip("The first segment to use to compute the up vector")]
        [SerializeField] private string upSegment1;
        [Tooltip("The second segment to use to compute the up vector")]
        [SerializeField] private string upSegment2;

        protected override void Start()
        {
            base.Start();
            OnValidate();
        }

        /// <inheritdoc />
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                if (segmentMarkers == null)
                {
                    segmentMarkers = new Dictionary<string, List<string>>();
                }
                segmentMarkers.Clear();
                foreach (SegmentMarkerPattern pattern in patterns)
                {
                    segmentMarkers.Add(pattern.name, pattern.markerNames);
                }
            }
        }

        /// <inheritdoc />
        protected override Dictionary<string, Vector3> ProcessSegments(Dictionary<string, Vector3> segments, ViconStreamData viconStreamData)
        {
            Vector3 forward;
            Vector3 right;
            Vector3 up;

            if (segments.TryGetValue(forwardSegment1, out Vector3 forward1) && segments.TryGetValue(forwardSegment2, out Vector3 forward2))
            {
                forward = forward2 - forward1;
            }
            else
            {
                Debug.LogError($"Missing segment. Make sure `forwardSegment1` and `forwardSegment2` are also in `pattern`");
                return segments;
            }

            if (specifyRight)
            {
                if (segments.TryGetValue(rightSegment1, out Vector3 right1) && segments.TryGetValue(rightSegment2, out Vector3 right2))
                {
                    right = right2 - right1;
                }
                else
                {
                    Debug.LogError($"Missing segment. Make sure `rightSegment1` and `rightSegment2` are also in `pattern`");
                    return segments;
                }

                up = Vector3.Cross(right, forward);
            }
            else
            {
                if (segments.TryGetValue(upSegment1, out Vector3 up1) && segments.TryGetValue(upSegment2, out Vector3 up2))
                {
                    up = up2 - up1;
                }
                else
                {
                    Debug.LogError($"Missing segment. Make sure `upSegment1` and `upSegment2` are also in `pattern`");
                    return segments;
                }
            }

            Quaternion rot = Quaternion.LookRotation(forward, up);
            foreach(string segmentName in segments.Keys)
            {
                segmentsRotation[segmentName] = rot;
            }

            return segments;
        }
    }
}
