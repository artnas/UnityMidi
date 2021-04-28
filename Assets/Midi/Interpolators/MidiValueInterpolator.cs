using UnityEngine;

namespace Midi.Interpolators
{
    public class MidiValueInterpolator : MonoBehaviour
    {
        /// <summary>
        /// The player to use
        /// </summary>
        public MidiPlayer MidiPlayer;
        
        public Transform TargetTransform;

        /// <summary>
        /// Output value
        /// </summary>
        [Header("Value")]
        public float Value;
        
        /// <summary>
        /// Value range. ex (0.25, 0.75) will keep the value between 0.25 to 0.75
        /// </summary>
        public Vector2 ValueRange = new Vector2(0, 1f);
        
        /// <summary>
        /// Value interpolation speed
        /// </summary>
        [Range(0, 100f)] public float InterpolationSpeed = 1f;
        
        /// <summary>
        /// Range in which note values will be taken into account
        /// </summary>
        [Header("Midi")]
        public Vector2Int NoteFilterRange = new Vector2Int(0, 127);
        
        /// <summary>
        /// Which track to use
        /// </summary>
        [Range(0, 255)] public int TrackIndex;

        private void Start()
        {
            // try getting the player automatically if not set
            if (MidiPlayer == null)
                MidiPlayer = GetComponent<MidiPlayer>();

            Value = ValueRange.x;
        }

        private void Update()
        {
            if (!MidiPlayer || MidiPlayer.ActiveTracks.Count <= TrackIndex)
            {
                UpdateValue(0);
                return;
            }
            
            UpdateValueFromTrack();
        }

        private void UpdateValueFromTrack()
        {
            var track = MidiPlayer.ActiveTracks[TrackIndex];
            var maxActiveNote = Mathf.Max(track.Track.MinNote, NoteFilterRange.x);
            
            foreach (var activeBlock in track.ActiveBlocks)
            {
                // if outside range, skip
                if (activeBlock.Note < NoteFilterRange.x || activeBlock.Note > NoteFilterRange.y) 
                    continue;
                
                if (activeBlock.Note > maxActiveNote)
                    maxActiveNote = activeBlock.Note;
            }

            var percentageInNoteRange =
                ((float)maxActiveNote - track.Track.MinNote) / ((float)track.Track.MaxNote - track.Track.MinNote);
            
            UpdateValue(percentageInNoteRange);
        }

        private void UpdateValue(float target)
        {
            var targetWithinRange = ValueRange.x + (ValueRange.y - ValueRange.x) * target;
            
            Value = Mathf.Lerp(Value, targetWithinRange, Time.deltaTime * InterpolationSpeed);
            
            if (TargetTransform)
                TargetTransform.localScale = Vector3.one * Value;
        }
    }
}