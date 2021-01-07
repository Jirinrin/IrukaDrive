using System;
using System.IO;
using System.Threading.Tasks;
using Shared.Domain;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.Networking;

namespace Tools
{
    public static class SerializationHelpers
    {
        private static AudioType GetAudioType(string ext)
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
        
        private static byte[] LoadFile(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }
        private static async Task<byte[]> LoadFileAsync(string filePath)
        {
            // var req = new UnityWebRequest($"file://{filePath}");
            // await req.SendWebRequest();
            // return req.downloadHandler.data;
            return await Task.Run(() => File.ReadAllBytes(filePath));
        }
        
        private static AudioClip LoadSong(string filePath)
        {
            var clip = NAudioPlayer.FromMp3Data(LoadFile(filePath));
            return clip;
        }
        private static async Task<AudioClip> LoadSongAsync(string filePath)
        {
            var req = UnityWebRequestMultimedia.GetAudioClip($"file://{filePath}", GetAudioType(Path.GetExtension(filePath)));
            await req.SendWebRequest();
            var clip = DownloadHandlerAudioClip.GetContent(req);
            return clip;
        }
        
        private static Texture2D LoadImage(string filePath)
        {
            var texture = new Texture2D(2, 2);
            var f = LoadFile(filePath);
            texture.LoadImage(f);
            return texture;
        }
        private static async Task<Texture2D> LoadImageAsync(string filePath)
        {
            var texture = new Texture2D(2, 2);
            var f = await LoadFileAsync(filePath);
            texture.LoadImage(f);
            return texture;
        }

        // // todo: move somewhere else than SerializationHelpers
        // public static AudioClip FindSong()
        // {
        //     var path = EditorUtility.OpenFilePanel("Select a Song","","mp3,ogg,wav");
        //     return LoadSong(path);
        // }

        public static void SaveBeatmapAs(Beatmap beatmap, Action<string> onSuccess)
        {
            FileBrowser.ShowSaveDialog(p =>
            {
                SaveBeatmapToFile(beatmap, p[0]);
                onSuccess(p[0]);
            }, null, FileBrowser.PickMode.Files,false,
                Path.GetDirectoryName(beatmap.filePath), Path.GetFileName(beatmap.filePath), title: "Save beatmap");
        }
        public static void SaveBeatmap(Beatmap beatmap)
        {
            SaveBeatmapToFile(beatmap, beatmap.filePath);
        }

        private static void SaveBeatmapToFile(Beatmap beatmap, string path)
        {
            beatmap.SortWords();
            Serialization.WriteToXmlFile(path, beatmap);

            // todo: do some checking on overlapping words? Or do that in the editor?
        }

        public static void LoadBeatmap(Action<Beatmap> onFinished)
        {
            var dir = $"{Application.streamingAssetsPath}/DriveCharts";
            FileBrowser.ShowLoadDialog(async p => onFinished(await Shared.Cache.GetBeatmapAsync(p[0])), () => onFinished(null),
                FileBrowser.PickMode.Files,false, dir, title: "Select a drive chart"); // ext: "drive"
        }

        /// <summary>
        /// Limited to charts with MP3 song
        /// </summary>
        private static Beatmap InitBeatmap(Beatmap b, string filePath)
        {
            b.filePath = filePath;
            b.song = LoadSong(Path.Combine(Path.GetDirectoryName(filePath), b.songFile));
            b.jacket = LoadImage(Path.Combine(Path.GetDirectoryName(filePath), b.jacketFile));
            return b;
        }
        private static async Task<Beatmap> InitBeatmapAsync(Beatmap b, string filePath)
        {
            b.filePath = filePath;
            b.song = await LoadSongAsync(Path.Combine(Path.GetDirectoryName(filePath), b.songFile));
            b.jacket = await LoadImageAsync(Path.Combine(Path.GetDirectoryName(filePath), b.jacketFile));
            return b;
        }
        /// <summary>
        /// Limited to charts with MP3 song
        /// </summary>
        public static Beatmap LoadBeatmap(string filePath)
        {
            var beatmap = Serialization.ReadFromXmlFile<Beatmap>(filePath);
            return InitBeatmap(beatmap, filePath);
        }
        
        public static async Task<Beatmap> LoadBeatmapAsync(string filePath)
        {
            var beatmap = await Serialization.ReadFromXmlFileAsync<Beatmap>(filePath);
            return await InitBeatmapAsync(beatmap, filePath);
        }
    }
}
