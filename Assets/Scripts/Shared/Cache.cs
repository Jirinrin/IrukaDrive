using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Shared.Domain;
using Tools;
using UnityEngine;

namespace Shared
{
    // todo: evict old items when taking too much memory
    public static class Cache
    {
        private static readonly string BeatmapPath = $"{Application.streamingAssetsPath}/DriveCharts";

        private static readonly Dictionary<string, Beatmap> BeatmapsLookup = new Dictionary<string, Beatmap>();
        public static async Task<Beatmap> GetBeatmapAsync(string path)
        {
            if (!BeatmapsLookup.ContainsKey(path))
                BeatmapsLookup[path] = await SerializationHelpersAsync.LoadBeatmap(path, await GetSongAsync(Path.GetDirectoryName(path)));
            return BeatmapsLookup[path];
        }
        
        private static readonly Dictionary<string, Song> SongsLookup = new Dictionary<string, Song>();
        public static async Task<Song> GetSongAsync(string folderPath)
        {
            if (!SongsLookup.ContainsKey(folderPath))
                SongsLookup[folderPath] = await SerializationHelpersAsync.LoadSong(folderPath);
            return SongsLookup[folderPath];
        }

        private static readonly Dictionary<string, AudioClip> AudioLookup = new Dictionary<string, AudioClip>();
        [ItemCanBeNull]
        public static async Task<AudioClip> GetAudioAsync(string path)
        {
            if (path == null) return null;
            if (!AudioLookup.ContainsKey(path))
                AudioLookup[path] = await SerializationHelpersAsync.LoadAudio(path);
            return AudioLookup[path];
        }

        public static readonly List<Song> Songs = new List<Song>();
        public static async Task InitSongs()
        {
            if (Songs.Any())
                return;

            // todo: find nested song folders etc
            var songs = Directory.GetDirectories($"{Application.streamingAssetsPath}/DriveCharts");
            foreach (var songFolder in songs)
            {
                var songFolderPath = Path.Combine(BeatmapPath, songFolder);

                var s = await GetSongAsync(songFolderPath);

                if (!s.HasValidAudio)
                {
                    Debug.LogWarning($"Song \"{songFolderPath}\" has no (valid) audio file");
                    continue;
                }
                if (s == null)
                    continue;

                Songs.Add(s);
            }
        }
    }
}