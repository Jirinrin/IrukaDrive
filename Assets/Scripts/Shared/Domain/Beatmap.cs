using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;
using UnityEngine;

namespace Shared.Domain
{
    public enum Difficulty
    {
        Novice,
        Advanced,
        Expert,
    }
    
    [Serializable]
    public class Beatmap
    {
        // Back reference to the song it belongs to
        [NonSerialized][XmlIgnore] public Song song;
        
        // Cosmetic stuff
        [CanBeNull] public string jacketFileOverride;
        [NonSerialized][XmlIgnore] public Texture2D jacket;
        public Difficulty difficulty;
        public string creator;

        // Useful metadata
        [NonSerialized][XmlIgnore] public string filePath;
        public float? finishTimestamp; // You can specify this to have a beatmap end before the song file ends
        public Guid id; // Generated automatically by the beatmap editor

        // Notes, expected to be sorted
        public List<BeatmapWord> words;
        
        public int version = 1;
        
        // Getters
        public int NotesCount => words.Aggregate(0, (acc, w) => acc + w.text.Length);
        public float BeatsPerSec => song.BeatsPerSec;

        public Beatmap()
        {
            id = Guid.NewGuid();
        }
    }
}