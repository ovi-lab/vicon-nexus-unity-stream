using System.Collections.Generic;
using NativeWebSocket;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using UnityEditor;
using System.Collections;

namespace ubco.ovilab.ViconUnityStream
{
    public class SubjectDataManager : MonoBehaviour
    {
        public enum FileSaveLocation
        {
            /// <summary>
            /// Use <see cref="Application.dataPath"/>
            /// </summary>
            dataPath = 0,
            /// <summary>
            /// Use <see cref="Application.persistentDataPath"/>
            /// </summary>
            persistentDataPath = 1,
            /// <summary>
            /// Use a fixed path.
            /// </summary>
            fixedPath = 2
        }

        [Tooltip("The Webscoket URL used for connection.")]
        [SerializeField] private string baseURI = "ws://viconmx.hcilab.ok.ubc.ca:5001/markers/";
        /// <summary>
        /// The Webscoket URL used for connection.
        /// </summary>
        public string BaseURI { get => baseURI; set => baseURI = value; }

        [Tooltip("Should the subjects use default, recorded or live streamed data")]
        [SerializeField]
        private StreamType streamType;
        public StreamType StreamType
        {
            get => streamType;
            set
            {
                streamType = value;
                ProcessDefaultDataAndWebSocket();
                LoadRecordedJsonl();
            }
        }

        [Tooltip("Enable writing data to disk.")]
        [SerializeField] private bool enableWriteData = false;
        /// <summary>
        /// Enable writing data to disk.
        /// </summary>
        public bool EnableWriteData {
            get => enableWriteData;
            set
            {
                if (enableWriteData)
                {
                    forceWrite = true;
                }
                enableWriteData = value;
            }
        }

        [SerializeField, Tooltip("The base location, relative to which the file will be written.")]
        private FileSaveLocation fileSaveLocationBase;

        /// <summary>
        /// The base location, relative to which the file will be written.
        /// </summary>
        public FileSaveLocation FileSaveLocationBase { get => fileSaveLocationBase; set => fileSaveLocationBase = value; }

        [SerializeField, Tooltip("The base location when FileSaveLocationBase is Fixed.")]
        private string fixedFileSaveLocationBase = "";

        /// <summary>
        /// The base location when FileSaveLocationBase is Fixed.
        /// <seealso cref="FileSaveLocationBase.fixedPath"/>
        /// </summary>
        public string FixedFileSaveLocationBase { get => fixedFileSaveLocationBase; set => fixedFileSaveLocationBase = value; }

        [Tooltip("Path to write the subject data file. Will be relative to fileSaveLocationBase.")]
        [SerializeField]
        private string pathToDataFile;

        /// <summary>
        /// Path to write the subject data file. Will be relative to fileSaveLocationBase.
        /// </summary>
        public string PathToDataFile { get => pathToDataFile; set => pathToDataFile = value; }

        [SerializeField, Tooltip("The file saved would have this prifix + date string")]
        private string fileNameBase = "Session";

        /// <summary>
        /// The file saved would have this prifix + date string.
        /// </summary>
        public string FileNameBase { get => fileNameBase; set => fileNameBase = value; }

        [SerializeField] private int totalFrames = 0;

        /// <summary>
        /// When using StreamType.Recorded, the total number of frames in the data that is loaded.
        /// </summary>
        public int TotalFrames { get => totalFrames; }

        [SerializeField] private int currentFrame = 0;

        /// <summary>
        /// When using StreamType.Recorded, the frame number in the loaded data being played.
        /// </summary>
        public int CurrentFrame { get => currentFrame; set => currentFrame = value; }

        [SerializeField] private bool play;
        /// <summary>
        /// When using StreamType.Recorded, if the data manager should progress to next frame automatically or not.
        /// </summary>
        public bool Play { get => play; set => play = value; }

        [SerializeField, Tooltip("The jsonl files to load when using StreamType.Recorded")]
        private List<TextAsset> jsonlFilesToLoad = new();

