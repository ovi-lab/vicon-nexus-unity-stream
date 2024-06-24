using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ubco.ovilab.ViconUnityStream.Editor
{
    [CustomEditor(typeof(CustomSubjectScript), true)]
    [CanEditMultipleObjects]
    public class CustomSubjectScriptEditor: UnityEditor.Editor
    {
        private static readonly string[] excludedSerializedNames = new string[]{"m_Script", "subjectDataManager"};
        private CustomSubjectScript customSubjectScript;

        private SerializedProperty scriptProperty;
        private SerializedProperty subjectDataManagerProperty;

        private void OnEnable()
        {
            customSubjectScript = target as CustomSubjectScript;

            scriptProperty = serializedObject.FindProperty("m_Script");
            subjectDataManagerProperty = serializedObject.FindProperty("subjectDataManager");
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(scriptProperty);
            GUI.enabled = true;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(subjectDataManagerProperty);
            SubjectDataManager subjectDataManager = customSubjectScript.subjectDataManager;
            if (subjectDataManager == null)
            {
                string buttonText;
                subjectDataManager = FindObjectOfType<SubjectDataManager>();
                if (subjectDataManager != null)
                {
                    EditorGUILayout.HelpBox("SubjectDataManager is not assigned. A SubjectDataManager is in the scene", MessageType.Error);
                    buttonText = "Assign SubjectDataManager";
                }
                else
                {
                    EditorGUILayout.HelpBox("SubjectDataManager is not assigned.", MessageType.Error);
                    buttonText = "Assign SubjectDataManager";
                }

                if (GUILayout.Button(buttonText))
                {
                    if (subjectDataManager == null)
                    {
                        subjectDataManager = (new GameObject("SubjectDataManager")).AddComponent<SubjectDataManager>();
                    }
                    customSubjectScript.subjectDataManager = subjectDataManager;
                    EditorUtility.SetDirty(customSubjectScript);
                }
            }
            else
            {
                EditorGUILayout.LabelField("URI", subjectDataManager.BaseURI);
                EditorGUILayout.LabelField("Use default data", subjectDataManager.UseDefaultData.ToString());
                EditorGUILayout.LabelField("Enable Write data", subjectDataManager.EnableWriteData.ToString());
                EditorGUILayout.LabelField("Use XR Hand Subsystem", subjectDataManager.UseXRHandSubsystem.ToString());
                if (GUILayout.Button("Modify subject data manager"))
                {
                    Selection.activeObject = subjectDataManager;
                    InternalEditorUtility.SetIsInspectorExpanded(subjectDataManager, true);
                }
            }

            EditorGUILayout.Space();

            DrawPropertiesExcluding(serializedObject, excludedSerializedNames);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
