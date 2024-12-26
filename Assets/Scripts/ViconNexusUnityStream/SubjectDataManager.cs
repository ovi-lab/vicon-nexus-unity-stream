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

namespace ubco.ovilab.ViconUnityStream
{
    public class SubjectDataManager : MonoBehaviour
    {
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
                LoadRecordedJson();
            }
        }

        [Tooltip("Enable writing data to disk.")]
        [SerializeField] private bool enableWriteData = false;
        /// <summary>
        /// Enable writing data to disk.
        /// </summary>
        public bool EnableWriteData { get => enableWriteData; set => enableWriteData = value; }

        [Tooltip("Path to write the subject data file. If in editor, will be relative to the Assets folder. In player, will use persistentDataPath.")]
        [SerializeField] private string pathToDataFile;
        [SerializeField, Tooltip("The file saved would have this prifix + date string")]
        private string fileNameBase = "Session";

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

        [SerializeField, Tooltip("The json file to load when using StreamType.Recorded")]
        private List<TextAsset> jsonFilesToLoad;

        /// <summary>
        /// The json file to load when using StreamType.Recorded.
        /// </summary>
        /// <seealso cref="AddToJsonFilesToLoad"/>
        /// <seealso cref="RemoveFromJsonFilesToLoad"/>
        /// <seealso cref="ClearJsonFilesToLoad"/>
        public ReadOnlyCollection<TextAsset> JsonFilesToLoad { get => jsonFilesToLoad.AsReadOnly(); }

        /// <summary>
        /// Deserialized data recieved by the data manager.
        /// </summary>
        public Dictionary<string, ViconStreamData> StreamedData => data;

        /// <summary>
        /// The raw data being recieved by data manager.
        /// </summary>
        public Dictionary<string, string> StreamedRawData => rawData;

        private List<string> subjectList = new();
        private WebSocket webSocket;
        private string pathToRecordedData;
        private Dictionary<string, ViconStreamData> data = new();
        private Dictionary<string, string> rawData = new();
        private Dictionary<string, Dictionary<string, ViconStreamData>> recordedData = new();
        private Dictionary<string, Dictionary<string, ViconStreamData>> dataToWrite = new();

        private void Awake()
        {
            pathToRecordedData = Path.Combine(
#if UNITY_EDITOR
                Application.dataPath,
#else
                Application.persistentDataPath,
#endif
                pathToDataFile);
            if (!Directory.Exists(pathToRecordedData))
            {
                Directory.CreateDirectory(pathToRecordedData);
            }
        }

        /// <inheritdoc />
        private void OnEnable()
        {
            MaybeSetupConnection();
            // FIXME: Prevent loading from slowing down. Better caching?
            LoadRecordedJson();
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
        private void OnDestory()
        {
            // TODO: Ensure not running into memory issues.
            if (enableWriteData)
            {
                string jsonData = JsonConvert.SerializeObject(dataToWrite, Formatting.Indented);
                fileNameBase = fileNameBase + "_" + DateTime.Now.ToString("dd-MM-yy hh-mm-ss") + ".json";
                pathToRecordedData = Path.Combine(pathToRecordedData, fileNameBase);
                File.AppendAllTextAsync(pathToRecordedData, jsonData);
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
        /// Load all data from the <see cref="JsonFilesToLoad"/> list.
        /// </summary>
        public int LoadRecordedJson()
        {
            if (streamType != StreamType.Recorded)
            {
                return -1;
            }

            Debug.Log($"Loading Recorded Data");

            recordedData.Clear();

            foreach (TextAsset jsonFileToLoad in jsonFilesToLoad)
            {
                if (jsonFileToLoad == null)
                {
                    continue;
                }
                Debug.Log($"Now Loading {jsonFileToLoad.name}");
                Dictionary<string, Dictionary<string, ViconStreamData>> temp = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ViconStreamData>>>(jsonFileToLoad.text);
                recordedData = recordedData.Concat(temp).ToDictionary(k => k.Key, v => v.Value);
            }
            totalFrames = recordedData.Count;
            return totalFrames;
        }

        /// <summary>
        /// Add a TextAsset to the list of json file to load as recoded data.
        /// </summary>
        public void AddToJsonFilesToLoad(TextAsset asset)
        {
            jsonFilesToLoad.Add(asset);
            LoadRecordedJson();
        }

        /// <summary>
        /// Remove a TextAsset to the list of json file to load as recoded data.
        /// </summary>
        public void RemoveFromJsonFilesToLoad(TextAsset asset)
        {
            jsonFilesToLoad.Remove(asset);
            LoadRecordedJson();
        }

        /// <summary>
        /// Clear the list of json file to load as recoded data.
        /// </summary>
        public void ClearJsonFilesToLoad()
        {
            jsonFilesToLoad.Clear();
            LoadRecordedJson();
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
                data[subject.Key] = subject.Value;
                rawData[subject.Key] = subject.Value.ToString();
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

            if (enableWriteData)
            {
                dataToWrite[currentTicks.ToString()] = new(data);
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
