using UnityEditor;

namespace ubco.ovilab.ViconUnityStream
{
    [FilePath("UserSettings/Vicon.config", FilePathAttribute.Location.ProjectFolder)]
    public class CustomSubjectConfig : ScriptableSingleton<CustomSubjectConfig>
    {
        private bool enabled = true;

        public bool Changed { get; private set; } = false;

        public bool Enabled {
            get {
                return enabled;
            }
            set {
                if (enabled != value)
                {
                    Changed = true;
                }
                enabled = value;
            }
        }

        public void Save()
        {
            Save(true);
            Changed = false;
        }
    }
}
