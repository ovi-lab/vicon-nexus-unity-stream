using System;
using System.Collections.Generic;
using System.Linq;
using ubco.ovilab.ViconUnityStream;
using UnityEngine;
using System.Threading;
using HMDUtils;
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
    public Dictionary<string, Data> StreamedData => streamedData;
    public Dictionary<string, string> StreamedRawData => rawData;
    public IViconClient ViconClient => viconClient;

    [SerializeField] private ClientConfigArgs clientConfig;
    
    private List<string> subjectList = new();
    private Dictionary<string, Data> streamedData = new();
    private Dictionary<string, string> rawData = new();
    
    private bool isConnectionThreadRunning;
    private static bool isConnected;
    
    private Client viconClient;
    private Thread connectThread;
    private FusionService coordinateUtils;
    public List<string> markerNames;
    
    //Not sure if a callback is needed -- Copied over from sample code
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
    private void Update()
    {
        if (!isConnected)
            return;
        viconClient.GetNewFrame();
        
        foreach (string subject in subjectList)
        {
            Output_GetMarkerCount markerCount = viconClient.GetMarkerCount(subject);
            //Debug.Log(markerCount.Translation[0]);
            if (markerCount.Result != Result.Success)
            {
                Debug.LogWarning($"Could not Get {subject}'s data this frame");
                return;
            }
            
            Data viconPositionData = new()
            {
                position = ProcessData(subject, markerCount.MarkerCount)
            };
            if (!streamedData.TryAdd(subject, viconPositionData))
            {
                streamedData[subject] = viconPositionData;
            }
            //Debug.Log(streamedData[subject].position["base1"][0]);
            
        }
    }

    /// <inheritdoc />
    private void OnDisable()
    {
        if (isConnectionThreadRunning)
        {
            isConnectionThreadRunning = false;
            connectThread.Join();
        }
    }

    private void OnDestroy()
    {
        MaybeDisableConnection();
        viconClient = null;
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
        if (UseDefaultData || subjectList.Count == 0 || isConnectionThreadRunning)
        {
            return;
        }

        viconClient = new Client();
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
        viconClient.EnableMarkerData();
        //Debug.Log("Marker Data Status: " + viconClient.IsMarkerDataEnabled().Enabled);
        isConnectionThreadRunning = false;
    }

    private Dictionary<string, List<float>> ProcessData(string subject, uint markerCount)
    {
        Dictionary<string, List<float>> markerPositionsDict = new();
        for(uint i = 0; i < markerCount; i++)
        {
            string markerName = viconClient.GetMarkerName(subject, i).MarkerName;
             var globalTranslation = viconClient.GetMarkerGlobalTranslation(subject, markerName);
             // if (globalTranslation.Result != Result.Success)
             // {
             //     Debug.LogWarning("Failed");
             // }
            markerPositionsDict[markerName] = new List<float>()
            {
                (float)globalTranslation.Translation[0],
                (float)globalTranslation.Translation[1],
                (float)globalTranslation.Translation[2]
            };
            //Debug.Log($"{markerName}: {globalTranslation.Translation[0]}");
        }
        
        return markerPositionsDict;
    }
    
    private void ProcessData(string subject, string segmentName)
    {
        double[] translationData = viconClient.GetSegmentLocalTranslation(subject, segmentName).Translation;
        double[] orientationData = viconClient.GetSegmentLocalRotationQuaternion(subject, segmentName).Rotation;
        FusionService.Quat viconOrientation = new(orientationData[0], orientationData[1], orientationData[2], orientationData[3]);
        FusionService.Vec viconTranslation = new(translationData[0], translationData[1], translationData[2]);
        FusionService.Pose posData = FusionService.GetMappedVicon(viconOrientation, viconTranslation);
        List<float> positionData = new()
            {(float)posData.Position.X, (float)posData.Position.Y, (float)posData.Position.Z};
        Output_GetSegmentChildCount childCount = viconClient.GetSegmentChildCount(subject, segmentName);
        streamedData[subject].position[segmentName] = positionData;
        if(childCount.SegmentCount <= 0) return;
        for (uint i = 0; i < childCount.SegmentCount; i++)
        {
            Output_GetSegmentChildName childSegment = viconClient.GetSegmentChildName(subject, segmentName, i);
            ProcessData(subject, childSegment.SegmentName);
        }
    }

    private void ProcessChildData(string subject, string segmentName)
    {
        
        
        coordinateUtils.Create();
        //HMDUtils.FusionService.Pose ViconInHMD = FusionService.GetMappedVicon(ViconOrientation, translationData);
        
        
       
    }

    /// <summary>
    /// Disable connection 
    /// </summary>
    private void MaybeDisableConnection()
    {
        if(viconClient == null) return;
        Result disconnectionStatus = viconClient.Disconnect().Result;
        Debug.Log($"Disconnected:{disconnectionStatus}");
    }
    
    
    /// <summary>
    /// Register a subject to receive subject data.
    /// </summary>
    public void RegisterSubject(string subjectName)
    {
        subjectList.Add(subjectName);
        MaybeSetupConnection();
    }

    /// <summary>
    /// Unregister a subject.
    /// </summary>
    public void UnRegisterSubject(string subjectName)
    {
        subjectList.Remove(subjectName);
    }
}
