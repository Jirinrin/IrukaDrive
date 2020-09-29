using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay
{
    [Serializable]
    public class Beatmap
    {
        public string songFile;
        [NonSerialized][XmlIgnore] public AudioClip song;
        [NonSerialized][XmlIgnore] public string filePath;
        public float bpm;
        public float beatOffset;
        [Range(2,4)] public int beatsPerBar = 4;
        public int barOffset;
        // Expected to already be sorted
        public List<BeatmapWord> words;
    }

    public struct BeatmapResult
    {
        public List<RuntimeNote> NoteResults;
    }
}