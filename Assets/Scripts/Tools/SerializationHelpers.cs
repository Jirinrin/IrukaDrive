using System;
using System.IO;
using Shared.Domain;
using SimpleFileBrowser;
using UnityEngine;

namespace Tools
{
    public static class SerializationHelpers
    {
        private static byte[] LoadFile(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }
        
        private static Texture2D LoadImage(string filePath)
        {
            var texture = new Texture2D(2, 2);
            var f = LoadFile(filePath);
            texture.LoadImage(f);
            return texture;
        }

        // // todo: move somewhere else than SerializationHelpers
        // public static AudioClip FindSong()
        // {
        //     var path = EditorUtility.OpenFilePanel("Select a Song","","mp3,ogg,wav");
        //     return LoadSong(path);
        // }
        
        // BEATMAP STUFF

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
            var valRes = BeatmapValidation.ValidateBeatmap(beatmap);
            OnBeatmapValidation?.Invoke(valRes);
            if (valRes.IsValid)
                Serialization.WriteToJsonFile(path, beatmap);
        }
        
        /// <summary>
        /// Limited to charts with MP3 song
        /// </summary>
        public static Beatmap LoadBeatmap(string filePath, Song song = null)
        {
            var b = Serialization.ReadFromJsonFile<Beatmap>(filePath);
            b.song = song ?? LoadSong(Path.GetDirectoryName(filePath));
            b.filePath = filePath;

            return b;
        }

        // SONG STUFF

        public static void SaveSong(Song song)
        {
            Serialization.WriteToJsonFile(song.filePath, song);
        }

        public static Song LoadSong(string folderPath)
        {
            var (diffs, ok) = SerializationHelperUtils.CheckSong(folderPath);
            if (!ok) return null;

            var songFilePath = Path.Combine(folderPath, "song.json");
            var s = Serialization.ReadFromJsonFile<Song>(songFilePath);
            s.filePath = songFilePath;
            s.folderPath = folderPath;
            s.diffs = diffs;

            foreach (var d in s.diffs)
                d.song = s;

            return s;
        }

        public static event Action<BeatmapValidation.ValidationResult> OnBeatmapValidation;
    }
}
