using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Shared.Domain
{
    [Serializable]
    public class Song
    {
        public string title = "";
        public string artist = "";

        public string jacketFile;
        [IgnoreDataMember] [ItemCanBeNull]
        public Task<Texture2D> Jacket =>
            Cache.GetImageAsync(jacketFile == null ? null : Path.Combine(folderPath, jacketFile));

        public string audioFile;
        [IgnoreDataMember] [ItemCanBeNull]
        public Task<AudioClip> Audio =>
            Cache.GetAudioAsync(audioFile == null ? null : Path.Combine(folderPath, audioFile));
        
        public float bpm = 120f;
        public float beatOffset;
        [Range(2,4)] public int beatsPerBar = 4;
        [Range(0,3)] public int barOffset;
        
        [NonSerialized][IgnoreDataMember] [NotNull] public string folderPath;
        [NonSerialized][IgnoreDataMember] [NotNull] public string filePath;
        [NonSerialized][IgnoreDataMember] [NotNull] public SongDifficulty[] diffs = new SongDifficulty[0]; // Max 4?

        [IgnoreDataMember] public float BeatsPerSec => bpm / 60f;
        [IgnoreDataMember] public bool HasValidAudio => audioFile != null && File.Exists(Path.Combine(folderPath, audioFile));

        [NonSerialized] public int wheelIndex;

        public int version = 1;

        public Song()
        {
            folderPath = "";
            filePath = "";
        }
        public Song(string folderPath) : this()
        {
            this.folderPath = folderPath;
            filePath = Path.Combine(folderPath, "song.json");
        }
    }
}