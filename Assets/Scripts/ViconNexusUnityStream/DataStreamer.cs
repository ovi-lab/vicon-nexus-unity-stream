using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NativeWebSocket;
using ubco.ovilab.ViconUnityStream;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using MessagePack;
using MessagePack.Resolvers;
using Newtonsoft.Json.Linq;
using UnityEngine.Serialization;

public class DataStreamer : Singleton<DataStreamer>
{
    [SerializeField] private string baseURI = "ws://viconmx.hcilab.ok.ubc.ca:5001/";
    public List<string> subjectList;
    public WebSocket signalStream => webSocket;
    public Dictionary<string, Data> StreamedData => data;
    public Dictionary<string, string> StreamedRawData => rawData;
    
    private WebSocket webSocket;
    private Dictionary<string, Data> data = new();
    private Dictionary<string, string> rawData = new();
    private async void Start()
    {
        var customSubjects = FindObjectsOfType<CustomSubjectScript>();
        foreach (CustomSubjectScript customSubject in customSubjects)
        {
            subjectList.Add(customSubject.subjectName);
        }
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
            webSocket.Close();
            Debug.Log("Connection closed!");
        };

        webSocket.OnMessage += StreamData;
        await webSocket.Connect();
    }

    private void StreamData(byte[] receivedData)
    {
        JObject jsonObject = JObject.Parse(Encoding.UTF8.GetString(receivedData));
        foreach (string subject in subjectList)
        {
            data[subject] = JsonConvert.DeserializeObject<Data>(jsonObject[subject]!.ToString());
            rawData[subject] = JsonConvert.SerializeObject(data);
        }
    }
    
    private void FixedUpdate()
    {
        webSocket.DispatchLatestMessage();
    }

    private void OnDisable()
    {
        webSocket.OnMessage -= StreamData;
    }
}
