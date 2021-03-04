using System.IO;
using System.Linq;
using UnityEngine;

namespace Tools
{
    public class SerializationHelperUtils
    {
        public static AudioType GetAudioType(string ext)
        {
            switch (ext.Substring(1).ToLower())
            {
                case "mp3":
                case "m4a":
                    return AudioType.MPEG;
                case "ogg":
                    return AudioType.OGGVORBIS;
                case "wav":
                    return AudioType.WAV;
                case "aiff":
                    return AudioType.AIFF;
                default:
                    Debug.LogWarning($"Unknown audio type: {ext}");
                    return AudioType.UNKNOWN;
            }
        }

        public static (string[] diffPaths, bool ok) CheckSong(string folderPath)
        {
            var songFilePath = Path.Combine(folderPath, "song.xml");
            if (!File.Exists(songFilePath))
            {
                Debug.LogWarning($"Song {folderPath} has no song.xml");
                return (null, false);
            }
            
            var diffPaths = Directory.GetFiles(folderPath, "*.drive").Select(d => Path.Combine(folderPath, d)).ToArray();
            if (!diffPaths.Any())
            {
                Debug.LogWarning($"Song {folderPath} has no diffs");
                return (null, false);
            }

            return (diffPaths, true);
        }
    }
}