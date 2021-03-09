using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Shared.Domain;
using UnityEngine;

namespace Tools
{
    public static class SerializationHelperUtils
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

        public static readonly Regex ImageExtensionRegex = new Regex(@"^(?:jpg|png)$");

        public static (SongDifficulty[] diffPaths, bool ok) CheckSong(string folderPath)
        {
            var songFilePath = Path.Combine(folderPath, "song.json");
            if (!File.Exists(songFilePath))
            {
                Debug.LogWarning($"Song {folderPath} has no song.json");
                return (null, false);
            }

            var diffPaths = Directory.GetFiles(folderPath, "*.drive").Select(d => Path.Combine(folderPath, d)).ToArray();
            if (!diffPaths.Any())
            {
                Debug.LogWarning($"Song {folderPath} has no diffs");
                return (null, false);
            }

            var diffs = diffPaths.Select(path =>
            {
                // We have to go from Beatmap to SongDifficulty, because otherwise it will fail when trying to parse
                // the `words` field when encountering a `\\` inside of a string (then it expects ',' for some reason,
                // this is probably a bu\g in Utf8Json).
                // => todo: better way to deal with this deserialization, ideally stop the deserialization before coming to the words field.
                // e.g. look at the QuaternionFormatter how it custom works with fields. (also a Beatmap custom serializer could be nice, putting words at the very bottom)
                // or could just make a simple (?) matcher on a couple byte sequences for the 2 properties we need
                var diff = Serialization.ReadFromJsonFile<Beatmap>(path);
                diff.filePath = path;
                return (SongDifficulty) diff;
            }).ToArray();
            Array.Sort(diffs);

            return (diffs, true);
        }
    }
}