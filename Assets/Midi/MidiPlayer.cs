using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Midi
{
    public class MidiPlayer : MonoBehaviour
    {
        public MidiFile MidiAsset;
        public bool PlayOnAwake = true;
        public bool DisplayDebugUi;

        private MidiFile _currentMidiFile;
        private Coroutine _coroutine;
        private float _startTime;
        List<TrackProgress> _tracks = new List<TrackProgress>();
        
        private void Awake()
        {
            if (MidiAsset && PlayOnAwake)
                Play();
        }

        public void Play()
        {
            Stop();
            _coroutine = StartCoroutine(MidiEnumerator(MidiAsset));
        }

        public void Stop()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }

        private class TrackProgress
        {
            public MidiData.MidiTrack Track;
            public List<MidiData.MidiBlock> ActiveBlocks = new List<MidiData.MidiBlock>();
            public int CurrentBlockIndex = -1;
        }
        
        private IEnumerator MidiEnumerator(MidiFile midiAsset)
        {
            _currentMidiFile = midiAsset;
            _startTime = Time.time;
            
            _tracks.Clear();
            foreach (var track in midiAsset.Data.Tracks)
            {
                if (track.Blocks.Count > 0)
                {
                    _tracks.Add(new TrackProgress
                    {
                        Track = track
                    });
                }
            }

            while (true)
            {
                var timeSinceStart = Time.time - _startTime;
                var hasUnfinishedTracks = false;
                
                // iterate all tracks
                foreach (var trackData in _tracks)
                {
                    var nextBlockIndex = trackData.CurrentBlockIndex + 1;

                    // try get next block
                    if (nextBlockIndex < trackData.Track.Blocks.Count)
                    {
                        hasUnfinishedTracks = true;
                        
                        var nextBlock = trackData.Track.Blocks[nextBlockIndex];

                        if (timeSinceStart >= nextBlock.StartTimeSec)
                        {
                            // add block to the list
                            trackData.ActiveBlocks.Add(nextBlock);
                            trackData.CurrentBlockIndex++;
                            OnBlockStart(nextBlock);
                        }
                    }

                    // check if any active block has finished
                    for (var i = 0; i < trackData.ActiveBlocks.Count; i++)
                    {
                        var block = trackData.ActiveBlocks[i];
                        if (timeSinceStart >= block.EndTimeSec)
                        {
                            // remove finished block from the list
                            trackData.ActiveBlocks.Remove(block);
                            i--;
                            OnBlockEnd(block);
                        }
                    }
                }

                // break if all tracks finished
                if (!hasUnfinishedTracks)
                    break;

                yield return null;
            }

            _coroutine = null;
        }

        private void OnBlockStart(MidiData.MidiBlock block)
        {
        }
        
        private void OnBlockEnd(MidiData.MidiBlock block)
        {
        }
        
        void OnGUI()
        {
            if (_coroutine == null || !DisplayDebugUi)
                return;

            var text = $"MIDI {_currentMidiFile.name} playing ({Time.time - _startTime:0.##}s)\n";
            for (var index = 0; index < _tracks.Count; index++)
            {
                var trackData = _tracks[index];
                text += $"track {index} - current block: {trackData.CurrentBlockIndex} ({trackData.ActiveBlocks.Count} active blocks)";
            }

            GUI.Label(new Rect(10, 10, 400, 400), text);
        }
    }
}