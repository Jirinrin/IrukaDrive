using System;
using BeatmapEditor.SingletonComponents;
using Shared;
using Shared.Domain;
using Tools;
using Tools.Commons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatmapEditor
{
    public class BeatmapEditorManager : Singleton<BeatmapEditorManager>
    {
        public static Beatmap currentBeatmap;

        private static bool _inEditorPlay;

        public void PlayBeatmapFrom(float beatTime, bool autoplay = true)
        {
            // todo: somehow keep this scene loaded in background
            _inEditorPlay = true;
            GameManager.ToGameplay(currentBeatmap, currentBeatmap.BeatsToSecs(beatTime), autoplay);
        }

        private void Start()
        {
            // For dev
            if (GameManager.State != GameState.BeatmapEditor)
            {
                GameManager.SetState(GameState.BeatmapEditor);
                // Uncomment this for easy iterating
                currentBeatmap = SerializationHelpers.LoadBeatmap($"{Application.streamingAssetsPath}/Beatmaps/Tutorial/easy.drive");
                EditorTrack.Instance.InitTrack(currentBeatmap);
            }
            else if (_inEditorPlay)
            {
                _inEditorPlay = false;
                EditorTrack.Instance.InitTrack(currentBeatmap, true);
            }
            else if (currentBeatmap != null)
                ReloadBeatmap();
        }

        public void SaveBeatmap()
        {
            SerializationHelpers.SaveBeatmap(currentBeatmap);
            // todo: display some message that it succeeded
        }
        public void SaveBeatmapAs()
        {
            SerializationHelpers.SaveBeatmapAs(currentBeatmap, path => currentBeatmap.filePath = path ?? currentBeatmap.filePath);
            // todo: display some message that it succeeded
        }
        
        public void LoadBeatmap()
        {
            if (currentBeatmap != null)
                ResetEditor();
            
            SerializationHelpers.LoadBeatmap(b =>
            {
                currentBeatmap = b ?? currentBeatmap;
                EditorTrack.Instance.InitTrack(currentBeatmap);
            });
        }

        private void ReloadBeatmap()
        {
            currentBeatmap = SerializationHelpers.LoadBeatmap(currentBeatmap.filePath);
            ResetEditor();
            EditorTrack.Instance.InitTrack(currentBeatmap);
        }

        private void ResetEditor()
        {
            EditorTrack.Instance.ResetTrack();
        }

        public void PlaySong()
        {
            if (!SongManager.Instance.IsPlaying)
                SongManager.Instance.LoadSong(currentBeatmap, tickOnBeat: true);
            else
                SongManager.Instance.Stop();
        }

        public void BackToMainMenu() => GameManager.ToMainMenu();
    }
}
