using Midi.Interpolators;
using UnityEngine;

namespace Midi.Visualisers
{
    public class ScaleVisualiser : MonoBehaviour
    {
        public MidiTrackValueInterpolator Interpolator;

        /// <summary>
        /// Value range. ex (0.25, 0.75) will keep the value between 0.25 to 0.75
        /// </summary>
        public Vector2 ScaleRange = new Vector2(0, 1f);
        
        private void Start()
        {
            if (!Interpolator)
                Interpolator = GetComponent<MidiTrackValueInterpolator>();
        }

        private void Update()
        {
            if (!Interpolator)
            {
                transform.localScale = Vector3.zero;
            }
            else
            {
                var valueWithinRange = ScaleRange.x + (ScaleRange.y - ScaleRange.x) * Interpolator.Value;
                transform.localScale = valueWithinRange * Vector3.one;
            }
        }
    }
}