using System;
using System.Collections.Generic;

namespace ubco.ovilab.ViconUnityStream
{
    [Serializable]
    public class ViconStreamData
    {
        public Dictionary<string, List<float>> data;
        public Dictionary<string, List<string>> hierachy;

        public ViconStreamData()
        {
            data = new();
        }
    }
}
