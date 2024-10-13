using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace ubco.ovilab.ViconUnityStream.Editor
{
    [CanEditMultipleObjects]
    [CustomPropertyDrawer(typeof(XRHandJointRadius), true)]
    public class XRHandJointRadiusDrawer: PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty jointProp = property.FindPropertyRelative("joint");
            SerializedProperty valueProp = property.FindPropertyRelative("radius");

            float width = EditorGUIUtility.currentViewWidth;
            float valueHeight = EditorGUIUtility.singleLineHeight;

            // Calculate rects
            Rect keyRect = new Rect(position.x, position.y, position.width * 0.5f, valueHeight);
            Rect valueRect = new Rect(position.x + position.width * 0.51f, position.y, position.width * 0.48f, valueHeight);

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.LabelField(keyRect, $"{XRHandJointIDUtility.FromIndex(jointProp.enumValueIndex)}");
            EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
