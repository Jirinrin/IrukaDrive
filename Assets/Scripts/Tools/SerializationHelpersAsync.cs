using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        
        private static async Task<AudioClip> LoadAudio(string filePath)
        {
            var req = UnityWebRequestMultimedia.GetAudioClip($"file://{filePath}", SerializationHelperUtils.GetAudioType(Path.GetExtension(filePath)));
            await req.SendWebRequest();
            var clip = DownloadHandlerAudioClip.GetContent(req);
            return clip;
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
            var dir = $"{Application.streamingAssetsPath}/DriveCharts";
            FileBrowser.ShowLoadDialog(async p =>
                onFinished(p[0].EndsWith(".drive")
                    ? cache
                        ? await Cache.GetBeatmapAsync(p[0])
                        : await LoadBeatmap(p[0], await LoadSong(Path.GetDirectoryName(p[0])))
                    : null),
                () => onFinished(null),
                FileBrowser.PickMode.Files,false, dir, title: "Select a drive chart"); // ext: "drive"
        }

        public static async Task<Beatmap> LoadBeatmap(string filePath, Song song)
        {
            var b = await Serialization.ReadFromJsonFileAsync<Beatmap>(filePath);

            b.song = song;
            b.filePath = filePath;
            b.jacket = b.jacketFileOverride != null
                ? await LoadImage(Path.Combine(Path.GetDirectoryName(filePath)!, b.jacketFileOverride)) 
                : b.song.jacket;

            return b;
        }
        
        // SONG STUFF

        public static async Task<Song> LoadSong(string folderPath)
        {
            var (diffs, ok) = SerializationHelperUtils.CheckSong(folderPath);
            if (!ok) return null;
            
            var songFilePath = Path.Combine(folderPath, "song.json");
            var s = await Serialization.ReadFromJsonFileAsync<Song>(songFilePath);
            s.filePath = songFilePath;
            s.folderPath = folderPath;
            s.diffs = diffs;
            
            if (s.audioPath != null)
                s.audio = await LoadAudio(Path.Combine(folderPath, s.audioPath));            
            if (s.jacketPath != null)
                s.jacket = await LoadImage(Path.Combine(folderPath, s.jacketPath));
            
            return s;
        }
    }
}
