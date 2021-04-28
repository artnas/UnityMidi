using System;
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

        public AudioSource AudioSource;

        public readonly List<TrackProgress> ActiveTracks = new List<TrackProgress>();

        private MidiFile _currentMidiFile;
        private Coroutine _coroutine;
        private float _startTime;
        
        public Action<MidiData.MidiBlock> OnBlockStarted => midiBlock => { }; 
        public Action<MidiData.MidiBlock> OnBlockCompleted => midiBlock => { }; 
        public Action OnPlayingStarted => () => { }; 
        public Action<bool> OnPlayingStopped => completed => { }; 
        
        public class TrackProgress
        {
            public MidiData.MidiTrack Track;
            public List<MidiData.MidiBlock> ActiveBlocks = new List<MidiData.MidiBlock>();
            public int CurrentBlockIndex = -1;
        }
        
        private void Awake()
        {
            // try get audio source
            if (!AudioSource)
                AudioSource = GetComponent<AudioSource>();
            
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
                OnPlayingStopped?.Invoke(false);
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }

        private IEnumerator MidiEnumerator(MidiFile midiAsset)
        {
            OnPlayingStarted?.Invoke();
            
            _currentMidiFile = midiAsset;
            _startTime = Time.time;
            
            ActiveTracks.Clear();
            foreach (var track in midiAsset.Data.Tracks)
            {
                if (track.Blocks.Count > 0)
                {
                    ActiveTracks.Add(new TrackProgress
                    {
                        Track = track
                    });
                }
            }

            while (true)
            {
                var timeSinceStart = AudioSource ? AudioSource.time : Time.time - _startTime;
                var hasUnfinishedTracks = false;
                
                // iterate all tracks
                foreach (var trackData in ActiveTracks)
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
            
            OnPlayingStopped?.Invoke(true);
        }

        private void OnBlockStart(MidiData.MidiBlock block)
        {
            OnBlockStarted?.Invoke(block);
        }
        
        private void OnBlockEnd(MidiData.MidiBlock block)
        {
            OnBlockCompleted?.Invoke(block);
        }
        
        void OnGUI()
        {
            if (_coroutine == null || !DisplayDebugUi)
                return;

            var text = $"MIDI {_currentMidiFile.name} playing ({Time.time - _startTime:0.##}s)\n";
            for (var index = 0; index < ActiveTracks.Count; index++)
            {
                var trackData = ActiveTracks[index];
                text += $"track {index} - current block: {trackData.CurrentBlockIndex} ({trackData.ActiveBlocks.Count} active blocks)";
            }

            GUI.Label(new Rect(10, 10, 400, 400), text);
        }
    }
}