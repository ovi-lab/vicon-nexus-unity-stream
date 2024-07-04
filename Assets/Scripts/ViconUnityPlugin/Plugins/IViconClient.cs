using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViconDataStreamSDK.CSharp;

public interface IViconClient
{

    public void ConfigureClient();
    public bool ConnectClient(string baseURI);

    public Output_IsConnected IsConnected();
    public Output_EnableLightweightSegmentData EnableLightweightSegmentData()
    public Output_GetSegmentLocalRotationQuaternion GetSegmentRotation(string SubjectName, string SegmentName);
    public Output_GetSegmentLocalTranslation GetSegmentTranslation(string SubjectName, string SegmentName);
    public Output_GetSegmentStaticScale GetSegmentScale(string SubjectName, string SegmentName);
    public Output_GetSegmentLocalTranslation GetScaledSegmentTranslation(string SubjectName, string SegmentName);
    public Output_GetSubjectRootSegmentName GetSubjectRootSegmentName(string SubjectName);
    public Output_GetSegmentParentName GetSegmentParentName(string SubjectName, string SegmentName);
    public Output_SetAxisMapping SetAxisMapping(Direction X, Direction Y, Direction Z);
    public void GetNewFrame();
    public uint GetFrameNumber();
    public void UpdateSubjectFilter();
    

}
