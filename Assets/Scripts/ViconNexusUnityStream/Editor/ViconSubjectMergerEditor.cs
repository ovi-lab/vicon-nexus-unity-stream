// Place this in Assets/Editor or any Editor folder
using UnityEditor;
using UnityEngine;
using ubco.ovilab.ViconUnityStream.Utils;

[CustomEditor(typeof(ViconSubjectMerger))]
public class ViconSubjectMergerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ViconSubjectMerger merger = (ViconSubjectMerger)target;

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Merge Subject"))
            {
                merger.MergeSubject();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use the merge function.", MessageType.Info);
        }
    }
}