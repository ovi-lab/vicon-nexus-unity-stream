using System.Collections.Generic;
using NativeWebSocket;
using ubco.ovilab.ViconUnityStream;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

public class SubjectDataManager : MonoBehaviour
{
    [Tooltip("The Webscoket URL used for connection.")]
    [SerializeField] private string baseURI = "ws://viconmx.hcilab.ok.ubc.ca:5001/markers/";
    /// <summary>
    /// The Webscoket URL used for connection.
    /// </summary>
    public string BaseURI { get => baseURI; set => baseURI = value; }

    [Tooltip("Should the subjects use the default data?")]
    [SerializeField] private bool useDefaultData = false;
    /// <summary>
    /// Should the subjects use the default data?
    /// </summary>
    public bool UseDefaultData
    {
        get => useDefaultData;
        set
        {
            useDefaultData = value;
            ProcessDefaultDataAndWebSocket();
        }
    }

    [Tooltip("Enable writing data to disk.")]
    [SerializeField] private bool enableWriteData = false;
    /// <summary>
    /// Enable writing data to disk.
    /// </summary>
    public bool EnableWriteData { get => enableWriteData; set => enableWriteData = value; }

    public Dictionary<string, ViconStreamData> StreamedData => data;
    public Dictionary<string, string> StreamedRawData => rawData;

    private List<string> subjectList = new List<string>();
    private WebSocket webSocket;
    private Dictionary<string, ViconStreamData> data = new Dictionary<string, ViconStreamData>();
    private Dictionary<string, string> rawData = new Dictionary<string, string>();

    /// <inheritdoc />
    private void OnEnable()
    {
        MaybeSetupConnection();
    }

    /// <inheritdoc />
    private void FixedUpdate()
    {
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

    /// <summary>
    /// Ensure that connection is turned off when using default data and vice versa.
    /// To be called when default data is changed.
    /// </summary>
    private void ProcessDefaultDataAndWebSocket()
    {
        if (UseDefaultData)
        {
            MaybeDisableConnection();
        }
        else
        {
            MaybeSetupConnection();
        }
    }

    /// <summary>
    /// Setup websocket connection.
    /// </summary>
    private async void MaybeSetupConnection()
    {
        if (UseDefaultData || subjectList.Count == 0 || (webSocket != null && (webSocket.State == WebSocketState.Connecting || webSocket.State == WebSocketState.Open)))
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
    public void UnRegsiterSubject(string subjectName)
    {
        subjectList.Remove(subjectName);
    }
}
