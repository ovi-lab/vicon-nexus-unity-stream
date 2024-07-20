using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ubco.ovilab.ViconUnityStream.Editor
{
    /// <summary>
    /// <see cref="SettingsProvider"/> for <see cref="ViconXRSettings"/>
    /// </summary>
    public class ViconXRSettingsProvider: SettingsProvider
    {
        private SerializedObject viconXRSettingsObject;

        class Styles
        {
            public static GUIContent HMDPositionOffset = new GUIContent("HMD Position Offset", "Offset to be applied to the XRDevice (HMD) data before setting the centerEyePosition.");
            public static GUIContent EnableHandSubsystem = new GUIContent("Enable Hand Subsystem", "Enable XRHandSubsystem.");
            public static GUIContent EnableViconXRDevice = new GUIContent("Enable Vicon XR Device", "Enable Vicon XR Device which provides HMD positions through Input system.");
        }

        public ViconXRSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope) {}

        /// <inheritdoc />
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            viconXRSettingsObject = GetSerializedSettings();
        }

        /// <inheritdoc />
        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.PropertyField(viconXRSettingsObject.FindProperty("_HMDPositionOffset"), Styles.HMDPositionOffset);
            EditorGUILayout.PropertyField(viconXRSettingsObject.FindProperty("enableXRHandSubsystem"), Styles.EnableHandSubsystem);
            EditorGUILayout.PropertyField(viconXRSettingsObject.FindProperty("enableViconXRDevice"), Styles.EnableViconXRDevice);
            viconXRSettingsObject.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static ViconXRSettings GetOrCreateSettings()
        {
            ViconXRSettings settings = AssetDatabase.LoadAssetAtPath<ViconXRSettings>(ViconXRConstants.settingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<ViconXRSettings>();

                string directoryPath = Path.GetDirectoryName(ViconXRConstants.settingsPath);
                if (!AssetDatabase.IsValidFolder(directoryPath))
                {
                    string parentDirectoryPath = Path.GetDirectoryName(directoryPath);
                    if (!AssetDatabase.IsValidFolder(parentDirectoryPath))
                    {
                        AssetDatabase.CreateFolder("Assets/", Path.GetFileName(parentDirectoryPath));
                    }
                    AssetDatabase.CreateFolder(parentDirectoryPath, Path.GetFileName(directoryPath));
                }
                AssetDatabase.Refresh();

                AssetDatabase.CreateAsset(settings, ViconXRConstants.settingsPath);
                AssetDatabase.SaveAssets();
                try
                {
                    EditorBuildSettings.AddConfigObject(ViconXRConstants.settingsKey, settings, true);
                }
                catch(System.Exception e)
                {
                    Debug.LogError($"Failed to save ViconXRSettings asset: {e.Message}");
                }
            }

            EnsureLoaderIsCreated();
            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        internal static void EnsureLoaderIsCreated()
        {
            ViconXRLoader loader = AssetDatabase.LoadAssetAtPath<ViconXRLoader>(ViconXRConstants.loaderPath);
            if (loader == null)
            {
                loader = ScriptableObject.CreateInstance<ViconXRLoader>();
                AssetDatabase.CreateAsset(loader, ViconXRConstants.loaderPath);
                AssetDatabase.SaveAssets();
                try
                {
                    EditorBuildSettings.AddConfigObject(ViconXRConstants.loaderKey, loader, true);
                }
                catch(System.Exception e)
                {
                    Debug.LogError($"Failed to setup ViconXRLoader asset: {e.Message}");
                }
            }

            if (loader != null)
            {
                List<Object> preLoadedAssets = UnityEditor.PlayerSettings.GetPreloadedAssets().ToList();
                if (!preLoadedAssets.Contains(loader))
                {
                    preLoadedAssets.Add(loader);
                    UnityEditor.PlayerSettings.SetPreloadedAssets(preLoadedAssets.ToArray());
                }
            }
        }

        public static bool IsSettingsAvailable()
        {
            return File.Exists(ViconXRConstants.settingsPath);
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            if (IsSettingsAvailable())
            {
                GetOrCreateSettings();
            }

            ViconXRSettingsProvider provider = new ViconXRSettingsProvider("Project/Vicon", SettingsScope.Project);

            // Automatically extract all keywords from the Styles.
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
            return provider;
        } 
    }
}
