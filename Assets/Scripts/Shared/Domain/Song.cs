using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using UnityEngine;

namespace Shared.Domain
{
    [Serializable]
    public class Song
    {
        public string title;
        public string artist;
        public string jacketPath;
        [NonSerialized][IgnoreDataMember] [CanBeNull] public Texture2D jacket;
        
        public string audioPath;
        [NonSerialized][IgnoreDataMember] [CanBeNull] public AudioClip audio;
        
        public float bpm;
        public float beatOffset;
        [Range(2,4)] public int beatsPerBar = 4;
        public int barOffset;
        
        [NonSerialized][IgnoreDataMember] [NotNull] public string folderPath;
        [NonSerialized][IgnoreDataMember] [NotNull] public string filePath;
        [NonSerialized][IgnoreDataMember] [NotNull] public SongDifficulty[] diffs; // Max 4?

        private float? _beatsPerSec;
        [IgnoreDataMember] public float BeatsPerSec => _beatsPerSec ??= bpm / 60f;
        
        public int version;
    }
}