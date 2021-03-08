using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Tools;
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
    public class Beatmap : SongDifficulty
    {
        // Back reference to the song it belongs to
        [NonSerialized][IgnoreDataMember] public Song song;
        
        // Cosmetic stuff
        [CanBeNull] public string jacketFileOverride;
        [NonSerialized][IgnoreDataMember] public Texture2D jacket;

        // Useful metadata
        public float? finishTimestamp; // You can specify this to have a beatmap end before the song file ends
        public Guid id; // Generated automatically by the beatmap editor

        // Notes, expected to be sorted
        public List<BeatmapWord> words;

        public int version = 1;
        
        // Getters
        [IgnoreDataMember] public int NotesCount => words.Aggregate(0, (acc, w) => acc + w.text.Length);
        [IgnoreDataMember] public float BeatsPerSec => song.BeatsPerSec;
        [IgnoreDataMember] public float LastBeat => words.Last().LastBeat();

        public Beatmap()
        {
            id = Guid.NewGuid();
        }

        // Methods
        public Beatmap CloneState()
        {
            var clone = (Beatmap) MemberwiseClone();
            clone.words = words.Select(w => w.Clone()).ToList();
            return clone;
        }
        public void SortWords() => words = words.OrderBy(word => word.beat).ToList();
    }

    [Serializable]
    public class SongDifficulty : IComparable<SongDifficulty>
    {
        [NonSerialized][IgnoreDataMember] public string filePath;

        public string creator;
        public Difficulty difficulty;
        [CanBeNull] public string difficultyNameOverride;

        [IgnoreDataMember] public string DifficultyName => difficultyNameOverride ?? difficulty.ToString();
        public int CompareTo(SongDifficulty other) => difficulty.CompareTo(other.difficulty);
    }
}