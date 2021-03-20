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

        [SerializeField] private GameObject songWheelContainer;
        [SerializeField] private SongDataPanel songDataPanel;
        
        [SerializeField] private SongCard songCardPrefab;

        private AudioSource _audioSource;

        private Song _selectedSong;
        private SongDifficulty _selectedDiff;
        private float _cardHeight;
        private Dictionary<string, SongCard> _cardsLookup;

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
            InitSongWheel();
            songDataPanel.OnChooseDiff += SelectDiff;
            SelectSong(Cache.Songs.First());
        }

        private void SelectSongSimple(Song song) => SelectSong(song);
        private async void SelectSong(Song song, bool selectAnyway = false)
        {
            if (_selectedSong?.folderPath == song.folderPath && !selectAnyway)
                return;
            
            if (_selectedSong != null)
                _cardsLookup[_selectedSong.folderPath].SetSelected(false);
            _selectedSong = song;
            _cardsLookup[_selectedSong.folderPath].SetSelected(true);
            songDataPanel.SetSong(song);
            // todo: allow beatmap to specify where preview should start or something
            SongManager.Instance.PlaySong(await song.Audio, 45f);
        }

        private void SelectDiff(SongDifficulty diff)
        {
            _selectedDiff = diff;
        }

        // todo: only render the relevant section of the list (with a RecyclerList etc)
        private void InitSongWheel()
        {
            _cardsLookup = new Dictionary<string, SongCard>();
            
            foreach (var (song, i) in Cache.Songs.WithIndex())
            {
                var card = Instantiate(songCardPrefab, songWheelContainer.transform);
                card.Init(song, SelectSongSimple);
                _cardsLookup[song.folderPath] = card;
                
                if (i == 0)
                    _cardHeight = card.GetComponent<RectTransform>().rect.height;

                var tf = card.transform;
                var pos = tf.localPosition;
                tf.localPosition = new Vector3(pos.x, -_cardHeight * i, pos.z);
            }
        }
        
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
