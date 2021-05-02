using System;

namespace Midi
{
    [Serializable]
    public class MidiImportSettings
    {
        public bool OverrideBpm;
        public byte Bpm = 120;
    }
}