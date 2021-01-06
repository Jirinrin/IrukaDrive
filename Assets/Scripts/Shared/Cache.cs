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
        private static readonly string BeatmapPath = $"{Application.streamingAssetsPath}/Beatmaps";

        private static readonly Dictionary<string, Beatmap> Beatmaps = new Dictionary<string, Beatmap>();
        public static Beatmap GetBeatmap(string path)
        {
            if (!Beatmaps.ContainsKey(path))
                Beatmaps[path] = SerializationHelpers.LoadBeatmap(path);
            return Beatmaps[path];
        }
        public static async Task<Beatmap> GetBeatmapAsync(string path)
        {
            if (!Beatmaps.ContainsKey(path))
                Beatmaps[path] = await SerializationHelpers.LoadBeatmapAsync(path);
            return Beatmaps[path];
        }
        
        public static readonly List<Song> Songs = new List<Song>();
        // todo: allow nested beatmaps and stuff
        // todo: think about the right structure. Which data per chart and which maybe in a shared thing? Which data do we want to know in song select already?
        public static void InitSongs()
        {
            if (Songs.Any())
                return;
            
            var songs = Directory.GetDirectories($"{Application.streamingAssetsPath}/Beatmaps");
            foreach (var songFolder in songs)
            {
                var songPath = Path.Combine(BeatmapPath, songFolder);
                var diffPaths = Directory.GetFiles(songPath, "*.drive").Select(d => Path.Combine(songPath, d)).ToArray();
                if (!diffPaths.Any())
                {
                    Debug.LogWarning($"Song {songPath} has no diffs");
                    continue;
                }

                var firstBeatmap = SerializationHelpers.LoadBeatmap(diffPaths.First());
                Songs.Add(new Song
                {
                    title = firstBeatmap.title,
                    artist = firstBeatmap.artist,
                    jacket = firstBeatmap.jacket,
                    song = firstBeatmap.song,
                    folderName = songFolder,
                    folderPath = songPath,
                    diffPaths = diffPaths,
                });
            }
        }

    }
}