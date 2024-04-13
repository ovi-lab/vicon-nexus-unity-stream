using UnityEngine;

namespace ubco.ovilab.ViconUnityStream
{
    public class HWDFollower: MonoBehaviour
    {
        public Transform base1;
        public Transform base2;
        public Transform base3;
        public Transform base4;

        public bool applyPosFilter = false;
        public bool applyRotFilter = false;
        public float rotFilterMinCutoff = 0.1f, rotFilterBeta = 50;
        public float posFilterMinCutoff = 0.1f, posFilterBeta = 50;

        private OneEuroFilter<Quaternion> rotFilter;
        private OneEuroFilter<Vector3> posFilter;

        void Start()
        {
            OnValidate();
        }

        void OnValidate()
        {
            if (Application.isPlaying)
            {
                rotFilter = new OneEuroFilter<Quaternion>(90, rotFilterMinCutoff, rotFilterBeta);
                posFilter = new OneEuroFilter<Vector3>(90, posFilterMinCutoff, posFilterBeta);
            }
        }

        void Update()
        {
            if (applyPosFilter)
            {
                transform.position = posFilter.Filter(base1.position, Time.realtimeSinceStartup);
            }
            else
            {
                transform.position = base1.position;
            }

            Vector3 forward = base2.position - base1.position;
            if (forward != Vector3.zero)
            {
                Vector3 right = base3.position - base4.position;
                if (right != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(forward, Vector3.Cross(right, -forward));
                    if (applyRotFilter)
                    {
                        transform.rotation = rotFilter.Filter(rotation, Time.realtimeSinceStartup);
                    }
                    else
                    {
                        transform.rotation = rotation;
                    }
                }
            }
        }
    }
}
