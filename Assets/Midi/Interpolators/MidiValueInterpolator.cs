using System;
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

            var range = new Vector2Int(
                Mathf.Max(track.Track.MinNote, NoteFilterRange.x), 
                Mathf.Min(track.Track.MaxNote, NoteFilterRange.y));
            
            var maxActiveNote = Mathf.Max(track.Track.MinNote, NoteFilterRange.x);
            var activeBlocksInRange = 0;
            
            foreach (var activeBlock in track.ActiveBlocks)
            {
                // if outside range, skip
                if (activeBlock.Note < NoteFilterRange.x || activeBlock.Note > NoteFilterRange.y) 
                    continue;

                if (activeBlock.Note > maxActiveNote)
                {
                    maxActiveNote = activeBlock.Note;
                    activeBlocksInRange++;
                }
            }

            if (activeBlocksInRange == 0)
            {
                UpdateValue(0);
            }
            else
            {
                var percentageInNoteRange =
                    ((float)maxActiveNote - range.x) / ((float)range.y - range.x);

                UpdateValue(percentageInNoteRange);
            }
        }

        private void UpdateValue(float target)
        {
            var targetWithinRange = ValueRange.x + (ValueRange.y - ValueRange.x) * target;

            if (Value < targetWithinRange)
            {
                // fast interpolation if value is lower
                Value = Mathf.Lerp(Value, targetWithinRange, Time.deltaTime * IncreasingValueInterpolationSpeed);
            }
            else
            {
                Value = Mathf.Lerp(Value, targetWithinRange, Time.deltaTime * DecreasingValueInterpolationSpeed);
            }

            if (TargetTransform)
                TargetTransform.localScale = Vector3.one * Value;
        }
    }
}