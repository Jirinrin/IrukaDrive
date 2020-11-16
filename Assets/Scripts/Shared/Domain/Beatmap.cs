using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Shared.Domain
{
    [Serializable]
    public class Beatmap
    {
        // Cosmetic stuff
        public string title;
        public string artist;
        public string jacketFile;

        // Important metadata
        public string songFile;
        [NonSerialized][XmlIgnore] public AudioClip song;
        [NonSerialized][XmlIgnore] public string filePath;
        public float bpm;
        public float beatOffset;
        [Range(2,4)] public int beatsPerBar = 4;
        public int barOffset;
        public float? finishTimestamp; // You can specify this to have a beatmap end before the song file ends
        public Guid id; // Generated automatically by the beatmap editor

        // Notes, expected to be sorted
        public List<BeatmapWord> words;
        
        // Getters
        public int NotesCount => words.Aggregate(0, (acc, w) => acc + w.text.Length);

        public Beatmap()
        {
            id = Guid.NewGuid();
        }
    }
}