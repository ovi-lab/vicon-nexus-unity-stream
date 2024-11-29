using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.XR.Hands;
using System;

namespace ubco.ovilab.ViconUnityStream.Editor
{
    [CustomEditor(typeof(CustomHandScript), true)]
    [CanEditMultipleObjects]
    public class CustomHandScriptEditor: CustomSubjectScriptEditor
    {
        protected override string[] excludedSerializedNames => base.excludedSerializedNames.Union(new string[]{"driveSkeleton", "xrHandJointRadiiList"}).ToArray();
        private CustomHandScript customSubjectScript;

        private SerializedProperty driveSekeltonProperty;
        private SerializedProperty xrHandJointRadiiListProperty;
        private bool usingXRHands, jointRadiiFolout;
        // private SerializedProperty subjectDataManagerProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            customSubjectScript = target as CustomHandScript;

            driveSekeltonProperty = serializedObject.FindProperty("driveSkeleton");
            xrHandJointRadiiListProperty = serializedObject.FindProperty("xrHandJointRadiiList");

            if (xrHandJointRadiiListProperty.arraySize == 0)
            {
                int maxVal = xrHandJointRadiiListProperty.arraySize = XRHandJointIDUtility.ToIndex(XRHandJointID.EndMarker);
                for (int i = XRHandJointIDUtility.ToIndex(XRHandJointID.BeginMarker); i < maxVal; ++i)
                {
                    SerializedProperty prop = xrHandJointRadiiListProperty.GetArrayElementAtIndex(i);
                    prop.FindPropertyRelative("joint").enumValueIndex = i;
                }
                serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI()
        {
            DrawSubjectHeader();
            usingXRHands = ViconXRSettings.runtimeInstance.EnableXRHandSubsystem;

            bool guiEnabled = GUI.enabled;
            EditorGUILayout.Space();
            GUI.enabled = !usingXRHands;
            if (usingXRHands)
            {
                driveSekeltonProperty.boolValue = true;
                EditorGUILayout.HelpBox("XR Hands requires drive skeleton to be active. See `Project settings > Vicon` to configure this.", MessageType.Info);
            }

            EditorGUILayout.PropertyField(driveSekeltonProperty);
            GUI.enabled = guiEnabled;

            DrawPropertiesExcluding(serializedObject, excludedSerializedNames);

            jointRadiiFolout = EditorGUILayout.BeginFoldoutHeaderGroup(jointRadiiFolout, "XR Hand Joint Radii List", null, ShowHeaderContextMenu);
            if (jointRadiiFolout)
            {
                EditorGUI.indentLevel ++;
                for (int i = XRHandJointIDUtility.ToIndex(XRHandJointID.ThumbMetacarpal); i<XRHandJointIDUtility.ToIndex(XRHandJointID.EndMarker); ++i)
                {
                    SerializedProperty prop = xrHandJointRadiiListProperty.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(prop, GUIContent.none);
                }
                EditorGUI.indentLevel --;
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void ShowHeaderContextMenu(Rect position)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Reset Values"), false, ResetClicked);
            menu.DropDown(position);
        }

        private void ResetClicked()
        {
            Undo.RecordObject(Selection.activeObject, "Resetting values");
            xrHandJointRadiiListProperty.ClearArray();
            UnityEngine.Assertions.Assert.AreEqual(xrHandJointRadiiListProperty.arraySize, 0);
            xrHandJointRadiiListProperty.arraySize = XRHandJointIDUtility.ToIndex(XRHandJointID.EndMarker);

            for (int i = XRHandJointIDUtility.ToIndex(XRHandJointID.BeginMarker); i < XRHandJointIDUtility.ToIndex(XRHandJointID.EndMarker); ++i)
            {
                SerializedProperty prop = xrHandJointRadiiListProperty.GetArrayElementAtIndex(i);
                prop.FindPropertyRelative("joint").enumValueIndex = i;
                prop.FindPropertyRelative("radius").floatValue = 0;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
