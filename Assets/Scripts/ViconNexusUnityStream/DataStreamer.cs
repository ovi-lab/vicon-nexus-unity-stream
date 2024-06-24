using System.Collections.Generic;
using NativeWebSocket;
using ubco.ovilab.ViconUnityStream;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class DataStreamer : Singleton<DataStreamer>
{
    [SerializeField] private string baseURI = "ws://viconmx.hcilab.ok.ubc.ca:5001/";
    public Dictionary<string, Data> StreamedData => data;
    public Dictionary<string, string> StreamedRawData => rawData;
    
    private List<string> subjectList;
    private WebSocket webSocket;
    private Dictionary<string, Data> data = new();
    private Dictionary<string, string> rawData = new();

    /// <inheritdoc />
    private void OnEnable()
    {
        SetupConnection();
    }

    /// <inheritdoc />
    private void FixedUpdate()
    {
        webSocket.DispatchLatestMessage();
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
        if (webSocket.State == WebSocketState.Connecting || webSocket.State == WebSocketState.Open)
        {
            return;
        }

        if (webSocket == null)
        {
            webSocket = new WebSocket(baseURI);
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
