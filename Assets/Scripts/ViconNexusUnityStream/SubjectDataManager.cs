using System.Collections.Generic;
using NativeWebSocket;
using ubco.ovilab.ViconUnityStream;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class SubjectDataManager : MonoBehaviour
{
    [Tooltip("The Webscoket URL used for connection.")]
    [SerializeField] private string baseURI = "ws://viconmx.hcilab.ok.ubc.ca:5001/markers/";
    /// <summary>
    /// The Webscoket URL used for connection.
    /// </summary>
    public string BaseURI { get => baseURI; set => baseURI = value; }

    [Tooltip("Should the subjects use default, recorded or live streamed data")] [SerializeField]
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

    [Tooltip("Path to write the subject data file.")]
    [SerializeField] private string pathToDataFile;
    public Dictionary<string, ViconStreamData> StreamedData => data;
    public Dictionary<string, string> StreamedRawData => rawData;

    private List<string> subjectList = new();
    private WebSocket webSocket;
    private Dictionary<string, ViconStreamData> data = new();
    private Dictionary<string, string> rawData = new();

    private Dictionary<string, Dictionary<string, ViconStreamData>> recordedData = new();
    private Dictionary<string, Dictionary<string, ViconStreamData>> dataToWrite = new();
    private string pathToRecordedData;
    [SerializeField] private int currentFrame = 0;
    private int totalFrames = 0;
    private string fileName = "Session";
    private List<string> recordedSessions = new List<string>();
    private void Awake()
    {
        pathToRecordedData = Path.Combine(Application.dataPath, pathToDataFile);
        if (!Directory.Exists(pathToRecordedData))
        {
            Directory.CreateDirectory(pathToRecordedData);
        }
    }

    /// <inheritdoc />
    private void OnEnable()
    {
        MaybeSetupConnection();
        LoadRecordedJson();
    }

    /// <inheritdoc />
    private void FixedUpdate()
    {
        if (streamType == StreamType.Recorded)
        {
            StreamLocalData();
            return;
        }
        webSocket?.DispatchLatestMessage();
    }

    /// <inheritdoc />
    private void OnDisable()
    {
        if (enableWriteData)
        {
            string jsonData = JsonConvert.SerializeObject(dataToWrite, Formatting.Indented);
            fileName = fileName + "_" + DateTime.Now.ToString("dd-MM-yy hh-mm-ss") + ".json";
            pathToRecordedData = Path.Combine(pathToRecordedData, fileName);
                File.AppendAllTextAsync(pathToRecordedData, jsonData);
        }
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

    private void LoadRecordedJson()
    {
        if(streamType != StreamType.Recorded) return;

        Debug.Log($"Loading Recorded Data");

        foreach (string session in Directory.GetFiles(pathToRecordedData, "*.json"))
        {
            string jsonData = Path.Combine(pathToRecordedData, session);
            string json = File.ReadAllText(jsonData);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"No recorded data found at {pathToRecordedData}.");
                continue;
            }
            Debug.Log($"Now Loading {jsonData}");
            JObject recordedJson = JObject.Parse(json);
            Dictionary<string, Dictionary<string, ViconStreamData>> temp = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ViconStreamData>>>(recordedJson.ToString());
            recordedData = recordedData.Concat(temp).ToDictionary(k => k.Key, v => v.Value);
        }
        totalFrames = recordedData.Count;
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

    private void StreamLocalData()
    {
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
        currentFrame++;
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