        /// <summary>
        /// The jsonl files to load when using StreamType.Recorded.
        /// </summary>
        /// <seealso cref="AddToJsonlFilesToLoad"/>
        /// <seealso cref="RemoveFromJsonlFilesToLoad"/>
        /// <seealso cref="ClearJsonlFilesToLoad"/>
        public ReadOnlyCollection<TextAsset> JsonlFilesToLoad { get => jsonlFilesToLoad.AsReadOnly(); }

        /// <summary>
        /// Deserialized data recieved by the data manager.
        /// </summary>
        public Dictionary<string, ViconStreamData> StreamedData => data;

        /// <summary>
        /// The raw data being recieved by data manager.
        /// </summary>
        public Dictionary<string, string> StreamedRawData => rawData;

        /// <summary>
        /// The path to which the data is being recorded.
        /// <seealso cref="SetPathToRecordedData"/>
        /// </summary>
        public string PathToRecordedData { get => pathToRecordedData; }

        private List<string> subjectList = new();
        private WebSocket webSocket;
        private string pathBaseToRecordedData,
            pathToRecordedData;
        private Dictionary<string, ViconStreamData> data = new();
        private Dictionary<string, string> rawData = new();
        private Dictionary<string, Dictionary<string, ViconStreamData>> recordedData = new();
        private Task task;
        private Dictionary<string, Dictionary<string, ViconStreamData>> dataToWrite;
        private bool forceWrite;
        private readonly object dataLock = new object();

        private void Awake()
        {
            dataToWrite = new Dictionary<string, Dictionary<string, ViconStreamData>>();
            SetPathToRecordedData();
            StartCoroutine(PeriodicallyWriteData());
        }

        /// <summary>
        /// Sets the path to the recorded data file by generating a unique filename 
        /// with the current date and time, appending the ".jsonl.txt" extension,
        /// and combining it with the base path. Calling this again will force data being
        /// written to a different file. Returns the file location to which data is written.
        /// The file be in the location will be:
        /// `<see cref="FileSaveLocationBase"/>/<see cref="PathToDataFile"/>/<see cref="FileNameBase"/>_< date > < time >.jsonl.txt`
        /// 
        /// This method needs to invoked after setting the values of
        /// <see cref="FileSaveLocationBase"/>, <see cref="PathToDataFile"/>
        /// or <see cref="FileNameBase"/> for them to take effect.
        ///
        /// <seealso cref="PathToRecordedData"/>
        /// The file written will be a jsonl file. It is saved as .jsonl.txt as unity doesn't
        /// support jsonl as a TextAsset format.
        /// </summary>
        public string SetPathToRecordedData()
        {
            string basePath = FileSaveLocationBase switch
            {
                SubjectDataManager.FileSaveLocation.dataPath => Application.dataPath,
                SubjectDataManager.FileSaveLocation.persistentDataPath => Application.persistentDataPath,
                SubjectDataManager.FileSaveLocation.fixedPath => FixedFileSaveLocationBase,
                _ => throw new System.Exception($"Unkown value for FileSaveLocation")
            };

            pathBaseToRecordedData = Path.Combine(
                basePath,
            pathToDataFile);
            if (!Directory.Exists(pathBaseToRecordedData))
            {
                Directory.CreateDirectory(pathBaseToRecordedData);
            }
            // KLUDGE: jsonl is not supported as a TextAsset in unity.
            // Hence appending the .txt so that it can be detected as a TextAsset.
            string fileName = fileNameBase + "_" + DateTime.Now.ToString("dd-MM-yy hh-mm-ss") + ".jsonl.txt";
            pathToRecordedData = Path.Combine(pathBaseToRecordedData, fileName);
            Debug.Log($"Setting data file path to: {pathToRecordedData}");
            return pathToRecordedData;
        }

