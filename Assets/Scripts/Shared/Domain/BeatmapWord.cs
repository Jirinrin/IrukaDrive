using System;
using JetBrains.Annotations;

namespace Shared.Domain
{
    [Serializable]
    public class BeatmapWord
    {
        public float beat;
        public string text = ""; // will be split up into chars
        public float beatInterval = C.DefaultBeatInterval;

        public bool isChord;

        public BeatmapWord() { }
        public BeatmapWord(float beat)
        {
            this.beat = beat;
        }
        
        public BeatmapWord Clone(float? beatOverride = null, [CanBeNull] string textOverride = null) => new BeatmapWord
        {
            beat = beatOverride ?? beat,
            text = textOverride ?? text,
            beatInterval = beatInterval,
            isChord = isChord,
        };

        // ReSharper disable IdentifierTypo
        public bool ShouldSerializeisChord() => isChord;
        public bool ShouldSerializebeatInterval() => !isChord;
    }
}