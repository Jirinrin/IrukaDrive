using System;
using Shared;

namespace Gameplay
{
    [Serializable]
    public class BeatmapWord
    {
        public float beat;
        public string text = ""; // will be split up into chars
        public float beatInterval = C.DefaultBeatInterval;

        public BeatmapWord() { }
        public BeatmapWord(float beat)
        {
            this.beat = beat;
        }
    }
}