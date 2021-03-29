using System;
using System.Collections.Generic;
using DG.Tweening;
using Mopsicus.InfiniteScroll;
using Shared.Domain;
using Tools.Commons;
using UnityEngine;
using UnityEngine.UI;
using Cache = Shared.Cache;

namespace Menu.ScreenControllers.SongSelect.Components
{
    public class SongWheel : Singleton<SongWheel>
    {
        private const int CardHeight = 60;

        private InfiniteScroll _infiniteScroll;
        private ScrollRect _scrollRect;
        public RectTransform rectTransform;

        // todo: somehow get rid of cards lookup?
        private Dictionary<string, SongCard> _cardsLookup;

        private Song _selectedSong;

        public event Action<Song> OnSelectSong;
        private void EmitSelectSong(Song song) => OnSelectSong?.Invoke(song);

        private void OnFillItem(int index, GameObject item)
        {
            var song = Cache.songs[index];

            var card = item.GetComponent<SongCard>();
            card.Init(song, EmitSelectSong, _selectedSong?.folderPath == song.folderPath);
            _cardsLookup[song.folderPath] = card;
        }

        private static int OnHeight(int index) => CardHeight;

        private void OnPull(InfiniteScroll.Direction direction) =>
            ScrollToExtreme(direction == InfiniteScroll.Direction.Top ? -1 : 1);

        // private void Refresh()
        // {
        //     // todo: refresh
        //     _infiniteScroll.ApplyDataTo(Cache.Songs.Count, Cache.Songs.Count-Cache.Songs.Count, InfiniteScroll.Direction.Bottom);
        // }

        public void GoToIndex(int index)
        {
            var i = Mathf.Clamp(index, 0, Cache.songs.Count - 1);
            if (Cache.songs.Count > 3)
                ScrollToIndex(i);
            EmitSelectSong(Cache.songs[i]);
        }

        private void ScrollToIndex(int index, bool instant = false)
        {
            var v = 1f - (index / (float) (Cache.songs.Count - 1));
            if (instant) _scrollRect.verticalNormalizedPosition = v;
            else _scrollRect.DOVerticalNormalizedPos(v, .3f);
        }
        public void ScrollToExtreme(int direction) => GoToIndex(direction == 1 ? 0 : Cache.songs.Count-1);
        private void ScrollSkip(int direction) => GoToIndex(_selectedSong.wheelIndex + direction*-1 * Mathf.FloorToInt(rectTransform.rect.height / CardHeight));
        private void Scroll1(int direction) => GoToIndex(_selectedSong.wheelIndex + direction*-1);

        public void InitSongWheel()
        {
            _cardsLookup = new Dictionary<string, SongCard>();
            _infiniteScroll.InitData(Cache.songs.Count);
        }

        public void SelectSong(Song song, bool setScroll = false)
        {
            if (_selectedSong != null)
                _cardsLookup[_selectedSong.folderPath]?.SetSelected(false);
            _selectedSong = song;
            _cardsLookup[_selectedSong.folderPath]?.SetSelected(true);
            if (setScroll)
                ScrollToIndex(song.wheelIndex, true);
        }

        private void OnEnable()
        {
            rectTransform = GetComponent<RectTransform>();
            _infiniteScroll = GetComponent<InfiniteScroll>();
            _infiniteScroll.OnFill += OnFillItem;
            _infiniteScroll.OnHeight += OnHeight;
            _infiniteScroll.OnPull += OnPull;
            _scrollRect = GetComponent<ScrollRect>();
            MenuInputManager.PressVertExtreme += ScrollToExtreme;
            MenuInputManager.PressVertSkip += ScrollSkip;
            MenuInputManager.PressVert += Scroll1;
        }
        private void OnDisable()
        {
            MenuInputManager.PressVertExtreme -= ScrollToExtreme;
            MenuInputManager.PressVertSkip -= ScrollSkip;
            MenuInputManager.PressVert -= Scroll1;
        }
    }
}