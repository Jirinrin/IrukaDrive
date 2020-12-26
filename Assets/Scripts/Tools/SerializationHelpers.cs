using System;
using System.IO;
using System.Linq;
using Gameplay;
using JetBrains.Annotations;
using Shared.Domain;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    public static class SerializationHelpers
    {
        private static AudioClip LoadSong(string filePath)
        {
            Debug.Log("load song!!");
            var fileRequest = new WWW($"file://{filePath}");
            // yield return www;
        
            // todo: use UnityWebRequest; use www.GetAudioClip or UnityWebRequestMultimedia.GetAudioClip

            Debug.Log("did request!!");

            var clip = NAudioPlayer.FromMp3Data(fileRequest.bytes);
            return clip;
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

        // todo: move somewhere else than SerializationHelpers
        public static AudioClip FindSong()
        {
            var path = EditorUtility.OpenFilePanel("Select a Song","","mp3,ogg,wav");
            return LoadSong(path);
        }

        public static void SaveBeatmapAs(Beatmap beatmap)
        {
            var path = EditorUtility.SaveFilePanel("Save beatmap", Path.GetDirectoryName(beatmap.filePath),
                Path.GetFileName(beatmap.filePath), "drive");
            SaveBeatmapToFile(beatmap, path);
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

        [CanBeNull]
        public static Beatmap LoadBeatmap()
        {
            var beatmapPath = EditorUtility.OpenFilePanel("Select a Beatmap","","drive");
            return beatmapPath.Length == 0 
                ? null 
                : LoadBeatmap(beatmapPath);
        }
        public static Beatmap LoadBeatmap(string filePath)
        {
            // filePath = filePath.Replace('/', Path.DirectorySeparatorChar);
        
            var beatmap = Serialization.ReadFromXmlFile<Beatmap>(filePath);
            beatmap.filePath = filePath;
            var songPath = Path.GetDirectoryName(filePath) + Path.DirectorySeparatorChar + beatmap.songFile;
            beatmap.song = LoadSong(songPath);
            return beatmap;
        }
    }
}
