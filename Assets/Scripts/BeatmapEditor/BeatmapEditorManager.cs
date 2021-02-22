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

        private bool _isFunctional;
        private void SetFunctional(bool isFunctional)
        {
            if (_isFunctional == isFunctional)
                return;
            _isFunctional = isFunctional;
            EditorTrackGestures.Instance.enabled = isFunctional;
        }

        public void PlayBeatmapFrom(float beatTime, bool autoplay = true)
        {
            // todo: somehow keep this scene loaded in background
            _inEditorPlay = true;
            GameManager.ToGameplay(currentBeatmap, currentBeatmap.BeatsToSecs(beatTime), autoplay);
        }

        private void Start()
        {
            SetFunctional(true);
            // For dev
            if (GameManager.State != GameState.BeatmapEditor)
            {
                GameManager.SetState(GameState.BeatmapEditor);
                // Uncomment this for easy iterating
                currentBeatmap = SerializationHelpers.LoadBeatmap($"{Application.streamingAssetsPath}/DriveCharts/SDVX Tutorial/2_advanced.drive");
                EditorTrack.Instance.LoadBeatmap(currentBeatmap);
            }
            else if (_inEditorPlay)
            {
                _inEditorPlay = false;
                EditorTrack.Instance.LoadBeatmap(currentBeatmap, true);
            }
            else if (currentBeatmap != null)
                ReloadBeatmap();
            else
                SetFunctional(false);
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
        
        // Called from Load button
        public void LoadBeatmap()
        {
            if (currentBeatmap != null)
                ResetEditor();
            
            SerializationHelpers.LoadBeatmap(b =>
            {
                currentBeatmap = b ?? currentBeatmap;
                EditorTrack.Instance.LoadBeatmap(currentBeatmap);
                SetFunctional(true);
            });
        }

        private async void ReloadBeatmap()
        {
            currentBeatmap = await SerializationHelpers.LoadBeatmapAsync(currentBeatmap.filePath);
            ResetEditor();
            EditorTrack.Instance.LoadBeatmap(currentBeatmap, true);
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

        private void OnEnable()
        {
            EditorInputManager.Save += SaveBeatmap;
        }
        private void OnDisable()
        {
            EditorInputManager.Save -= SaveBeatmap;
        }
    }
}
