using System;
using Shared;
using Tools;
using UnityEngine;

namespace Menu.ScreenControllers
{
    public class SongSelectScreen : MonoBehaviour
    {        
        public void ToGameplay() =>
            // Use this for easy dev
            // GameManager.ToGameplay(SerializationHelpers.LoadBeatmap( Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Documents\Unity\IrukaDrive\Assets\Beatmaps\Tutorial\easy.drive")));
            SerializationHelpers.LoadBeatmap(b => GameManager.ToGameplay(b));

        public void ToGameplay(string pathInBeatmapsFolder)
        {
            GameManager.ToGameplay(SerializationHelpers.LoadBeatmap(
                // todo: figure out path in actual installation
                Environment.ExpandEnvironmentVariables($@"%USERPROFILE%\Documents\Unity\IrukaDrive\Assets\Beatmaps\{pathInBeatmapsFolder}.drive")
            ));
        }
    }
}
