using System;
using System.Collections.Generic;

namespace Midi
{
    [Serializable]
    public class MidiRawData
    {
        public int Format;

        public int TicksPerQuarterNote;

        public MidiRawTrack[] Tracks;

        public int TracksCount;
        
        [Serializable]
        public class MidiRawTrack
        {
            public int Index;

            public List<MidiEvent> MidiEvents = new List<MidiEvent>();

            public List<TextEvent> TextEvents = new List<TextEvent>();
        }

        [Serializable]
        public struct MidiEvent
        {
            public int Time;

            public byte Type;

            public byte Arg1;

            public byte Arg2;

            public byte Arg3;

            public MidiEventType MidiEventType => (MidiEventType)Type;

            public MetaEventType MetaEventType => (MetaEventType)Arg1;

            public int Channel => Arg1;

            public int Note => Arg2;

            public int Velocity => Arg3;

            public ControlChangeType ControlChangeType => (ControlChangeType)Arg2;

            public int Value => Arg3;

            public override string ToString()
            {
                return MidiEventType == MidiEventType.MetaEvent 
                    ? $"event type: {MetaEventType}, channel: -, time: {Time}, note: {Note}, velocity: {Velocity}" 
                    : $"event type: {MidiEventType}, channel: {Channel}, time: {Time}, note: {Note}, velocity: {Velocity}";
            }
        }

        [Serializable]
        public struct TextEvent
        {
            public int Time;

            public byte Type;

            public string Value;

            public TextEventType TextEventType => (TextEventType)Type;
        }
        
        public enum MidiEventType : byte
        {
            NoteOff = 0x80,

            NoteOn = 0x90,

            KeyAfterTouch = 0xA0,

            ControlChange = 0xB0,

            ProgramChange = 0xC0,

            ChannelAfterTouch = 0xD0,

            PitchBendChange = 0xE0,

            MetaEvent = 0xFF
        }

        public enum ControlChangeType : byte
        {
            BankSelect = 0x00,

            Modulation = 0x01,

            Volume = 0x07,

            Balance = 0x08,

            Pan = 0x0A,

            Sustain = 0x40
        }

        public enum TextEventType : byte
        {
            Text = 0x01,

            TrackName = 0x03,

            Lyric = 0x05,
        }

        public enum MetaEventType : byte
        {
            Tempo = 0x51,

            TimeSignature = 0x58,

            KeySignature = 0x59
        }
    }
}