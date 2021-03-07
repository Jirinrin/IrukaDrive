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
        
        private static AudioClip LoadAudio(string filePath)
        {
            var clip = NAudioPlayer.FromMp3Data(LoadFile(filePath));
            return clip;
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
            Serialization.WriteToJsonFile(path, beatmap);

            // todo: do some checking on overlapping words? Or do that in the editor?
        }
        
        /// <summary>
        /// Limited to charts with MP3 song
        /// </summary>
        public static Beatmap LoadBeatmap(string filePath, Song song = null)
        {
            var b = Serialization.ReadFromJsonFile<Beatmap>(filePath);
            var dir = Path.GetDirectoryName(filePath);
                
            b.song = song ?? LoadSong(dir);
            b.filePath = filePath;
            b.jacket = b.jacketFileOverride != null
                ? LoadImage(Path.Combine(dir!, b.jacketFileOverride)) 
                : b.song.jacket;

            return b;
        }

        // SONG STUFF

        public static void NewSong(Beatmap beatmap, Action<string> onSuccess)
        {
            FileBrowser.ShowSaveDialog(p =>
                {
                    var folder = p[0];
                    if (Directory.Exists(folder))
                    {
                        Debug.LogWarning($"{folder} already exists");
                        return;
                    }
                    Directory.CreateDirectory(folder);
                    var s = new Song
                    {
                        folderPath = folder,
                        filePath = Path.Combine(folder, "song.xml"),
                    };
                    SaveSong(s);
                    SaveBeatmapToFile(new Beatmap(), Path.Combine(folder, "beginner.drive"));

                }, null, FileBrowser.PickMode.Folders,false,
                Path.GetDirectoryName(beatmap.filePath), Path.GetFileName(beatmap.filePath), "Save beatmap");
        }
        public static void SaveSong(Song song)
        {
            Serialization.WriteToJsonFile(song.filePath, song);
        }

        public static Song LoadSong(string folderPath)
        {
            var (diffs, ok) = SerializationHelperUtils.CheckSong(folderPath);
            if (!ok) return null;

            // todo: song in json
            var songFilePath = Path.Combine(folderPath, "song.xml");
            var s = Serialization.ReadFromXmlFile<Song>(songFilePath);
            s.filePath = songFilePath;
            s.folderPath = folderPath;
            s.diffPaths = diffs;
            
            s.audio = LoadAudio(Path.Combine(folderPath, s.audioPath));
            s.jacket = LoadImage(Path.Combine(folderPath, s.jacketPath));

            return s;
        }
    }
}
