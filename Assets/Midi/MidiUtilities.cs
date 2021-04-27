namespace Midi
{
    public static class MidiUtilities
    {
        public static float MidiTimeToMs(int bpm, int ppq, int time)
        {
            return 60000f / (bpm * ppq) * time;
        }
    }
}