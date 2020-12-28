using System;
using Shared;
using Tools;
using UnityEngine;

namespace Menu.ScreenControllers
{
    public class SongSelectScreen : MonoBehaviour
    {        
        public void ToGameplay() =>
            GameManager.ToGameplay(SerializationHelpers.LoadBeatmap(
                // Uncomment this for easy dev
                // Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Documents\Unity\IrukaDrive\Assets\Beatmaps\Tutorial\easy.drive")
            ));

        public void ToGameplay(string pathInBeatmapsFolder)
        {
            GameManager.ToGameplay(SerializationHelpers.LoadBeatmap(
                // todo: figure out path in actual installation
                Environment.ExpandEnvironmentVariables($@"%USERPROFILE%\Documents\Unity\IrukaDrive\Assets\Beatmaps\{pathInBeatmapsFolder}.drive")
            ));
        }
    }
}
