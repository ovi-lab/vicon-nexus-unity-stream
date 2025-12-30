using UnityEngine;
using TMPro;
using ubco.ovilab.HPUI.Core.Interaction;

namespace ubco.ovilab.ViconUnityStream.Samples
{
    public class SampleInteractions: MonoBehaviour
    {
        public TextMeshPro text;

        public void OnGesture(HPUIGestureEventArgs args)
        {
            text.text = $"Gesture @ {args.Position} in {args.CumulativeDirection}";
        }
    }
}
