using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.XR.Management.Metadata;

namespace ubco.ovilab.ViconUnityStream.Editor
{
    class ViconXRMetadata: IXRPackage
    {
        class ViconXRLoaderMetadata : IXRLoaderMetadata
        {
            public string loaderName { get; set; }
            public string loaderType { get; set; }
            public List<BuildTargetGroup> supportedBuildTargets { get; set; }
        }

        class ViconXRPackageMetadata : IXRPackageMetadata
        {
            public string packageName { get; set; }
            public string packageId { get; set; }
            public string settingsType { get; set; }
            public List<IXRLoaderMetadata> loaderMetadata { get; set; }
        }

        public IXRPackageMetadata metadata
        {
            get
            {
                // // Register package notification information anytime the metadata is asked requested.
                // var packageNotificationInfo = new PackageNotificationInfo(GUIContent.none, "");
                // PackageNotificationUtils.RegisterPackageNotificationInformation(ViconXRConstants.packageID, packageNotificationInfo);
                return new ViconXRPackageMetadata() {
                    packageName = "ViconXR",
                        packageId = ViconXRConstants.packageID,
                        // settingsType = typeof(SampleSettings).FullName,

                        loaderMetadata = new List<IXRLoaderMetadata>() {
                            new ViconXRLoaderMetadata()
                            {
                                loaderName = "Vicon",
                                loaderType = typeof(ViconXRLoader).FullName,
                                supportedBuildTargets = new List<BuildTargetGroup>() {
                                    BuildTargetGroup.Standalone,
                                    BuildTargetGroup.Android
                                }
                            },
                        }
                };
            }
        }

        public bool PopulateNewSettingsInstance(ScriptableObject obj)
        {
            return true;
        }

    }
}
