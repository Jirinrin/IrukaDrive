using System;
using BeatmapEditor.Domain;
using BeatmapEditor.SingletonComponents;
using Shared;
using Shared.Domain;
using TMPro;
using Tools;
using Tools.Commons;
using UnityEngine;

namespace BeatmapEditor
{
    public class BeatmapEditorManager : Singleton<BeatmapEditorManager>
    {
        [SerializeField] private TextMeshProUGUI listenSongBtnText;

        public static Song currentSong;
        public static Beatmap currentBeatmap;

        private static bool _inEditorPlay;

        private bool _isFunctional;
        private void SetFunctional(bool isFunctional)
        {
            if (_isFunctional == isFunctional)
                return;
            _isFunctional = isFunctional;
            EditorInputManager.Instance.enabled = isFunctional;
        }

        public void PlayBeatmapFrom(float beatTime, bool autoplay = true)
        {
            if (!currentSong.HasValidAudio)
                return;
            // todo: somehow keep this scene loaded in background...?
            _inEditorPlay = true;
            GameManager.ToGameplay(currentBeatmap, currentSong.BeatsToSecs(beatTime), autoplay);
        }

        private static async void LoadDummyBeatmapAsync()
        {
            var dir = $"{Application.streamingAssetsPath}/DriveCharts/Yuusha no Natsuyasumi";
            currentSong = await SerializationHelpersAsync.LoadSong(dir);
            currentBeatmap = await SerializationHelpersAsync.LoadBeatmap($"{dir}/advanced.drive", currentSong);
            EditorTrack.Instance.LoadBeatmap(currentBeatmap);
            EditorHistory.Reset();
        }

        private void Start()
        {
            SetFunctional(true);
            if (GameManager.State != GameState.BeatmapEditor)
            {
                // For dev
                GameManager.SetState(GameState.BeatmapEditor);
                LoadDummyBeatmapAsync();
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
            SerializationHelpers.SaveSong(currentSong);
            // todo: display some message that it succeeded
        }
        public void SaveBeatmapAs()
        {
            SerializationHelpers.SaveBeatmapAs(currentBeatmap, path => currentBeatmap.filePath = path ?? currentBeatmap.filePath);
            // todo: display some message that it succeeded
        }
        
        // Called from Load button
        public void LoadBeatmap() => WithLoadBeatmap(b => SerializationHelpersAsync.LoadSelectBeatmap(b));
        public void NewSong() => WithLoadBeatmap(SerializationHelpersAsync.NewSong);
        public void NewBeatmap() => WithLoadBeatmap(b => SerializationHelpersAsync.NewBeatmap(currentSong, b));

        private void WithLoadBeatmap(Action<Action<Beatmap>> loader)
        {
            EditorInputManager.Instance.enabled = false;
            loader(b =>
            {
                OnLoadBeatmap(b);
                EditorInputManager.Instance.enabled = true;
            });
        }
        private void OnLoadBeatmap(Beatmap b)
        {
            if (currentBeatmap != null)
                ResetEditor();

            currentBeatmap = b ?? currentBeatmap;
            currentSong = currentBeatmap.song;
            EditorTrack.Instance.LoadBeatmap(currentBeatmap);
            SetFunctional(true);
        }

        private async void ReloadBeatmap()
        {
            // todo: also reload song?
            currentBeatmap = await SerializationHelpersAsync.LoadBeatmap(currentBeatmap.filePath, currentSong);
            RefreshBeatmap();
        }

        public void RefreshBeatmap()
        {
            ResetEditor();
            EditorTrack.Instance.LoadBeatmap(currentBeatmap, true);
        }

        private void ResetEditor()
        {
            EditorTrack.Instance.ResetTrack();
            EditorHistory.Reset();
        }

        public async void ListenSong()
        {
            if (!SongManager.Instance.IsPlaying)
            {
                if (!currentSong.HasValidAudio)
                {
                    Debug.LogWarning("Please add a valid audio file");
                    return;
                }
                await SongManager.Instance.LoadSong(currentSong, tickOnBeat: true);
                listenSongBtnText.text = "Stop";
            }
            else
            {
                SongManager.Instance.Stop();
                listenSongBtnText.text = "Listen";
            }
        }

        public void BackToMainMenu() => GameManager.ToMainMenu();
    }
}
