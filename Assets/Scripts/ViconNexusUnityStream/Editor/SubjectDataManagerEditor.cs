using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ubco.ovilab.ViconUnityStream.Editor
{
    [CustomEditor(typeof(SubjectDataManager), true)]
    [CanEditMultipleObjects]
    public class SubjectDataManagerEditor: UnityEditor.Editor
    {
        private List<CustomSubjectScript> subjectScripts = new();
        private SubjectDataManager subjectDataManager;

        private SerializedProperty scriptProperty, totalFramesProperty, currentFrameProperty,
            playProperty, pathToDataFileProperty, jsonFilesToLoadProperty, enableWriteDataProperty,
            fileNameBaseProperty, baseURIProperty, streamTypeProperty;

        private void OnEnable()
        {
            subjectScripts.AddRange(FindObjectsByType<CustomSubjectScript>(FindObjectsSortMode.None));

            subjectDataManager = target as SubjectDataManager;

            scriptProperty = serializedObject.FindProperty("m_Script");
            baseURIProperty = serializedObject.FindProperty("baseURI");
            streamTypeProperty = serializedObject.FindProperty("streamType");
            pathToDataFileProperty = serializedObject.FindProperty("pathToDataFile");
            currentFrameProperty = serializedObject.FindProperty("currentFrame");
            totalFramesProperty = serializedObject.FindProperty("totalFrames");
            jsonFilesToLoadProperty = serializedObject.FindProperty("jsonFilesToLoad");
            enableWriteDataProperty = serializedObject.FindProperty("enableWriteData");
            fileNameBaseProperty = serializedObject.FindProperty("fileNameBase");
            playProperty = serializedObject.FindProperty("play");
        }

        private void OnDisable()
        {
            subjectScripts = null;
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(scriptProperty);
            GUI.enabled = true;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Subjects in scene", EditorStyles.boldLabel);
            foreach (CustomSubjectScript subjectScript in subjectScripts)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{subjectScript.transform.name} ({subjectScript.SubejectName})");
                if (GUILayout.Button("Goto subject"))
                {
                    Selection.activeObject = subjectScript;
                    InternalEditorUtility.SetIsInspectorExpanded(subjectScript, true);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            CustomSubjectConfig.instance.Enabled = EditorGUILayout.Toggle("Subjects enabled", CustomSubjectConfig.instance.Enabled);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateContent();
            }
            EditorGUILayout.PropertyField(baseURIProperty);
            using(var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(streamTypeProperty);
                if (check.changed && Application.isPlaying)
                {
                    serializedObject.ApplyModifiedProperties();
                    subjectDataManager.LoadRecordedJson();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Controls for recording LiveStream data", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(enableWriteDataProperty);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(pathToDataFileProperty);
            if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
            {
                pathToDataFileProperty.stringValue = Path.GetRelativePath(Application.dataPath, EditorUtility.OpenFolderPanel("Location to save live streamed data", "", ""));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(fileNameBaseProperty);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Contols for playing recorded data", EditorStyles.boldLabel);
            int totalFrames = totalFramesProperty.intValue;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(jsonFilesToLoadProperty);
            if (EditorGUI.EndChangeCheck())
            {
                totalFrames = totalFramesProperty.intValue = subjectDataManager.LoadRecordedJson();
            }
            EditorGUILayout.IntSlider(currentFrameProperty, 0, totalFrames, $"Current Frame (out of {totalFrames})");
            EditorGUILayout.PropertyField(playProperty);

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateContent()
        {
            CustomSubjectConfig.instance.Save();
            foreach (CustomSubjectScript script in subjectScripts)
            {
                script.enabled = CustomSubjectConfig.instance.Enabled;

                EditorUtility.SetDirty(script);
            }
            Debug.Log($"Updated {subjectScripts.Count} subject script(s):"+
                      $"\n    Enabled:            {CustomSubjectConfig.instance.Enabled};"+
                      $"\n    StreamType: {subjectDataManager.StreamType.ToString()};"+
                      $"\n    Writing data:       {subjectDataManager.EnableWriteData};"+
                      $"\n    Scripts updated: \n         " +
                      string.Join(",\n         ", subjectScripts));
        }
    }
}
