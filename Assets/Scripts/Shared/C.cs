using System.Collections.Generic;
using UnityEngine;

namespace Shared
{
    public static class C
    {
        public const float FloatTolerance = 0.001f;
    
        public const float EditorBeatSnap = .5f;
        public const float EditorBeatSnapFine = .25f;
        public const float EditorBeatSnapSuperFine = .01f;
        public const float DefaultBeatInterval = 1f;
        public static readonly List<float> BeatIntervalValues = new List<float>{.125f, 1f/6f, .25f, 1f/3f, .5f, 1f, 2f, 4f};

        // todo: better system than *1000 and /1000?
        public const float BeatIndexFactor = 1000f;
        public const int BeatIndexFactorInt = 1000;
        public static int BeatToIndex(this float value) => Mathf.RoundToInt(value * BeatIndexFactor);
        public static float IndexToBeat(this int value) => value / BeatIndexFactor;

        public const float DefaultScrollSpeed = 40f;

        public const float TimingWindowPerfect = 90; // ms
        public const float TimingWindowPerfectSec = TimingWindowPerfect/1000f; // ms
        public const float TimingWindowGood = 140; // ms
        public const float TimingWindowGoodSec = TimingWindowGood/1000f;

        public static readonly Color CharColorDefaultGameplay = new Color(.5f,.5f,.5f);
        public static readonly Color CharColorDefaultEditor = Color.white;
        public static Color CharColorDefault => GameManager.State == GameState.BeatmapEditor ? CharColorDefaultEditor : CharColorDefaultGameplay;
        public static readonly Color CharColorHighlight = Color.green;

        public static readonly string DriveChartsDirPath = $"{Application.streamingAssetsPath}/DriveCharts";
    }
}
