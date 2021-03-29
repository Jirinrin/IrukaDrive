using System;
using System.Collections.Generic;
using Mopsicus.InfiniteScroll;
using Shared.Domain;
using Tools;
using UnityEngine;
using Cache = Shared.Cache;

namespace Menu.ScreenControllers.SongSelect.Components
{
    public class SongWheel : MonoBehaviour
    {
        [SerializeField] private SongCard songCardPrefab;
        [SerializeField] private GameObject songWheelContainer;
        [SerializeField] private InfiniteScroll infiniteScroll;

        private float _cardHeight;
        private Dictionary<string, SongCard> _cardsLookup;

        private Song _selectedSong;

        public event Action<Song> OnSelectSong;

        private void EmitNewSong(Song song) => OnSelectSong?.Invoke(song);

        // todo: only render the relevant section of the list (with a RecyclerList etc)
        public void InitSongWheel()
        {
            _cardsLookup = new Dictionary<string, SongCard>();

            foreach (var (song, i) in Cache.Songs.WithIndex())
            {
                var card = Instantiate(songCardPrefab, songWheelContainer.transform);
                card.Init(song, EmitNewSong);
                _cardsLookup[song.folderPath] = card;

                if (i == 0)
                    _cardHeight = card.GetComponent<RectTransform>().rect.height;

                var tf = card.transform;
                var pos = tf.localPosition;
                tf.localPosition = new Vector3(pos.x, -_cardHeight * i, pos.z);
            }
        }

        public void SelectSong(Song song)
        {
            if (_selectedSong != null)
                _cardsLookup[_selectedSong.folderPath].SetSelected(false);
            _selectedSong = song;
            _cardsLookup[_selectedSong.folderPath].SetSelected(true);
        }


    }
}