        private IEnumerator PeriodicallyWriteData()
        {
            while(true)
            {
                yield return new WaitForSeconds(5);

                if (EnableWriteData || forceWrite)
                {
                    forceWrite = false;
                    Dictionary<string, Dictionary<string, ViconStreamData>> dataCopy;
                    lock (dataLock)
                    {
                        dataCopy = dataToWrite;
                        dataToWrite = new Dictionary<string, Dictionary<string, ViconStreamData>>();
                    }
                    task = Task.Run(() =>
                    {
                        WriteToFile(pathToRecordedData, dataCopy);
                    });
                }
            }
        }

        private void WriteToFile(string pathToRecordedData, Dictionary<string, Dictionary<string, ViconStreamData>> data)
        {
            using (StreamWriter stream = new StreamWriter(pathToRecordedData, append: true))
            {
                string jsonlData = JsonConvert.SerializeObject(data);
                stream.WriteLine(jsonlData);
                Debug.Log($"Appending to {pathBaseToRecordedData}");
            }
        }

        /// <inheritdoc />
        private void OnEnable()
        {
            MaybeSetupConnection();
            // FIXME: Prevent loading from slowing down. Better caching?
            LoadRecordedJsonl();
        }

        /// <inheritdoc />
        private void FixedUpdate()
        {
            if (streamType == StreamType.Recorded)
            {
                StreamRecordedData();
                return;
            }
            webSocket?.DispatchLatestMessage();
        }

        /// <inheritdoc />
        private void OnDisable()
        {
            if (webSocket != null)
            {
                webSocket.OnMessage -= StreamData;
            }
            MaybeDisableConnection();
        }

        /// <inheritdoc />
        private void OnValidate()
        {
            ProcessDefaultDataAndWebSocket();
        }

        /// <inheritdoc />
        private void OnDestroy()
        {
            if (EnableWriteData)
            {
                StopAllCoroutines();
                if (task != null)
                {
                    task.Wait();
                }
                Debug.Log($"TODO WIRITINGGG222");
                WriteToFile(pathToRecordedData, dataToWrite);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Ensure that connection is turned off when using default data and vice versa.
        /// To be called when default data is changed.
        /// </summary>
        private void ProcessDefaultDataAndWebSocket()
        {
            if (streamType != StreamType.LiveStream)
            {
                MaybeDisableConnection();
            }
            else
            {
                MaybeSetupConnection();
            }
        }

        /// <summary>
        /// Load all data from the <see cref="JsonlFilesToLoad"/> list.
        /// </summary>
        public int LoadRecordedJsonl()
        {
            if (streamType != StreamType.Recorded)
            {
                return -1;
            }

            Debug.Log($"Loading Recorded Data");

            recordedData.Clear();

            foreach (TextAsset jsonlFileToLoad in jsonlFilesToLoad.Distinct())
            {
                if (jsonlFileToLoad == null)
                {
                    continue;
                }
                Debug.Log($"Now Loading {jsonlFileToLoad.name}");
                using (StringReader reader = new StringReader(jsonlFileToLoad.text))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Dictionary<string, Dictionary<string, ViconStreamData>> temp = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ViconStreamData>>>(line);
                        recordedData = recordedData.Concat(temp).ToDictionary(k => k.Key, v => v.Value);
                    }
                }
            }
            totalFrames = recordedData.Count;
            return totalFrames;
        }

        /// <summary>
        /// Add a TextAsset to the list of jsonl file to load as recoded data.
        /// </summary>
        public void AddToJsonlFilesToLoad(TextAsset asset)
        {
            jsonlFilesToLoad.Add(asset);
            LoadRecordedJsonl();
        }

        /// <summary>
        /// Remove a TextAsset to the list of jsonl file to load as recoded data.
        /// </summary>
        public void RemoveFromJsonlFilesToLoad(TextAsset asset)
        {
            jsonlFilesToLoad.Remove(asset);
            LoadRecordedJsonl();
        }

