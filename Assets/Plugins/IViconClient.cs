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
    public Output_GetMarkerName GetMarkerName(string SubjectName, uint MarkerIndex);
    public Output_GetSegmentName GetSegmentName(string SubjectName, uint SegmentIndex);
    public Output_GetSegmentParentName GetSegmentParentName(string SubjectName, string SegmentName);
    public Output_GetSegmentChildName GetSegmentChildName(string SubjectName, string SegmentName, uint SegmentIndex);
    public Output_GetSegmentChildCount GetSegmentChildCount(string SubjectName, string SegmentName);
    public Output_GetMarkerCount GetMarkerCount(string SubjectName);
    public Output_GetMarkerGlobalTranslation GetMarkerGlobalTranslation(string SubjectName, string MarkerName);
    public Output_SetAxisMapping SetAxisMapping(Direction X, Direction Y, Direction Z);
    public Output_IsMarkerDataEnabled IsMarkerDataEnabled();
    public Output_EnableMarkerData EnableMarkerData();
    public void GetNewFrame();
    public uint GetFrameNumber();
    public Output_Disconnect Disconnect();
}


[Serializable]
public class ClientConfigArgs
{
    [Header("Client Config")]
    public bool useLightweightData;
    public bool configureWireless = true;
    public StreamMode clientStreamMode;

}

//Temporary Fix -- Assembly definition issue 

public enum GapFillingStrategy
{
    UseRemote = 0,
    Ignore = 1,
    UsePrevious = 2,
    FillRelative = 3,
    ReTimed = 4
}