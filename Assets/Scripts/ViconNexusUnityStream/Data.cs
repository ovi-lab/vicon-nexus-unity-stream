using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace ubco.ovilab.ViconUnityStream
{
    [Serializable]
    public class Data
    {
        public Dictionary<string, List<float>> position;
        public Dictionary<string, List<string>> hierachy;
    }
}
