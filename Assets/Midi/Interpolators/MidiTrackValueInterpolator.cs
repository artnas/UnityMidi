using System;
using UnityEngine;

namespace Midi.Interpolators
{
    public class MidiTrackValueInterpolator : MonoBehaviour
    {
        /// <summary>
        /// The player to use
        /// </summary>
        public MidiPlayer MidiPlayer;

        /// <summary>
        /// Output value
        /// </summary>
        [Header("Value")]
        public float Value;
        
        /// <summary>
        /// Range of possible values for active notes
        /// </summary>
        public Vector2 ValueRange = new Vector2(0, 1f);

        /// <summary>
        /// Value interpolation speed when target value is lower than current
        /// </summary>
        [Range(0, 100f)] public float IncreasingValueInterpolationSpeed = 1f;
        
        /// <summary>
        /// Value interpolation speed when target value is lower than current
        /// </summary>
        [Range(0, 100f)] public float DecreasingValueInterpolationSpeed = 1f;
        
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

            // fix note filter range if x is bigger than y
            if (NoteFilterRange.x > NoteFilterRange.y)
            {
                var x = NoteFilterRange.x;
                NoteFilterRange.x = NoteFilterRange.y;
                NoteFilterRange.y = x;
            }
        }

        private void Update()
        {
            if (!MidiPlayer || MidiPlayer.ActiveTracks.Count <= TrackIndex)
            {
                UpdateValue(0);
                return;
            }
            
            UpdateToHighestActiveNote();
        }

        private void UpdateToHighestActiveNote()
        {
            var track = MidiPlayer.ActiveTracks[TrackIndex];

            var range = new Vector2Int(
                Mathf.Max(track.Track.MinNote, NoteFilterRange.x), 
                Mathf.Min(track.Track.MaxNote, NoteFilterRange.y));
            
            var maxActiveNote = 0;
            var activeBlocksInRange = 0;
            
            // iterate active blocks
            foreach (var activeBlock in track.ActiveBlocks)
            {
                // if outside note range, skip block
                if (activeBlock.Note < NoteFilterRange.x || activeBlock.Note > NoteFilterRange.y) 
                    continue;
                
                activeBlocksInRange++;

                if (activeBlock.Note > maxActiveNote)
                    maxActiveNote = activeBlock.Note;
            }

            // if has active blocks
            if (activeBlocksInRange > 0)
            {
                var percentageInNoteRange = ((float) maxActiveNote - range.x) / ((float) range.y - range.x);
                var valueWithinValueRange = ValueRange.x + (ValueRange.y - ValueRange.x) * percentageInNoteRange;
                UpdateValue(valueWithinValueRange);
            }
            else
            {
                UpdateValue(0);
            }
        }

        private void UpdateValue(float newValue)
        {
            // different interpolation depending on whether the value is higher or lower
            if (Value < newValue)
            {
                Value = Mathf.Lerp(Value, newValue, Time.deltaTime * IncreasingValueInterpolationSpeed);
            }
            else
            {
                Value = Mathf.Lerp(Value, newValue, Time.deltaTime * DecreasingValueInterpolationSpeed);
            }
        }
    }
}