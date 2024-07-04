using System;
using UnityEngine;
using ViconDataStreamSDK.CSharp;

public interface IViconClient
{

    public void ConfigureClient(ClientConfigArgs args);
    public void ConnectClient(string baseURI);
    public Output_IsConnected IsConnected();
    public Output_EnableLightweightSegmentData EnableLightweightSegmentData();
    public Output_GetSegmentLocalRotationQuaternion GetSegmentLocalRotationQuaternion(string SubjectName, string SegmentName);
    public Output_GetSegmentLocalTranslation GetSegmentLocalTranslation(string SubjectName, string SegmentName);
    public Output_GetSegmentLocalTranslation GetScaledSegmentTranslation(string SubjectName, string SegmentName);
    public Output_GetSegmentStaticScale GetSegmentStaticScale(string SubjectName, string SegmentName);
    public Output_GetSubjectRootSegmentName GetSubjectRootSegmentName(string SubjectName);
    public Output_GetSegmentParentName GetSegmentParentName(string SubjectName, string SegmentName);
    public Output_SetAxisMapping SetAxisMapping(Direction X, Direction Y, Direction Z);
    public void GetNewFrame();
    public uint GetFrameNumber();
    public Output_Disconnect Disconnect();
}


[Serializable]
public class ClientConfigArgs
{
    [Header("Retimed Client Config")]
    public bool isRetimed = false;
    public float retimedOffset = 0;
    
    [Header("Normal Client Config")]
    public StreamMode clientStreamMode;
    public bool configureWireless = true;
    
    [Header("CommonConfig")]
    public bool useLightweightData;
}


[Serializable]
public class ClientConfigArgs
{
    [Header("Retimed Client Config")]
    public bool isRetimed = false;
    public float retimedOffset = 0;
    
    [Header("Normal Client Config")]
    public StreamMode clientStreamMode;
    public bool configureWireless = true;
    
    [Header("CommonConfig")]
    public bool useLightweightData;
}