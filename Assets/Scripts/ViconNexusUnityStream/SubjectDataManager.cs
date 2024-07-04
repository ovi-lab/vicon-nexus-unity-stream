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
    [SerializeField] private ClientConfigArgs clientConfig;
    
    private List<string> subjectList = new();
    private WebSocket webSocket;
    private Dictionary<string, Data> data = new();
    private Dictionary<string, string> rawData = new();
    
    private bool isConnectionThreadRunning;
    private bool GetFrameThread = true;
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
    private void LateUpdate()
    {
        if(!GetFrameThread)
        {
            if (!isConnected)
            {
                return;
            }
            viconClient.GetNewFrame();
        }
        
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
    /// Setup Direct connection.
    /// </summary>
    private void MaybeSetupConnection()
    {
        if (UseDefaultData || subjectList.Count == 0)
        {
            return;
        }

        viconClient = clientConfig.isRetimed ? new RetimingClient() 
                                             : new Client();

        viconClient.ConfigureClient(clientConfig);
        connectThread = new Thread(ConnectClient);
        connectThread.Start();

    }

    private void ConnectClient()
    {
        isConnectionThreadRunning = true;

        isConnected = false;
        while (isConnectionThreadRunning && !viconClient.IsConnected().Connected)
        {
            viconClient.ConnectClient(baseURI);
            Thread.Sleep(200);
        }
        print($"Connected. Retiming Client:{clientConfig.useLightweightData}");

        if (clientConfig.useLightweightData)
        {
            Result result = viconClient.EnableLightweightSegmentData().Result;
            Debug.Log($"Lightweight Data Configuration: {result == Result.Success}");
        }
        
        viconClient.SetAxisMapping(Direction.Forward, Direction.Left, Direction.Up);
        ConnectionHandler( true);
        isConnectionThreadRunning = false;
    }

    /// <summary>
    /// Disable connection 
    /// </summary>
    private async void MaybeDisableConnection()
    {
        
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
