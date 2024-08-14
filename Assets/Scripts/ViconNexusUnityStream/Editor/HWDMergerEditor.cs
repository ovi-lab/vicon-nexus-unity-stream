using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ubco.ovilab.ViconUnityStream.Utils;

namespace ubco.ovilab.ViconUnityStream.Editor
{
    [CustomEditor(typeof(HWDMerger), true)]
    [CanEditMultipleObjects]
    public class HWDMergerEditor: UnityEditor.Editor
    {
        HWDMerger hwdMerger;
        private void OnEnable()
        {
             hwdMerger = target as HWDMerger;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Merge HWDs"))
            {
                hwdMerger.MergeHWDs();
            }
            GUI.enabled = true;
        }
    }
}
