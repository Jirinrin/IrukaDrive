using System;
using System.Xml.Serialization;
using JetBrains.Annotations;
using UnityEngine;

namespace Shared.Domain
{
    public class Song
    {
        public string title;
        public string artist;
        public string jacketPath;
        [NonSerialized][XmlIgnore] [CanBeNull] public Texture2D jacket;
        
        public string audioPath;
        [NonSerialized][XmlIgnore] [CanBeNull] public AudioClip audio;
        
        public float bpm;
        public float beatOffset;
        [Range(2,4)] public int beatsPerBar = 4;
        public int barOffset;
        
        [NonSerialized][XmlIgnore] [NotNull] public string folderPath;
        [NonSerialized][XmlIgnore] [NotNull] public string filePath;
        [NonSerialized][XmlIgnore] public string[] diffPaths; // Max 4?
        
        private float? _beatsPerSec;
        public float BeatsPerSec => _beatsPerSec ??= bpm / 60f;
        
        public int version;
    }
}