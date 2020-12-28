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
            // GameManager.ToGameplay(SerializationHelpers.LoadBeatmap( $"{Application.streamingAssetsPath}/Beatmaps/Tutorial/easy.drive"));
            SerializationHelpers.LoadBeatmap(b => GameManager.ToGameplay(b));

        public void ToGameplay(string pathInBeatmapsFolder)
        {
            GameManager.ToGameplay(SerializationHelpers.LoadBeatmap(
                $"{Application.streamingAssetsPath}/Beatmaps/{pathInBeatmapsFolder}.drive"));
        }
    }
}
