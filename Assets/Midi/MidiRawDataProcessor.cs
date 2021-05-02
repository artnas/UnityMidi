using System;
using System.Collections.Generic;
using UnityEngine;

namespace Midi
{
    public class MidiRawDataProcessor
    {
        public readonly List<MidiData.MidiBlock> allBlocks = new List<MidiData.MidiBlock>();
        public readonly List<MidiData.MidiTrack> tracks;
        private readonly List<Dictionary<byte, NoteOnEvent>> _noteTimeMap = new List<Dictionary<byte, NoteOnEvent>>();
        public byte Bpm { get; private set; } = 120;
        
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

        public MidiRawDataProcessor(ParsedMidiFile midiFile, MidiImportSettings midiImportSettings)
        {
            tracks = new List<MidiData.MidiTrack>(midiFile.TracksCount);
            for (var i = 0; i < midiFile.TracksCount; i++)
            {
                tracks.Add(new MidiData.MidiTrack());
                _noteTimeMap.Add(new Dictionary<byte, NoteOnEvent>());
            }

            if (midiImportSettings.OverrideBpm)
            {
                Bpm = midiImportSettings.Bpm;
            }

            foreach (var track in midiFile.Tracks)
            {
                var map = _noteTimeMap[track.Index];
                
                foreach (var midiEvent in track.MidiEvents)
                {
                    var timsMs = MidiUtilities.MidiTimeToMs(Bpm, midiFile.TicksPerQuarterNote, midiEvent.Time);
                    var note = midiEvent.Arg2;
                    
                    switch ((MidiRawData.MidiEventType)midiEvent.Type)
                    {
                        case MidiRawData.MidiEventType.NoteOff: // block end
                            var noteOnEvent = map[note];
                            // create new block
                            var block = new MidiData.MidiBlock
                            {
                                StartTimeMs = noteOnEvent.StartTimeMs,
                                EndTimeMs = timsMs,
                                Note = note,
                                Velocity = noteOnEvent.Velocity
                            };
                            
                            tracks[track.Index].AddBlock(block);

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

                            allBlocks.Add(block);
                            break;
                        case MidiRawData.MidiEventType.NoteOn:  // block start
                            if (map.TryGetValue(note, out var existingNoteOnEvent))
                            {
                                map[note] = new NoteOnEvent
                                {
                                    StartTimeMs = timsMs,
                                    Velocity = midiEvent.Arg3,
                                    Count = existingNoteOnEvent.Count + 1
                                };
                            }
                            else
                            {
                                map.Add(note, new NoteOnEvent
                                {
                                    StartTimeMs = timsMs,
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
                                    // only set the bpm if override bpm is off
                                    if (!midiImportSettings.OverrideBpm)
                                    {
                                        // set new bpm
                                        Bpm = (byte) midiEvent.Note;
                                    }
                                    
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

            foreach (var track in tracks)
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