        /// <summary>
        /// Clear the list of jsonl file to load as recoded data.
        /// </summary>
        public void ClearJsonlFilesToLoad()
        {
            jsonlFilesToLoad.Clear();
            LoadRecordedJsonl();
        }

        private void StreamRecordedData()
        {
            if (recordedData == null || recordedData.Count == 0)
            {
                return;
            }

            if (currentFrame >= totalFrames)
            {
                currentFrame = 0;
            }

            KeyValuePair<string, Dictionary<string, ViconStreamData>> currentFrameData = recordedData.ElementAt(currentFrame);
            foreach (KeyValuePair<string, ViconStreamData> subject in currentFrameData.Value)
            {
                if (subject.Value != null)
                {
                    data[subject.Key] = subject.Value;
                    rawData[subject.Key] = subject.Value.ToString();
                }
                else
                {
                    data[subject.Key] = null;
                    rawData[subject.Key] = null;
                    Debug.LogWarning($"Missing subject data in frame for `{subject}`");
                }
            }

            if (play)
            {
                currentFrame++;
            }
        }

        /// <summary>
        /// Setup websocket connection.
        /// </summary>
        private async void MaybeSetupConnection()
        {
            if (streamType != StreamType.LiveStream || subjectList.Count == 0 || (webSocket != null && (webSocket.State == WebSocketState.Connecting || webSocket.State == WebSocketState.Open)))
            {
                return;
            }

            if (webSocket == null)
            {
                webSocket = new WebSocket(BaseURI);
                webSocket.OnOpen += () =>
                {
                    Debug.Log("Connection open!");
                };

                webSocket.OnError += (e) =>
                {
                    Debug.Log("Error! " + e);
                };

                webSocket.OnClose += async (e) =>
                {
                    Debug.Log("Connection closed!");

                    if (subjectList.Count > 0)
                    {
                        // Retry in 1 seconds
                        await Task.Delay(TimeSpan.FromSeconds(1f));
                        Debug.Log("Trying to connect again");
                        MaybeSetupConnection();
                    }
                };
            }

            webSocket.OnMessage += StreamData;
            await webSocket.Connect();
        }

        /// <summary>
        /// Disable connection
        /// </summary>
        private async void MaybeDisableConnection()
        {
            if (webSocket != null && (webSocket.State != WebSocketState.Closing || webSocket.State != WebSocketState.Closed))
            {
                await webSocket.Close();
            }
        }

        /// <summary>
        /// Process the date from websocket. Is inteaded as callback for the <see cref="WebSocket.OnMessage"/>
        /// </summary>
        private void StreamData(byte[] receivedData)
        {
            JObject jsonObject = JObject.Parse(Encoding.UTF8.GetString(receivedData));
            long currentTicks = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            foreach (string subject in subjectList)
            {
                if (jsonObject.TryGetValue(subject, out JToken jsonDataObject))
                {
                    string rawJsonDataString = jsonDataObject.ToString();
                    data[subject] = JsonConvert.DeserializeObject<ViconStreamData>(rawJsonDataString);
                    rawData[subject] = rawJsonDataString;
                }
                else
                {
                    data[subject] = null;
                    rawData[subject] = null;
                    Debug.LogWarning($"Missing subject data in frame for `{subject}`");
                }
            }

            if (EnableWriteData)
            {
                lock(dataLock)
                {
                    dataToWrite[currentTicks.ToString()] = new(data);
                }
            }
        }

        /// <summary>
        /// Regsiter a subject to recieve subject data.
        /// </summary>
        public void RegisterSubject(string subjectName)
        {
            subjectList.Add(subjectName);
            MaybeSetupConnection();
        }

        /// <summary>
        /// Unregsiter a subject.
        /// </summary>
        public void UnRegisterSubject(string subjectName)
        {
            subjectList.Remove(subjectName);
        }
    }

    [Serializable]
    public enum StreamType
    {
        Default = 0,
        Recorded = 1,
        LiveStream = 2
    }
}
