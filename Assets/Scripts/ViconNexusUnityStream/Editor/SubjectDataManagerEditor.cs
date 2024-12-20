using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ubco.ovilab.ViconUnityStream.Editor
{
    [CustomEditor(typeof(SubjectDataManager), true)]
    [CanEditMultipleObjects]
    public class SubjectDataManagerEditor: UnityEditor.Editor
    {
        private static readonly string[] excludedSerializedNames = new string[]{"m_Script", "pathToDataFile", "totalFrames", "currentFrame", "play"};
        private List<CustomSubjectScript> subjectScripts = new();
        private SubjectDataManager subjectDataManager;

        private SerializedProperty scriptProperty, totalFramesProperty, currentFrameProperty, playProperty, pathToDataFileProperty;

        private void OnEnable()
        {
            subjectScripts.AddRange(FindObjectsByType<CustomSubjectScript>(FindObjectsSortMode.None));

            subjectDataManager = target as SubjectDataManager;

            scriptProperty = serializedObject.FindProperty("m_Script");
            pathToDataFileProperty = serializedObject.FindProperty("pathToDataFile");
            currentFrameProperty = serializedObject.FindProperty("currentFrame");
            totalFramesProperty = serializedObject.FindProperty("totalFrames");
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

            DrawPropertiesExcluding(serializedObject, excludedSerializedNames);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Recorded data controls", EditorStyles.boldLabel);
            int totalFrames = totalFramesProperty.intValue;
            GUI.enabled = subjectDataManager.StreamType == StreamType.Recorded;
            EditorGUILayout.PropertyField(pathToDataFileProperty);
            EditorGUILayout.IntSlider(currentFrameProperty, 0, totalFrames);
            EditorGUILayout.PropertyField(playProperty);
            GUI.enabled = true;

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
