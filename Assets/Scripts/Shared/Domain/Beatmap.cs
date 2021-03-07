using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
        [NonSerialized][IgnoreDataMember] public Song song;
        
        // Cosmetic stuff
        public Difficulty difficulty;
        public string creator;
        [CanBeNull] public string jacketFileOverride;
        [NonSerialized][IgnoreDataMember] public Texture2D jacket;

        // Useful metadata
        [NonSerialized][IgnoreDataMember] public string filePath;
        public float? finishTimestamp; // You can specify this to have a beatmap end before the song file ends
        public Guid id; // Generated automatically by the beatmap editor

        // Notes, expected to be sorted
        public List<BeatmapWord> words;
        
        public int version = 1;
        
        // Getters
        [IgnoreDataMember] public int NotesCount => words.Aggregate(0, (acc, w) => acc + w.text.Length);
        [IgnoreDataMember] public float BeatsPerSec => song.BeatsPerSec;

        public Beatmap()
        {
            id = Guid.NewGuid();
        }

        // ReSharper disable IdentifierTypo
        public bool ShouldSerializejacketFileOverride() => jacketFileOverride != null;
    }
}