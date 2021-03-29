using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Shared.Domain;
using Tools;
using UnityEngine;

namespace Shared
{
    // todo: Change out for proper LRU caches with a memory cap (Unity probably has something ready)
    // todo: lock when already loading something
    public static class Cache
    {
        private static readonly Dictionary<string, Beatmap> BeatmapsLookup = new Dictionary<string, Beatmap>();
        public static async Task<Beatmap> GetBeatmapAsync(string path)
        {
            if (!BeatmapsLookup.ContainsKey(path))
                BeatmapsLookup[path] = await SerializationHelpersAsync.LoadBeatmap(path, await GetSongAsync(Path.GetDirectoryName(path)));
            return BeatmapsLookup[path];
        }

        [ItemCanBeNull]
        private static async Task<T> GetAsyncLocked<T>(SemaphoreSlim lck, Func<Task<T>> getter)
        {
            await lck.WaitAsync();
            try
            {
                return await getter();
            }
            finally
            {
                lck.Release();
            }
        }
        
        private static readonly SemaphoreSlim SongLock = new SemaphoreSlim(1, 1);
        private static readonly Dictionary<string, Song> SongsLookup = new Dictionary<string, Song>();
        [ItemCanBeNull]
        public static Task<Song> GetSongAsync(string folderPath)
        {
            return GetAsyncLocked(SongLock, async () =>
            {
                if (!SongsLookup.ContainsKey(folderPath))
                    SongsLookup[folderPath] = await SerializationHelpersAsync.LoadSong(folderPath);
                return SongsLookup[folderPath];
            });
        }

        private static readonly SemaphoreSlim AudioLock = new SemaphoreSlim(1, 1);
        private static readonly Dictionary<string, AudioClip> AudioLookup = new Dictionary<string, AudioClip>();
        [ItemCanBeNull]
        public static Task<AudioClip> GetAudioAsync(string path)
        {
            if (path == null) return null;
            return GetAsyncLocked(AudioLock, async () =>
            {
                if (!AudioLookup.ContainsKey(path))
                    AudioLookup[path] = await SerializationHelpersAsync.LoadAudio(path);
                return AudioLookup[path];
            });
        }

        private static readonly SemaphoreSlim ImageLock = new SemaphoreSlim(1, 1);
        private static readonly Dictionary<string, Texture2D> ImageLookup = new Dictionary<string, Texture2D>();
        [ItemCanBeNull]
        public static Task<Texture2D> GetImageAsync(string path)
        {
            if (path == null) return null;
            return GetAsyncLocked(ImageLock, async () =>
            {
                if (!ImageLookup.ContainsKey(path))
                    ImageLookup[path] = await SerializationHelpersAsync.LoadImage(path);
                return ImageLookup[path];
            });
        }

        public static List<Song> songs = new List<Song>();
        public static async Task InitSongs()
        {
            if (songs.Any())
                return;

            await LoadSongsInFolder(C.DriveChartsDirPath);

            foreach (var (s, i) in songs.WithIndex())
                s.wheelIndex = i;
        }

        private static async Task LoadSongsInFolder(string folderPath)
        {
            if (File.Exists($"{folderPath}/song.json"))
            {
                await LoadSong(folderPath);
                return;
            }

            var dirNames = Directory.GetDirectories(folderPath);
            if (!dirNames.Any())
                Debug.LogWarning($"Folder \"{folderPath}\" is not a song and has no songs");
            else
                foreach (var d in dirNames)
                    await LoadSongsInFolder(Path.Combine(folderPath, d));
        }

        private static async Task LoadSong(string songFolderPath)
        {
            var s = await GetSongAsync(songFolderPath);

            if (s == null)
                return;
            if (!s.HasValidAudio)
            {
                Debug.LogWarning($"Song \"{songFolderPath}\" has no (valid) audio file");
                return;
            }

            songs.Add(s);
        }
    }
}