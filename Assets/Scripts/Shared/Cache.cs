using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Shared.Domain;
using Tools;
using UnityEngine;

namespace Shared
{
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

        public static readonly List<Song> Songs = new List<Song>();
        public static async Task InitSongs()
        {
            if (Songs.Any())
                return;
            
            var songs = Directory.GetDirectories($"{Application.streamingAssetsPath}/DriveCharts");
            foreach (var songFolder in songs)
            {
                var songFolderPath = Path.Combine(BeatmapPath, songFolder);

                var s = await GetSongAsync(songFolderPath);
                if (s != null)
                    Songs.Add(s);                
            }
        }
    }
}