using System;
using System.Collections.Generic;
using UnityEngine;

namespace Midi
{
    [Serializable]
    public class MidiData
    {
        public List<MidiTrack> Tracks = new List<MidiTrack>();
        public byte Bpm;

        [Serializable]
        public class MidiBlock
        {
            public float StartTimeMs;
            public float EndTimeMs;
            public byte Note;
            public byte Velocity;

            public float StartTimeSec => StartTimeMs / 1000f;
            public float EndTimeSec => EndTimeMs / 1000f;

            public float LengthSec => EndTimeSec - StartTimeSec;

            public override string ToString()
            {
                return $"note: {Note}, velocity: {Velocity}, time: {StartTimeSec}s -> {EndTimeSec}s ({EndTimeSec - StartTimeSec}s)";
            }
        }

        [Serializable]
        public class MidiTrack
        {
            public List<MidiBlock> Blocks = new List<MidiBlock>();

            public byte MinNote = byte.MaxValue;
            public byte MaxNote;

            public byte MinVelocity = byte.MaxValue;
            public byte MaxVelocity;
            
            public void AddBlock(MidiBlock midiBlock)
            {
                Blocks.Add(midiBlock);

                // if (midiBlock.Note != 0)
                MinNote = (byte)Mathf.Min(MinNote, midiBlock.Note);
                MaxNote = (byte)Mathf.Max(MaxNote, midiBlock.Note);
                
                // if (midiBlock.Velocity != 0)
                MinVelocity = (byte)Mathf.Min(MinVelocity, midiBlock.Velocity);
                MaxVelocity = (byte)Mathf.Max(MaxVelocity, midiBlock.Velocity);
            }
        }
    }
}