using System.Collections.Generic;
using System.IO;
using System.Linq;
using Menu.ScreenControllers.SongSelect.Components;
using Shared;
using Shared.Domain;
using Tools;
using UnityEngine;

namespace Menu.ScreenControllers.SongSelect
{
    public class SongSelectScreen : MonoBehaviour
    {
        private static readonly string BeatmapPath = $"{Application.streamingAssetsPath}/Beatmaps";

        [SerializeField] private GameObject songWheelContainer;
        [SerializeField] private SongDataPanel songDataPanel;
        
        [SerializeField] private SongCard songCardPrefab;

        private AudioSource _audioSource;

        private static List<Song> songsCached = new List<Song>();

        private Song _selectedSong;
        private string _selectedDiffPath;
        private float _cardHeight;
        private Dictionary<string, SongCard> _cardsLookup;

        private void Awake()
        {
            InitSongs();
        }

        private void OnEnable()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnDisable()
        {
            SongManager.Instance.Stop();
        }

        private void Start()
        {
            InitSongWheel();
            SelectSong(songsCached.First());
            songDataPanel.OnChooseDiff += SelectDiff;
        }

        // todo: allow nested beatmaps and stuff
        // todo: think about the right structure. Which data per chart and which maybe in a shared thing? Which data do we want to know in song select already?
        private void InitSongs()
        {
            if (songsCached.Any())
                return;
            
            var songs = Directory.GetDirectories($"{Application.streamingAssetsPath}/Beatmaps");
            Debug.Log($"yo. songs => {songs.Join(", ")}");
            foreach (var songFolder in songs)
            {
                var songPath = Path.Combine(BeatmapPath, songFolder);
                var diffPaths = Directory.GetFiles(songPath, "*.drive").Select(d => Path.Combine(songPath, d)).ToArray();
                Debug.Log($"diffs: {diffPaths.Join(" - ")}");
                if (!diffPaths.Any())
                {
                    Debug.LogWarning($"Song {songPath} has no diffs");
                    continue;
                }

                var firstBeatmap = SerializationHelpers.LoadBeatmap(diffPaths.First());
                songsCached.Add(new Song
                {
                    title = firstBeatmap.title,
                    artist = firstBeatmap.artist,
                    jacket = firstBeatmap.jacket,
                    song = firstBeatmap.song,
                    folderName = songFolder,
                    folderPath = songPath,
                    diffPaths = diffPaths,
                });
            }
        }

        private void SelectSong(Song song)
        {
            if (_selectedSong != null || _selectedSong?.folderPath == song.folderPath)
                _cardsLookup[_selectedSong.folderName].SetSelected(false);
            _selectedSong = song;
            _cardsLookup[_selectedSong.folderName].SetSelected(true);
            songDataPanel.SetSong(song);
            // todo: allow beatmap to specify where preview should start or something
            SongManager.Instance.PlaySong(song.song, 45f);
        }

        private void SelectDiff(string diffPath)
        {
            _selectedDiffPath = diffPath;
        }

        // todo: only render the relevant section of the list (with a RecyclerList etc)
        private void InitSongWheel()
        {
            _cardsLookup = new Dictionary<string, SongCard>();
            
            foreach (var (song, i) in songsCached.WithIndex())
            {
                var card = Instantiate(songCardPrefab, songWheelContainer.transform);
                card.Init(song, SelectSong);
                _cardsLookup[song.folderName] = card;
                
                if (i == 0)
                    _cardHeight = card.GetComponent<RectTransform>().rect.height;

                var tf = card.transform;
                var pos = tf.localPosition;
                tf.localPosition = new Vector3(pos.x, -_cardHeight * i, pos.z);
            }
        }
        
        public void Play() =>
            GameManager.ToGameplay(SerializationHelpers.LoadBeatmap(_selectedDiffPath));

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
