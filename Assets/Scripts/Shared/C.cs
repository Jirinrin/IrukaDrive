using System.Collections.Generic;
using UnityEngine;

namespace Shared
{
    public static class C
    {
        public const float FloatTolerance = 0.001f;
    
        public const float EditorBeatSnap = .5f;
        public const float DefaultBeatInterval = 1f;
        public static readonly List<float> BeatIntervalValues = new List<float>{.25f, .3333f, .5f, 1f, 2f, 4f};

        // todo: better system than *1000 and /1000?
        public const float BeatIndexFactor = 1000f;
        public const int BeatIndexFactorInt = 1000;
        public static int BeatToIndex(this float value) => Mathf.RoundToInt(value * BeatIndexFactor);
        public static float IndexToBeat(this int value) => value / BeatIndexFactor;

        public const float DefaultScrollSpeed = 40f;

        public const float TimingWindowPerfect = 90; // ms
        public const float TimingWindowGood = 140; // ms
        public const float TimingWindowMiss = 500; // ms
        public const float TimingWindowMissSec = .5f;
    }
}
