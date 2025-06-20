using ubco.ovilab.ViconUnityStream.Utils;
using UnityEditor;
using UnityEngine;

namespace ubco.ovilab.ViconUnityStream.Editor
{
    [CustomEditor(typeof(ViconCoordinateSystemMerger), true)]
    [CanEditMultipleObjects]
    public class ViconCoordinateSystemMergerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ViconCoordinateSystemMerger merger = (ViconCoordinateSystemMerger)target;
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Merge Coordinate Systems"))
                {
                    merger.MergeCoordinateSystems();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use the merge function.", MessageType.Info);
            }
            
        }
    }
}