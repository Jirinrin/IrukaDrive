using System.Collections.Generic;
using System.Linq;
using Menu.ScreenControllers.SongSelect.Components;
using Shared;
using Shared.Domain;
using Tools;
using UnityEngine;
using Cache = Shared.Cache;

namespace Menu.ScreenControllers.SongSelect
{
    public class SongSelectScreen : MonoBehaviour
    {
        [SerializeField] private SongDataPanel songDataPanel;
        [SerializeField] private SongWheel songWheel;

        private AudioSource _audioSource;

        private Song _selectedSong;
        private SongDifficulty _selectedDiff;

        private void OnEnable()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_selectedSong != null)
                SelectSong(_selectedSong, true);
            InputManager.PressBack += BackToTitle;
            InputManager.PressConfirm += Play;
        }

        private void OnDisable()
        {
            if (SongManager.Instance)
                SongManager.Instance.Stop();
            InputManager.PressBack -= BackToTitle;
            InputManager.PressConfirm -= Play;
        }

        private void Start() => Init();

        private async void Init()
        {
            // todo: already init this in title screen already
            await Cache.InitSongs();
            songWheel.InitSongWheel();
            songWheel.OnSelectSong += SelectSongSimple;
            songDataPanel.OnChooseDiff += SelectDiff;
            SelectSong(Cache.Songs.First());
        }

        private void SelectSongSimple(Song song) => SelectSong(song);
        private async void SelectSong(Song song, bool selectAnyway = false)
        {
            if (_selectedSong?.folderPath == song.folderPath && !selectAnyway)
                return;

            _selectedSong = song;
            songWheel.SelectSong(song);
            songDataPanel.SetSong(song);

            // todo: allow beatmap to specify where preview should start or something?
            var clip = await song.Audio;
            if (clip)
                SongManager.Instance.PlaySong(clip, clip.length * .4f);
        }

        private void SelectDiff(SongDifficulty diff) =>
            _selectedDiff = diff;

        public async void Play() =>
            GameManager.ToGameplay(await Cache.GetBeatmapAsync(_selectedDiff.filePath));

        public void ToGameplay() =>
            // Use this for easy dev
            // GameManager.ToGameplay(SerializationHelpers.LoadBeatmap( $"{Application.streamingAssetsPath}/DriveCharts/OfficialCharts/SDVX Tutorial/2_advanced.drive"));
            SerializationHelpersAsync.LoadSelectBeatmap(b => GameManager.ToGameplay(b), true);

        public async void ToGameplay(string pathInBeatmapsFolder)
        {
            GameManager.ToGameplay(await Cache.GetBeatmapAsync($"{Application.streamingAssetsPath}/DriveCharts/{pathInBeatmapsFolder}.drive"));
        }

        public void BackToTitle() => MenuManager.Instance.ToScreen(MenuScreen.Title);
    }
}
