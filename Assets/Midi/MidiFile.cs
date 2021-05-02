using UnityEngine;

namespace Midi
{
    /// <summary>
    /// https://music.stackexchange.com/a/34633
    /// </summary>
    public class MidiFile : ScriptableObject
    {
        public MidiRawData RawData;
        public MidiData Data;

        public static MidiFile Create(string filePath, MidiImportSettings midiImportSettings)
        {
            var midiFileAsset = CreateInstance<MidiFile>();
            var parsedMidiFile = new ParsedMidiFile(filePath);

            midiFileAsset.RawData = new MidiRawData
            {
                Format = parsedMidiFile.Format,
                TicksPerQuarterNote = parsedMidiFile.TicksPerQuarterNote,
                Tracks = parsedMidiFile.Tracks,
                TracksCount = parsedMidiFile.TracksCount
            };
            
            var processor = new MidiRawDataProcessor(parsedMidiFile, midiImportSettings);
            midiFileAsset.Data = new MidiData
            {
                Tracks = processor.tracks,
                Bpm = processor.Bpm
            };

            return midiFileAsset;
        }

        [ContextMenu("Print raw data")]
        public void PrintRawData()
        {
            Debug.Log($"Format: {RawData.Format}");
            Debug.Log($"TicksPerQuarterNote: {RawData.TicksPerQuarterNote}");
            Debug.Log($"TracksCount: {RawData.TracksCount}");

            foreach (var track in RawData.Tracks)
            {
                Debug.Log($"\nTrack: {track.Index}\n");

                foreach (var midiEvent in track.MidiEvents)
                {
                    Debug.Log(midiEvent);
                }
            }
        }
        
        [ContextMenu("Print processed data")]
        public void PrintProcessedData()
        {
            Debug.Log($"TracksCount: {Data.Tracks.Count}");

            for (var trackIndex = 0; trackIndex < Data.Tracks.Count; trackIndex++)
            {
                var track = Data.Tracks[trackIndex];
                Debug.Log($"\nTrack: {trackIndex}. note range: {track.MinNote} -> {track.MaxNote}, velocity range: {track.MinVelocity} -> {track.MaxVelocity}\n");

                foreach (var block in track.Blocks)
                {
                    Debug.Log(block);
                }
            }
        }
    }
}