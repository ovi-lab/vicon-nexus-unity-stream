using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Net.Http;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ubco.ovilab.ViconUnityStream
{
    public class CustomSubjectScript : MonoBehaviour
    {
        public string subjectName = "test";
        public float scale_1 = 0.001f;
        public float scale_2 = 0.02f;
        public string rootSegment = "Arm";
        public string defaultData = "{\"data\": {\"RWRB\": [-543.6625324688598, 207.2696870612411, 298.7514053730324], \"RFA2\": [-532.0721277646578, 220.17137432871033, 301.01629761935317], \"RFA1\": [-520.1440660572242, 201.39104705712728, 339.41934514555805], \"RWRA\": [-532.6974300716365, 189.02367806197196, 337.42141847242124], \"RH1\": [-560.7345454383594, 159.62419546128493, 330.3774691418835], \"RH3\": [-590.0032621097643, 131.76129785698242, 299.0094709326491], \"RH6\": [-562.2275968721467, 178.22968321613172, 289.7780921183954], \"RTH1\": [-521.0776063001258, 156.02975240617602, 339.52347728151585], \"RTH2\": [-533.1608764185024, 125.93223864863384, 346.6751934035616], \"RTH3\": [-544.0308683261262, 94.88325770113741, 340.18541871909747], \"RTH3P\": [-558.2383999037787, 98.54284010368167, 344.30830190364924], \"RTH4\": [-565.5234537423078, 83.35392844673802, 323.32104505694093], \"RH2\": [-585.6043517556936, 120.51194616610833, 321.788774764582], \"RIF1\": [-576.5885643353779, 78.82999103456628, 300.3582074396528], \"RIF2\": [-561.2383839807005, 59.365051118922004, 295.3749509588552], \"RIF3\": [-541.7682180796493, 45.73872562944883, 292.4172960222232], \"RTF1\": [-572.5416262201588, 87.20318096871338, 285.1907380694027], \"RTF2\": [-562.4119589663006, 62.738780575654275, 271.7188525984546], \"RTF3\": [-557.640123472733, 48.67593410004015, 265.6647298151231], \"RH4\": [-584.7190340388772, 139.86043222122856, 281.7731182084455], \"RRF1\": [-585.3695333810368, 117.21631146711684, 275.4715366418419], \"RRF2\": [-587.3318441581403, 98.12953148435754, 262.2992975340355], \"RRF3\": [-588.7462283921602, 71.40473234827627, 248.39760510410048], \"RRF4\": [-587.6055606791075, 56.662726798838854, 241.1235659054331], \"RH5\": [-576.2821771931049, 149.15875547966468, 267.5793086555055], \"RPF1\": [-587.1382537041475, 129.22951405026535, 250.65671596147286], \"RPF2\": [-593.5975138169013, 115.06673347598871, 237.79061600397645], \"RPF3\": [-598.0083756011302, 98.17580941339943, 225.52275793399647]}, \"hierachy\": {\"Arm\": [\"RWRB\", \"RFA2\", \"RFA1\", \"RWRA\"], \"Hand\": [\"RH1\", \"RH3\", \"RH6\"], \"R1D1\": [\"RTH1\"], \"R1D2\": [\"RTH2\"], \"R1D3\": [\"RTH3\", \"RTH3P\", \"RTH4\"], \"R2D1\": [\"RH2\"], \"R2D2\": [\"RIF1\"], \"R2D3\": [\"RIF2\", \"RIF3\"], \"R3D2\": [\"RTF1\"], \"R3D3\": [\"RTF2\", \"RTF3\"], \"R4D1\": [\"RH4\", \"RRF1\"], \"R4D2\": [\"RRF2\"], \"R4D3\": [\"RRF3\", \"RRF4\"], \"R5D1\": [\"RH5\"], \"R5D2\": [\"RPF1\"], \"R5D3\": [\"RPF2\", \"RPF3\"]}, \"sensorTriggered\": true}";
        private Data defaultDataObj;
        private byte[] defaultDataBytes;

        private static readonly HttpClient client = new HttpClient();

        public Text outputText;
        public event System.Action<Dictionary<string, Transform>> PostTransformCallback;

        [SerializeField]
        protected GapFillingStrategy gapFillingStrategy = GapFillingStrategy.UseRemote;

        public int dataQualityThreshold = 5; // If below this number of markers, stop processing and hide everything
        public UnityEvent OnHidingSubject;
        public UnityEvent OnShowingSubject;
        private bool subjectHidden = false;

        public bool SubjectHidden { get => subjectHidden; }

        bool _sensorTriggered;
        public bool sensorTriggered
        {
            get
            {
                return _sensorTriggered;
            }
            private set
            {
                if (!value)
                    expectSensorChange = false;
                _sensorTriggered = value && !expectSensorChange;
            }
        }
        public bool expectSensorChange { get; set; }

        public List<string> filePaths { get; protected set; }

        StreamWriter inputWriter;
        StreamWriter finalWriter;
        StreamWriter rawWriter;

        string rawData;

        protected Dictionary<string, Vector3> finalPositionVectors = new Dictionary<string, Vector3>();
        protected Dictionary<string, Transform> finalTransforms = new Dictionary<string, Transform>();
        protected Dictionary<string, Vector3> finalUpVectors = new Dictionary<string, Vector3>();
        protected Dictionary<string, Vector3> finalForwardVectors = new Dictionary<string, Vector3>();

        protected Dictionary<string, Vector3> segments = new Dictionary<string, Vector3>();
        protected Dictionary<string, Quaternion> segmentsRotation = new Dictionary<string, Quaternion>();
        protected Dictionary<string, List<string>> segmentMarkers;
        private Dictionary<string, LinkedList<List<float>>> previousData = new Dictionary<string, LinkedList<List<float>>>();
        private int previousDataQueueLimit = 3;

        private float timeSinceLastRequest = 0;

        public SubjectDataManager subjectDataManager;

        protected virtual void Start()
        {
            timeSinceLastRequest = Time.time;
            
            segmentMarkers = new Dictionary<string, List<string>>() {
                { "base1", new List<string>() { "base1"}},
                { "base2", new List<string>() { "base2"}},
                { "base3", new List<string>() { "base3"}},
                { "base4", new List<string>() { "base4"}},
                { "base5", new List<string>() { "base5"}}
            };

            SetupWriter();
            SetupFilter();
        }

        /// <inheritdoc />
        protected virtual void OnEnable()
        {
            subjectDataManager.RegisterSubject(subjectName);
        }
        
        /// <inheritdoc />
        protected virtual void OnDisable()
        {
            subjectDataManager.UnRegsiterSubject(subjectName);
        }

        protected void LateUpdate()
        {
            if (subjectDataManager.UseDefaultData)
            {
                if (defaultDataObj == null)
                {
                    defaultDataObj = JsonConvert.DeserializeObject<Data>(defaultData);
                    Debug.Log($"{defaultDataObj.data}");
                }
                ProcessData(defaultDataObj, defaultData);
            }
            else
            {
                // TODO: move all of this to SubjectDataManager
                if (subjectDataManager.StreamedData.TryGetValue(subjectName, out Data subjectDataObj) && subjectDataManager.StreamedRawData.TryGetValue(subjectName, out string subjectRawData))
                {
                    ProcessData(subjectDataObj, subjectRawData);
                }
                // TODO: Write empty lines when data not in frame?
                // else { }
            }
        }

        protected void SetupWriter()
        {
            if (!subjectDataManager.EnableWriteData)
                return;

            filePaths = new List<string>();

            string filePath;

            filePath = GetPath("input"); 
            inputWriter = new StreamWriter(filePath, true);
            filePaths.Add(filePath);
            Debug.Log("Writing to:  " + filePath);

            filePath = GetPath("final"); 
            finalWriter = new StreamWriter(filePath, true);
            filePaths.Add(filePath);
            Debug.Log("Writing to:  " + filePath);

            filePath = GetPath("raw"); 
            rawWriter = new StreamWriter(filePath, true);
            filePaths.Add(filePath);
            Debug.Log("Writing to:  " + filePath);
        }

        protected void SetupFilter()
        {
        }

        private string GetPath(string suffix)
        {
            return Application.persistentDataPath + "/stream_" + suffix + "_" + this.transform.name + "_" + DateTime.Now.ToString("dd-MM-yy hh-mm-ss") + ".csv";
        }

        public void WriteData()
        {
            if (!subjectDataManager.EnableWriteData || subjectDataManager.UseDefaultData)
                return;
            
            var currentTicks = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            
            inputWriter.WriteLine(currentTicks + ", " + "{" + string.Join(",", segments.Select(kvp => "[" +kvp.Key + ", " + kvp.Value.ToString("F6") + "]")) + "}");
            inputWriter.Flush();

            finalWriter.WriteLine(currentTicks + ", " + ConstructFinalWriterString());
            finalWriter.Flush();

            rawWriter.WriteLine(currentTicks + ", " + rawData);
            rawWriter.Flush();
        }

        protected virtual string ConstructFinalWriterString()
        {
            return "{ 'positions':" + string.Join(",", finalPositionVectors.Select(kvp => "[" +kvp.Key + ", " + kvp.Value.ToString("F6") + "]")) +
                ", 'up':" + string.Join(",", finalUpVectors.Select(kvp => "[" +kvp.Key + ", " + kvp.Value.ToString("F6") + "]")) +
                ", 'forward':" + string.Join(",", finalForwardVectors.Select(kvp => "[" +kvp.Key + ", " + kvp.Value.ToString("F6") + "]")) +
                "}";
        }

        private List<string> invalidMarkers = new List<string>();
        private List<float> k_curr, k_prev;
        private LinkedList<List<float>> markerQueue;
        private Vector3 pos, k_vector, t_prev_vector, t_current_vector;
        private Quaternion rot;
        private UnityWebRequest currentWebRequest;

        void ProcessData(Data data, string text)
        {
            rawData = text;
            sensorTriggered = data.sensorTriggered;
	
            if (outputText)
            {
                outputText.text = data.sensorTriggered.ToString();
                outputText.color = data.sensorTriggered ? Color.red: Color.blue;
            }

            int zeroMarkers = 0;

            foreach (KeyValuePair<string, List<string>> segment in segmentMarkers)
            {
                pos = Vector3.zero;
                rot = Quaternion.identity;
                bool dataValid = true;
                invalidMarkers.Clear();

                foreach (string marker in segment.Value)
                {
                    var _data = data.data[marker];

                    /// Need to run gap fillling stratergy
                    if (_data[0] == 0)
                    {
                        if (gapFillingStrategy == GapFillingStrategy.UsePrevious && previousData.ContainsKey(marker) && previousData[marker].Count > 0)
                        {
                            _data = GetPreviousData(marker);
                        }
                        /// NOTE: Rest of GapFillingStrategy.Ignore handled in ApplyBoneTransform
                        else if(gapFillingStrategy == GapFillingStrategy.Ignore)
                        {
                            dataValid = false;
                            break;
                        }
                        else if (gapFillingStrategy == GapFillingStrategy.FillRelative)
                        {
                            /// FillRelative would be a special case of Ignore
                            dataValid = false;
                            /// makes sense to do this only if thre are more than one markers
                            if (segment.Value.Count > 1)
                            {
                                /// Pushing to the quque so that all markers in the group have the same index;
                                SetPreviousData(marker, _data);
                                invalidMarkers.Add(marker);
                            }
                        }
                        /// GapFillingStrategy.UseRemote, basically means to use the _data as recived. Nothing to do for that case
                    }
                    else
                    {
                        SetPreviousData(marker, _data);
                    }
                    data.data[marker] = _data;
                }

                if (gapFillingStrategy == GapFillingStrategy.FillRelative && !dataValid && segment.Value.Count > 1)
                {
                    /// If all data is invalid or there is no previous data,
                    /// skip that data from being commited to previousData
                    if (invalidMarkers.Count == segment.Value.Count || previousData[segment.Value[0]].Count <= 1)
                    {
                        foreach(string marker in segment.Value)
                        {
                            previousData[marker].RemoveLast();
                        }
                    }
                    /// We can salvage this data
                    else
                    {
                        /// let k be the point which has data in previous and current frame
                        /// let t be a point which has data in previous frame but not current
                        /// t_curr (the estimate) = (t_prev - k_prev) + (k_curr - k_prev) + k_prev
                        /// t_curr = t_prev + (k_curr - k_prev)

                        /// pick a k
                        string k_marker = segment.Value.Where(x => !invalidMarkers.Contains(x)).ToArray()[0];
                        markerQueue = previousData[k_marker];
                        k_prev = markerQueue.ElementAt(markerQueue.Count - 2);
                        k_curr = GetPreviousData(k_marker);

                        k_vector = ListToVector(k_curr) - ListToVector(k_prev);

                        foreach (string t_marker in invalidMarkers)
                        {
                            markerQueue = previousData[t_marker];
                            t_prev_vector = ListToVector(markerQueue.ElementAt(markerQueue.Count - 2));
                            t_current_vector = t_prev_vector + k_vector;

                            /// Set the new value to the previous list
                            previousData[t_marker].Last.Value[0] = t_current_vector.x;
                            /// z <-> y because of difference in coord system of vicon and unity
                            previousData[t_marker].Last.Value[1] = t_current_vector.z;
                            previousData[t_marker].Last.Value[2] = t_current_vector.y;

                            /// Set that to the current data object
                            data.data[t_marker] = GetPreviousData(t_marker);
                        }
                        dataValid = true; /// Data is now valid
                    }
                }

                if (dataValid)
                {
                    foreach (string marker in segment.Value)
                    {
                        List<float> _pos = data.data[marker];
                        pos += ListToVector(_pos);
                        //break;
                        if (_pos.Count > 3)
                        {
                            rot.x = _pos[3];
                            rot.y = _pos[4];
                            rot.z = _pos[5];
                            rot.w = _pos[6];
                        }
                    }

                    segments[segment.Key] = pos / segment.Value.Count;
                    segmentsRotation[segment.Key] = rot;
                }
                else
                {
                    segments[segment.Key] = Vector3.zero;
                    segmentsRotation[segment.Key] = Quaternion.identity;
                }

                /// Test the data quality
                if(segments[segment.Key] == Vector3.zero)
                {
                    zeroMarkers += 1;
                }

                if(zeroMarkers > dataQualityThreshold)
                {
                    HideSubject();
                    break;
                }
            }

            segments = ProcessSegments(segments, data);

            ///Retest the data quality
            if(TestSegmentsQuality(segments))
            {
                ShowSubject();
            }
            else
            {
                HideSubject();
            }

            if(!subjectHidden)
            {
                // inputWriter.WriteLine(SceneProperties.currentTicks + ", " + "{" + string.Join(",", segments) + "}");
                // inputWriter.Flush();
                transform.position = segments[rootSegment] * scale_1;
                FindAndTransform(transform, rootSegment);

                if (PostTransformCallback != null)
                    PostTransformCallback(finalTransforms);

                CommitPreviousData();
            }

            WriteData();
        }

        private Vector3 ListToVector(List<float> list)
        {
            /// The vicon output uses a different coordinate system
            /// z <-> y because of difference in coord system of vicon and unity
            return new Vector3(list[0], list[2], list[1]);
        }

        private List<float> GetPreviousData(string marker)
        {
            return previousData[marker].Last();
        }

        /// makes sure the length of the queue is going to be fixed (by previousDataQueueLimit)
        private void SetPreviousData(string marker, List<float> value)
        {
            LinkedList<List<float>> _previousData;
            if (!previousData.ContainsKey(marker))
            {
                _previousData = previousData[marker] = new LinkedList<List<float>>();
            }
            else
            {
                _previousData = previousData[marker];
            }
            _previousData.AddLast(value);
        }

        private void CommitPreviousData()
        {
            foreach (LinkedList<List<float>> _previousData in previousData.Values)
            {
                if (_previousData.Count > previousDataQueueLimit)
                {
                    _previousData.RemoveFirst();
                }
            }
        }

        protected virtual Dictionary<string, Vector3> ProcessSegments(Dictionary<string, Vector3> segments, Data data)
        {
            return segments;
        }

        protected virtual void FindAndTransform(Transform iTransform, string BoneName)
        {
            int ChildCount = iTransform.childCount;
            for (int i = 0; i < ChildCount; ++i)
            {
                Transform Child = iTransform.GetChild(i);
                if (Child.name == BoneName)
                {
                    ApplyBoneTransform(Child);
                    TransformChildren(Child);
                    break;
                }
                // if not finding root in this layer, try the children
                FindAndTransform(Child, BoneName);
            }
        }

        protected void TransformChildren(Transform iTransform)
        {
            int childCount = iTransform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                Transform Child = iTransform.GetChild(i);
                this.ApplyBoneTransform(Child);
                TransformChildren(Child);
            }
        }

        protected virtual void ApplyBoneTransform(Transform Bone)
        {
            string BoneName = Bone.gameObject.name;
            if (segments.TryGetValue(BoneName, out Vector3 segment))
            {
                Bone.position = segment * scale_1;
                Bone.rotation = segmentsRotation[BoneName];
            }
            AddBoneDataToWriter(Bone);
        }

        protected void AddBoneDataToWriter(Transform Bone)
        {
            finalPositionVectors[Bone.name] = Bone.position;
            finalTransforms[Bone.name] = Bone;
            // finalPositionQuaternion[Bone.name] = Bone.rotation;
            finalUpVectors[Bone.name] = Bone.up;
            finalForwardVectors[Bone.name] = Bone.forward;
        }

        protected void HideSubject()
        {
            if (subjectHidden) return;
            OnHidingSubject.Invoke();
            subjectHidden = true;

            int ChildCount = transform.childCount;
            for (int i = 0; i < ChildCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        protected void ShowSubject()
        {
            if (!subjectHidden) return;
            OnShowingSubject.Invoke();
            subjectHidden = false;

            int ChildCount = transform.childCount;
            for (int i = 0; i < ChildCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Returns true if the `segments` are good to be applied to transforms
        /// </summary>
        protected virtual bool TestSegmentsQuality(Dictionary<string, Vector3> segments)
        {
            return true;
        }

        void OnDestroy()
        {
            if (subjectDataManager.EnableWriteData)
            {
                Debug.Log("Closing files: \n    " + string.Join("\n    ", filePaths));
                inputWriter?.Close();
                finalWriter?.Close();
                rawWriter?.Close();
            }
        }
    }

    public enum GapFillingStrategy
    {
        UseRemote,
        Ignore,
        UsePrevious,
        FillRelative
    }
}
