using System.Collections.Generic;
using NativeWebSocket;
using ubco.ovilab.ViconUnityStream;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Serialization;
using ViconDataStreamSDK.CSharp;
public class SubjectDataManager : MonoBehaviour
{
    [Tooltip("The hostname or ip address of the DataStream server.")]
    [SerializeField] private string baseURI = "viconmx.hcilab.ok.ubc.ca:801";
    /// <summary>
    /// The URL used for connection.
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

    public Dictionary<string, Data> StreamedData => data;
    public Dictionary<string, string> StreamedRawData => rawData;
    
    [Header("Client Config")]
    [SerializeField] private bool isRetimed = false;
    [SerializeField] private bool useLightweightData;
    [SerializeField] private StreamMode clientStreamMode;
    [SerializeField] private bool configureWireless = true;
    
    private List<string> subjectList = new();
    private WebSocket webSocket;
    private Dictionary<string, Data> data = new();
    private Dictionary<string, string> rawData = new();
    private bool isConnectionThreadRunning;
    private static bool isConnected;
    
    private IViconClient viconClient;
    private RetimingClient viconRetimingClient;
    private Thread connectThread;
    
    public delegate void ConnectionCallback(bool i_bConnected);
    public static void OnConnected(bool i_bConnected)
    {
        isConnected = i_bConnected;
    }

    ConnectionCallback ConnectionHandler = OnConnected;
    
    /// <inheritdoc />
    private void OnEnable()
    {
        MaybeSetupConnection();
    }

    /// <inheritdoc />
    private void FixedUpdate()
    {
        
    }

    /// <inheritdoc />
    private void OnDisable()
    {
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
    private void MaybeSetupConnection()
    {
        if (UseDefaultData || subjectList.Count == 0)
        {
            return;
        }

        if (isRetimed)
        {
            viconClient = new RetimingClient();
        }
        else
        {
            viconClient = new Client();
        }
        
        viconClient.ConfigureClient();

        connectThread = new Thread(ConnectClient);
        connectThread.Start();

    }

    private void ConnectClient()
    {
        isConnectionThreadRunning = true;

        bool isConnected = false;
        while (isConnectionThreadRunning && !viconClient.IsConnected().Connected)
        {
            viconClient.ConnectClient(baseURI);
            Thread.Sleep(200);
        }
        print($"Connected. Retiming Client:{isRetimed}");

        if (useLightweightData)
        {
            var result = viconClient.EnableLightweightSegmentData().Result;
            Debug.Log($"Lightweight Data Configuration: {result == Result.Success}");
        }
        
        viconClient.SetAxisMapping(Direction.Forward, Direction.Left, Direction.Up);
        ConnectionHandler( true );
        isConnectionThreadRunning = false;
        return;
        
        if (isRetimed)
        {
            

            if (useLightweightData)
            {
                var isLightWeightData = viconRetimingClient.EnableLightweightSegmentData().Result;
                Debug.Log($"Attempt to Use lightweightData: {isLightWeightData}");
            }

            viconRetimingClient.SetAxisMapping(Direction.Forward, Direction.Left, Direction.Up);
            ConnectionHandler( true);
            isConnectionThreadRunning = false;
            return;
        }
        else
        {
            while (isConnectionThreadRunning && !viconClient.IsConnected().Connected)
            {
                Output_Connect OC = viconRetimingClient.Connect(baseURI);
                Debug.LogWarning("Attempt to Connect: " + OC.Result);
                Thread.Sleep(200);
            }

            viconClient.SetStreamMode(clientStreamMode);
            viconClient.GetFrame();

            if (useLightweightData)
            {
                var isLightWeightData = viconClient.EnableLightweightSegmentData().Result;
                Debug.Log($"Attempt to Use lightweightData: {isLightWeightData}");
            }
            else
            {
                viconClient.EnableSegmentData();
            }

            viconClient.SetAxisMapping(Direction.Forward, Direction.Left, Direction.Up);
            ConnectionHandler( true);
        }
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
