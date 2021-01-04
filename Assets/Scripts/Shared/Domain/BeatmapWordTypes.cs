using System;
using JetBrains.Annotations;
using Tools;

namespace Shared.Domain
{
    [Serializable]
    public class BeatmapWord : BeatmapWordBase
    {
        public float beatInterval = C.DefaultBeatInterval;

        public BeatmapWord() { }
        public BeatmapWord(float beat) : base(beat) { }
        
        public override float BeatWidth => (text.Length-1) * beatInterval;
        
        public BeatmapWord Clone(float? beatOverride = null, [CanBeNull] string textOverride = null) => new BeatmapWord
        {
            beat = beatOverride ?? beat,
            text = textOverride ?? text,
            beatInterval = beatInterval,
        };
    }

    [Serializable]
    public class BeatmapChord : BeatmapWordBase
    {
        public override float BeatWidth => 0f;

        public BeatmapChord Clone(float? beatOverride = null, [CanBeNull] string textOverride = null) => new BeatmapChord
        {
            beat = beatOverride ?? beat,
            text = textOverride ?? text,
        };
    }
    
    [Serializable]
    public abstract class BeatmapWordBase
    {
        public string text = ""; // will be split up into chars
        public float beat;
        
        public abstract float BeatWidth { get; }

        protected BeatmapWordBase() { }
        protected BeatmapWordBase(float beat) => this.beat = beat;
    }
}