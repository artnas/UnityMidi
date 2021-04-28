using System;
using System.Collections.Generic;
using UnityEngine;

namespace Midi
{
    public class MidiProcessor
    {
        public List<MidiData.MidiBlock> AllBlocks = new List<MidiData.MidiBlock>();
        public List<MidiData.MidiTrack> Tracks = new List<MidiData.MidiTrack>();
        private List<Dictionary<byte, NoteOnEvent>> _noteTimeMap = new List<Dictionary<byte, NoteOnEvent>>();
        private int _currentBpm = 128;
        
        private struct NoteOnEvent
        {
            public float StartTimeMs;
            public byte Velocity;
            public int Count;

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
                _noteTimeMap.Add(new Dictionary<byte, NoteOnEvent>());
            }

            foreach (var track in midiFile.Tracks)
            {
                var map = _noteTimeMap[track.Index];
                
                foreach (var midiEvent in track.MidiEvents)
                {
                    var currentTimeMs = MidiUtilities.MidiTimeToMs(_currentBpm, midiFile.TicksPerQuarterNote, midiEvent.Time);
                    var note = midiEvent.Arg2;
                    
                    switch ((MidiRawData.MidiEventType)midiEvent.Type)
                    {
                        case MidiRawData.MidiEventType.NoteOff:
                            // block end
                            var noteOnEvent = map[note];
                            // create new block
                            var block = new MidiData.MidiBlock
                            {
                                StartTimeMs = noteOnEvent.StartTimeMs,
                                EndTimeMs = currentTimeMs,
                                Note = note,
                                Velocity = noteOnEvent.Velocity
                            };
                            
                            Tracks[track.Index].AddBlock(block);

                            if (map.TryGetValue(note, out var existingNote))
                            {
                                if (existingNote.Count == 1)
                                {
                                    map.Remove(note);
                                }
                                else
                                {
                                    map[note] = new NoteOnEvent
                                    {
                                        Count = existingNote.Count - 1,
                                        Velocity = existingNote.Velocity,
                                        StartTimeMs = existingNote.StartTimeMs
                                    };
                                }
                            }
                            else
                            {
                                throw new Exception($"note off event - could not find matching on event");
                            }

                            AllBlocks.Add(block);
                            break;
                        case MidiRawData.MidiEventType.NoteOn:
                            // block start
                            
                            if (map.TryGetValue(note, out var existingNoteOnEvent))
                            {
                                map[note] = new NoteOnEvent
                                {
                                    StartTimeMs = currentTimeMs,
                                    Velocity = midiEvent.Arg3,
                                    Count = existingNoteOnEvent.Count + 1
                                };
                            }
                            else
                            {
                                map.Add(note, new NoteOnEvent
                                {
                                    StartTimeMs = currentTimeMs,
                                    Velocity = midiEvent.Arg3,
                                    Count = 1
                                });
                            }
                            
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