using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Shared;
using Shared.Domain;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.Networking;
using Cache = Shared.Cache;

namespace Tools
{
    public static class SerializationHelpersAsync
    {
        private static async Task<byte[]> LoadFile(string filePath)
        {
            // var req = new UnityWebRequest($"file://{filePath}");
            // await req.SendWebRequest();
            // return req.downloadHandler.data;
            return await Task.Run(() => File.ReadAllBytes(filePath));
        }
        
        [ItemCanBeNull]
        public static async Task<AudioClip> LoadAudio(string filePath)
        {
            try
            {
                var req = UnityWebRequestMultimedia.GetAudioClip($"file://{filePath}",
                    SerializationHelperUtils.GetAudioType(Path.GetExtension(filePath)));
                await req.SendWebRequest();
                var clip = DownloadHandlerAudioClip.GetContent(req);
                return clip;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return null;
            }
        }
        
        private static async Task<Texture2D> LoadImage(string filePath)
        {
            var texture = new Texture2D(2, 2);
            var f = await LoadFile(filePath);
            texture.LoadImage(f);
            return texture;
        }
        
        // BEATMAP STUFF

        public static void LoadSelectBeatmap(Action<Beatmap> onFinished, bool cache = false)
        {
            FileBrowser.ShowLoadDialog(async p =>
                onFinished(p[0].EndsWith(".drive")
                    ? cache
                        ? await Cache.GetBeatmapAsync(p[0])
                        : await LoadBeatmap(p[0], await LoadSong(Path.GetDirectoryName(p[0])))
                    : null),
                () => onFinished(null),
                FileBrowser.PickMode.Files,false, C.DriveChartsDirPath, title: "Select a drive chart"); // ext: "drive"
        }

        public static async Task<Beatmap> LoadBeatmap(string filePath, Song song)
        {
            var b = await Serialization.ReadFromJsonFileAsync<Beatmap>(filePath);

            b.song = song;
            b.filePath = filePath;

            var jacketFileOverridePath = b.jacketFileOverride == null ? null : Path.Combine(Path.GetDirectoryName(filePath)!, b.jacketFileOverride);
            b.jacket = jacketFileOverridePath != null && File.Exists(jacketFileOverridePath)
                ? await LoadImage(jacketFileOverridePath)
                : b.song.jacket;

            return b;
        }
        
        // SONG STUFF

        [ItemCanBeNull]
        public static async Task<Song> LoadSong(string folderPath)
        {
            var (diffs, ok) = SerializationHelperUtils.CheckSong(folderPath);
            if (!ok) return null;
            
            var songFilePath = Path.Combine(folderPath, "song.json");
            var s = await Serialization.ReadFromJsonFileAsync<Song>(songFilePath);
            s.filePath = songFilePath;
            s.folderPath = folderPath;
            s.diffs = diffs;

            var jacketPath = Path.Combine(folderPath, s.jacketFile);
            if (s.jacketFile != null && File.Exists(jacketPath))
                s.jacket = await LoadImage(jacketPath);
            else
                Debug.LogWarning($"Song \"{folderPath}\" has no (valid) jacket file");

            return s;
        }

        public static void NewSong(Action<Beatmap> onFinished)
        {
            FileBrowser.ShowSaveDialog(p =>
                {
                    var folder = p[0];
                    if (Directory.Exists(folder))
                    {
                        Debug.LogWarning($"{folder} already exists");
                        onFinished(null);
                    }
                    Directory.CreateDirectory(folder);
                    var s = new Song(folder);
                    SerializationHelpers.SaveSong(s);
                    var b = new Beatmap(Path.Combine(folder, "beginner.drive"), s);
                    SerializationHelpers.SaveBeatmap(b);
                    onFinished(b);
                },
                () => onFinished(null), FileBrowser.PickMode.Files,false,
                C.DriveChartsDirPath, "NewSong", "Create song");
        }

        public static void NewBeatmap(Song song, Action<Beatmap> onFinished)
        {
            if (song == null)
                return;

            FileBrowser.ShowSaveDialog(p =>
                {
                    var fileName = p[0];
                    if (!fileName.EndsWith(".drive"))
                        fileName += ".drive";

                    if (File.Exists(fileName))
                    {
                        Debug.LogWarning($"{fileName} already exists");
                        onFinished(null);
                    }

                    var b = new Beatmap(Path.Combine(song.folderPath, fileName), song);
                    SerializationHelpers.SaveBeatmap(b);
                    onFinished(b);
                }, () => onFinished(null),
                FileBrowser.PickMode.Files,false,
                song.folderPath, "newchart", "Create drive chart");
        }
    }
}
