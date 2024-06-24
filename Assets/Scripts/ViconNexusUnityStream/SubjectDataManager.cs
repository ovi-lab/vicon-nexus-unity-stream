using System.Collections.Generic;
using NativeWebSocket;
using ubco.ovilab.ViconUnityStream;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
    public bool UseDefaultData { get => useDefaultData; set => useDefaultData = value; }

    [Tooltip("Enable writing data to disk.")]
    [SerializeField] private bool enableWriteData = false;
    /// <summary>
    /// Enable writing data to disk.
    /// </summary>
    public bool EnableWriteData { get => enableWriteData; set => enableWriteData = value; }

    [Tooltip("Ensure XR Hands Subsystem data is provided by approproiate subjects.")]
    [SerializeField] private bool useXRHandSubsystem = false;
    /// <summary>
    /// Ensure XR Hands Subsystem data is provided by approproiate subjects.
    /// </summary>
    public bool UseXRHandSubsystem { get => useXRHandSubsystem; set => useXRHandSubsystem = value; }

    public Dictionary<string, Data> StreamedData => data;
    public Dictionary<string, string> StreamedRawData => rawData;

    private List<string> subjectList = new();
    private WebSocket webSocket;
    private Dictionary<string, Data> data = new();
    private Dictionary<string, string> rawData = new();

    /// <inheritdoc />
    private void OnEnable()
    {
        if (subjectList.Count > 0)
        {
            SetupConnection();
        }
    }

    /// <inheritdoc />
    private void FixedUpdate()
    {
        webSocket?.DispatchLatestMessage();
    }

    /// <inheritdoc />
    private async void OnDisable()
    {
        webSocket.OnMessage -= StreamData;
        await webSocket.Close();
    }

    /// <summary>
    /// Setup websocket connection.
    /// </summary>
    private async void SetupConnection()
    {
        if (webSocket != null)
        {
            if (webSocket.State == WebSocketState.Connecting || webSocket.State == WebSocketState.Open)
            {
                return;
            }
        }
        else
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

            webSocket.OnClose += (e) =>
            {
                Debug.Log("Connection closed!");
            };
        }

        webSocket.OnMessage += StreamData;
        await webSocket.Connect();
    }


    /// <summary>
    /// Process the date from websocket. Is inteaded as callback for the <see cref="WebSocket.OnMessage"/>
    /// </summary>
    private void StreamData(byte[] receivedData)
    {
        JObject jsonObject = JObject.Parse(Encoding.UTF8.GetString(receivedData));
        foreach (string subject in subjectList)
        {
            data[subject] = JsonConvert.DeserializeObject<Data>(jsonObject[subject]!.ToString());
            rawData[subject] = JsonConvert.SerializeObject(data);
        }
    }

    /// <summary>
    /// Regsiter a subject to recieve subject data.
    /// </summary>
    public void RegisterSubject(string subjectName)
    {
        subjectList.Add(subjectName);
        if (webSocket == null || webSocket.State == WebSocketState.Closed || webSocket.State == WebSocketState.Closing)
        {
            SetupConnection();
        }
    }

    /// <summary>
    /// Unregsiter a subject.
    /// </summary>
    public void UnRegsiterSubject(string subjectName)
    {
        subjectList.Remove(subjectName);
    }
}
