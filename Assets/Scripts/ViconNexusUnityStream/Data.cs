using System;
using System.Collections.Generic;

namespace ubco.ovilab.ViconUnityStream
{
    [Serializable]
    public class Data
    {
        public Dictionary<string, List<float>> data;
        public Dictionary<string, List<string>> hierachy;
    }
}
