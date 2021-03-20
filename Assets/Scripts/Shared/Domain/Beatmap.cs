using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
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
        public float? finishTimestamp; // You can specify this to have a beatmap end before the song file ends

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
            words = new List<BeatmapWord>();
        }

        public Beatmap(string filePath, Song s) : this()
        {
            this.filePath = filePath;
            song = s;
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
        // Back reference to the song it belongs to
        [NonSerialized][IgnoreDataMember] public Song song;

        [NonSerialized][IgnoreDataMember] public string filePath;

        public string creator = "";
        public Difficulty difficulty;
        [CanBeNull] public string difficultyNameOverride;
        [CanBeNull] public string jacketFileOverride;

        [IgnoreDataMember] public string DifficultyName => difficultyNameOverride ?? difficulty.ToString();

        [IgnoreDataMember] [ItemCanBeNull]
        public Task<Texture2D> Jacket =>
            jacketFileOverride != null
                ? Cache.GetImageAsync(Path.Combine(song.folderPath, jacketFileOverride))
                : song.Jacket;

        public int CompareTo(SongDifficulty other) => difficulty.CompareTo(other.difficulty);

        public Guid id; // Generated automatically by the beatmap editor
    }
}