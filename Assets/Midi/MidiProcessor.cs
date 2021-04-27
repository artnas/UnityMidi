using System;
using System.Collections.Generic;
using UnityEngine;

namespace Midi
{
    public class MidiProcessor
    {
        public List<MidiData.MidiBlock> AllBlocks = new List<MidiData.MidiBlock>();
        public List<MidiData.MidiTrack> Tracks = new List<MidiData.MidiTrack>();
        private Dictionary<byte, NoteOnEvent> _noteTimeMap = new Dictionary<byte, NoteOnEvent>();
        private int _currentBpm = 128;
        
        private struct NoteOnEvent
        {
            public float StartTimeMs;
            public byte Velocity;

            public bool Equals(NoteOnEvent other)
            {
                return StartTimeMs == other.StartTimeMs;
            }

            public override int GetHashCode()
            {
                return StartTimeMs.GetHashCode();
            }
        }

        public MidiProcessor(ParsedMidiFile midiFile)
        {
            Tracks = new List<MidiData.MidiTrack>(midiFile.TracksCount);
            for (var i = 0; i < midiFile.TracksCount; i++)
            {
                Tracks.Add(new MidiData.MidiTrack());
            }

            foreach (var track in midiFile.Tracks)
            {
                foreach (var midiEvent in track.MidiEvents)
                {
                    var currentTimeMs = MidiUtilities.MidiTimeToMs(_currentBpm, midiFile.TicksPerQuarterNote, midiEvent.Time);
                    var note = midiEvent.Arg2;
                    
                    switch ((MidiRawData.MidiEventType)midiEvent.Type)
                    {
                        case MidiRawData.MidiEventType.NoteOff:
                            // block end
                            var noteOnEvent = _noteTimeMap[note];
                            // create new block
                            var block = new MidiData.MidiBlock
                            {
                                StartTimeMs = noteOnEvent.StartTimeMs,
                                EndTimeMs = currentTimeMs,
                                Note = note,
                                Velocity = noteOnEvent.Velocity
                            };
                            
                            Tracks[track.Index].AddBlock(block);

                            _noteTimeMap.Remove(note);
                            
                            AllBlocks.Add(block);
                            break;
                        case MidiRawData.MidiEventType.NoteOn:
                            // block start
                            _noteTimeMap.Add(note, new NoteOnEvent
                            {
                                StartTimeMs = currentTimeMs,
                                Velocity = midiEvent.Arg3
                            });
                            break;
                        case MidiRawData.MidiEventType.KeyAfterTouch:
                            break;
                        case MidiRawData.MidiEventType.ControlChange:
                            break;
                        case MidiRawData.MidiEventType.ProgramChange:
                            break;
                        case MidiRawData.MidiEventType.ChannelAfterTouch:
                            break;
                        case MidiRawData.MidiEventType.PitchBendChange:
                            break;
                        case MidiRawData.MidiEventType.MetaEvent:
                            switch (midiEvent.MetaEventType)
                            {
                                case MidiRawData.MetaEventType.Tempo:
                                    _currentBpm = midiEvent.Note;
                                    break;
                                case MidiRawData.MetaEventType.TimeSignature:
                                    break;
                                case MidiRawData.MetaEventType.KeySignature:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            foreach (var track in Tracks)
            {
                if (track.Blocks.Count == 0)
                {
                    track.MinNote = track.MaxNote = 0;
                    track.MinVelocity = track.MaxVelocity = 0;
                }
            }
        }
    }
}