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
            var fileRequest = new WWW($"file://{filePath}");
            // yield return www;
        
            // todo: use UnityWebRequest; use www.GetAudioClip or UnityWebRequestMultimedia.GetAudioClip
            return fileRequest.bytes;
        }
        
        private static AudioClip LoadSong(string filePath)
        {
            var clip = NAudioPlayer.FromMp3Data(LoadFile(filePath));
            return clip;
        }
        
        private static Texture2D LoadImage(string filePath)
        {
            var texture = new Texture2D(2, 2); // why only 2x2 tho...?
            texture.LoadImage(LoadFile(filePath));
            return texture;
        }

        // got this from stackoverflow https://stackoverflow.com/questions/30852691/loading-mp3-files-at-runtime-in-unity
        // private static AudioClip LoadSongg(string path) 
        // {
        //     var www = new WWW("file://" + path);
        //     // yield return www;
        //     
        //     var clip = www.GetAudioClip(false, false);
        //     
        //     var songName = clip.name;
        //     var length = clip.length;
        //     Debug.Log(songName);
        //     Debug.Log(length);
        //     return clip;
        // }

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
            var dir = $"{Application.streamingAssetsPath}/Beatmaps";
            FileBrowser.ShowLoadDialog(p => onFinished(LoadBeatmap(p[0])), () => onFinished(null),
                FileBrowser.PickMode.Files,false, dir, title: "Select a drive chart"); // ext: "drive"
        }
        public static Beatmap LoadBeatmap(string filePath)
        {
            // filePath = filePath.Replace('/', Path.DirectorySeparatorChar);
            var beatmap = Serialization.ReadFromXmlFile<Beatmap>(filePath);
            beatmap.filePath = filePath;
            beatmap.song = LoadSong(Path.Combine(Path.GetDirectoryName(filePath), beatmap.songFile));
            beatmap.jacket = LoadImage(Path.Combine(Path.GetDirectoryName(filePath), beatmap.jacketFile));
            return beatmap;
        }
    }
}
