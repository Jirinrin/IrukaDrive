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
            EditorTrackGestures.Instance.enabled = isFunctional;
        }

        public void PlayBeatmapFrom(float beatTime, bool autoplay = true)
        {
            // todo: somehow keep this scene loaded in background
            _inEditorPlay = true;
            GameManager.ToGameplay(currentBeatmap, currentSong.BeatsToSecs(beatTime), autoplay);
        }

        private void Start()
        {
            SetFunctional(true);
            // For dev
            if (GameManager.State != GameState.BeatmapEditor)
            {
                GameManager.SetState(GameState.BeatmapEditor);
                // Uncomment this for easy iterating
                var dir = $"{Application.streamingAssetsPath}/DriveCharts/SDVX Tutorial";
                currentBeatmap = SerializationHelpers.LoadBeatmap($"{dir}/2_advanced.drive");
                currentSong = SerializationHelpers.LoadSong(dir);
                EditorTrack.Instance.LoadBeatmap(currentBeatmap);
                EditorHistory.Reset();
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
            
            SerializationHelpersAsync.LoadSelectBeatmap(b =>
            {
                currentBeatmap = b ?? currentBeatmap;
                currentSong = currentBeatmap.song;
                EditorTrack.Instance.LoadBeatmap(currentBeatmap);
                SetFunctional(true);
            });
        }

        private async void ReloadBeatmap()
        {
            // todo: also reload song?
            currentBeatmap = await SerializationHelpersAsync.LoadBeatmap(currentBeatmap.filePath, currentSong);
            ResetEditor();
            EditorTrack.Instance.LoadBeatmap(currentBeatmap, true);
        }

        private void ResetEditor()
        {
            EditorTrack.Instance.ResetTrack();
            EditorHistory.Reset();
        }

        public void ListenSong()
        {
            if (!SongManager.Instance.IsPlaying)
            {
                SongManager.Instance.LoadSong(currentSong, tickOnBeat: true);
                listenSongBtnText.text = "Stop";
            }
            else
            {
                SongManager.Instance.Stop();
                listenSongBtnText.text = "Listen";
            }
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
