using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ubco.ovilab.ViconUnityStream.Editor
{
    [CustomEditor(typeof(CustomSubjectScript), true)]
    [CanEditMultipleObjects]
    public class CustomSubjectScriptEditor: UnityEditor.Editor
    {
        protected virtual string[] excludedSerializedNames => new string[]{"m_Script", "subjectDataManager", "OnHidingSubject", "OnShowingSubject"};
        private CustomSubjectScript customSubjectScript;

        private SerializedProperty scriptProperty;
        private SerializedProperty subjectDataManagerProperty;
        private SerializedProperty onHidingSubjectProperty;
        private SerializedProperty onShowingSubjectProperty;
        private bool eventFoldout;

        protected virtual void OnEnable()
        {
            customSubjectScript = target as CustomSubjectScript;

            scriptProperty = serializedObject.FindProperty("m_Script");
            subjectDataManagerProperty = serializedObject.FindProperty("subjectDataManager");
            onHidingSubjectProperty = serializedObject.FindProperty("OnHidingSubject");
            onShowingSubjectProperty = serializedObject.FindProperty("OnShowingSubject");
        }

        public override void OnInspectorGUI()
        {
            DrawSubjectHeader();
            DrawPropertiesExcluding(serializedObject, excludedSerializedNames);
            DrawEvents();
            serializedObject.ApplyModifiedProperties();
        }

        public virtual void DrawSubjectHeader()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(scriptProperty);
            GUI.enabled = true;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(subjectDataManagerProperty);
            SubjectDataManager subjectDataManager = subjectDataManagerProperty.objectReferenceValue as SubjectDataManager;
            if (subjectDataManager == null)
            {
                string buttonText;
                subjectDataManager = FindAnyObjectByType<SubjectDataManager>(FindObjectsInactive.Include);
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
                    subjectDataManagerProperty.objectReferenceValue = subjectDataManager;
                    EditorUtility.SetDirty(customSubjectScript);
                }
            }
            else
            {
                EditorGUILayout.LabelField("URI", subjectDataManager.BaseURI);
                EditorGUILayout.LabelField("Use default data", subjectDataManager.StreamType.ToString());
                EditorGUILayout.LabelField("Enable Write data", subjectDataManager.EnableWriteData.ToString());
                if (GUILayout.Button("Modify subject data manager"))
                {
                    Selection.activeObject = subjectDataManager;
                    InternalEditorUtility.SetIsInspectorExpanded(subjectDataManager, true);
                }
            }

            EditorGUILayout.Space();
        }

        public virtual void DrawEvents()
        {
            EditorGUILayout.Space();
            eventFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(eventFoldout, "Events");
            if (eventFoldout)
            {
                EditorGUILayout.PropertyField(onHidingSubjectProperty);
                EditorGUILayout.PropertyField(onShowingSubjectProperty);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
