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
        public bool DisplayDebugBlocks = true;

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
            // try get audio source if unset
            if (!AudioSource)
                AudioSource = GetComponent<AudioSource>();
            
            if (MidiAsset && PlayOnAwake)
                Play();
        }

        public void Play()
        {
            Stop();

            if (!MidiAsset)
                throw new Exception("MidiPlayer MidiAsset is null");
            
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
                // if (track.Blocks.Count > 0)
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
            
            if (AudioSource && AudioSource.loop)
            {
                // restart if audio source is looping
                _coroutine = StartCoroutine(MidiEnumerator(MidiAsset));
            }
            else
            {
                // finish otherwise
                _coroutine = null;
                OnPlayingStopped?.Invoke(true);
            }
        }

        private void OnBlockStart(MidiData.MidiBlock block)
        {
            OnBlockStarted?.Invoke(block);
        }
        
        private void OnBlockEnd(MidiData.MidiBlock block)
        {
            OnBlockCompleted?.Invoke(block);
        }

        [ContextMenu("Fast forward")]
        private void FastForward()
        {
            if (AudioSource)
            {
                AudioSource.time += 50;
            }
        }
        
        void OnGUI()
        {
            if (_coroutine == null || !DisplayDebugUi)
                return;
            
            var time = AudioSource ? AudioSource.time : Time.time - _startTime;

            var text = $"MIDI {_currentMidiFile.name} playing ({time:0.##}s)\n";
            for (var index = 0; index < ActiveTracks.Count; index++)
            {
                var trackData = ActiveTracks[index];
                text += $"track {index} - current block: {(trackData.CurrentBlockIndex == -1 ? "-" : trackData.CurrentBlockIndex.ToString())} ({trackData.ActiveBlocks.Count} active blocks)\n";
            }

            GUI.Label(new Rect(10, 10, 400, 400), text);
        }

        private void OnDrawGizmosSelected()
        {
            if (!DisplayDebugBlocks || !MidiAsset)
                return;
            
            var time = AudioSource ? AudioSource.time : Time.time - _startTime;

            var heightOffset = 10;

            var totalHeight = 10;
            foreach (var track in MidiAsset.Data.Tracks)
            {
                totalHeight += track.MaxNote - track.MinNote + 10;
            }

            for (var trackIndex = 0; trackIndex < MidiAsset.Data.Tracks.Count; trackIndex++)
            {
                var track = MidiAsset.Data.Tracks[trackIndex];
                var activeTrackData = _coroutine != null ? ActiveTracks[trackIndex] : null;

                for (var blockIndex = 0; blockIndex < track.Blocks.Count; blockIndex++)
                {
                    var block = track.Blocks[blockIndex];

                    if (_coroutine != null && activeTrackData.CurrentBlockIndex >= blockIndex)
                    {
                        var blockInProgress = block.EndTimeSec > time;
                        Gizmos.color = blockInProgress? Color.blue : Color.green;
                    }
                    else
                    {
                        Gizmos.color = Color.white;
                    }

                    var height = track.MaxNote - block.Note + heightOffset;

                    var center = block.StartTimeSec + block.LengthSec / 2f;
                    Gizmos.DrawCube(new Vector3(center, -height + totalHeight, 0), new Vector3(block.LengthSec, 1, 1));
                }

                heightOffset += track.MaxNote - track.MinNote + 10;
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(new Vector3(time, 0), new Vector3(time, 127));
        }
    }
}