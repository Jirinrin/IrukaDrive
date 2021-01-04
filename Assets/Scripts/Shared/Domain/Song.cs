using UnityEngine;

namespace Shared.Domain
{
    public class Song
    {
        public string title;
        public string artist;
        public Texture2D jacket;
        public AudioClip song;
        public string folderPath;
        public string folderName;

        // Max 4
        public string[] diffPaths;
    }
}