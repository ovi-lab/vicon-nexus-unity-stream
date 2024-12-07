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
            EditorGUILayout.PropertyField(viconXRSettingsObject.FindProperty("enableXRHandSubsystem"), Styles.EnableHandSubsystem);
            EditorGUILayout.PropertyField(viconXRSettingsObject.FindProperty("enableViconXRDevice"), Styles.EnableViconXRDevice);
            viconXRSettingsObject.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Recursive method to ensure directory exists
        /// </summary>
        internal static void EnsureDirectoryExists(string directory)
        {
            UnityEngine.Assertions.Assert.IsTrue(directory.Contains("Assets"), $"Trying to recursively create a directory not in Assets folder: {directory}");
            if (!AssetDatabase.IsValidFolder(directory))
            {
                string parentDirectory = Path.GetDirectoryName(directory);
                EnsureDirectoryExists(parentDirectory);
                AssetDatabase.CreateFolder(parentDirectory, Path.GetFileName(directory));
                AssetDatabase.Refresh();
            }
        }

        internal static ViconXRSettings GetOrCreateSettings()
        {
            ViconXRSettings settings = AssetDatabase.LoadAssetAtPath<ViconXRSettings>(ViconXRConstants.settingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<ViconXRSettings>();

                EnsureDirectoryExists(Path.GetDirectoryName(ViconXRConstants.settingsPath));
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

            if (settings != null)
            {
                EnsureObjectInPreLoadedAssets(settings);
            }

            return settings;
        }

        internal static bool EnsureObjectInPreLoadedAssets(Object obj)
        {
            List<Object> preLoadedAssets = UnityEditor.PlayerSettings.GetPreloadedAssets().ToList();
            if (!preLoadedAssets.Contains(obj))
            {
                preLoadedAssets.Add(obj);
                UnityEditor.PlayerSettings.SetPreloadedAssets(preLoadedAssets.ToArray());
            }
            return UnityEditor.PlayerSettings.GetPreloadedAssets().Contains(obj);
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        internal static bool EnsureLoaderIsCreated()
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
                return EnsureObjectInPreLoadedAssets(loader);
            }
            return false;
        }

        internal static bool IsSettingsAvailable()
        {
            return File.Exists(ViconXRConstants.settingsPath);
        }

        internal static bool IsLoaderAvailable()
        {
            return File.Exists(ViconXRConstants.loaderPath);
        }

        internal static void EnsureViconXRSettingsAndLoaderAreLoaded()
        {
            if (GetOrCreateSettings() == null)
            {
                Debug.LogError("Failed to load Vicon XR Settings asset");
            }

            if (!EnsureLoaderIsCreated())
            {
                Debug.LogError("Vicon settings failed to be loaded!");
            }
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateViconCustomSettingsProvider()
        {
            if (IsSettingsAvailable())
            {
                EnsureViconXRSettingsAndLoaderAreLoaded();
            }

            ViconXRSettingsProvider provider = new ViconXRSettingsProvider("Project/Vicon", SettingsScope.Project);

            // Automatically extract all keywords from the Styles.
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
            return provider;
        } 
    }
}
