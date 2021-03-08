using System;
using DG.Tweening;
using Shared.Domain;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Menu.ScreenControllers.SongSelect.Components
{
    public class SongCard : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI artistText;
        [SerializeField] private Image panel;

        private Song _song;
        private Action<Song> _onSelect;

        private bool _selected;
        public void SetSelected(bool selected)
        {
            _selected = selected;
            panel.DOColor(panel.color.SetAlpha(selected ? .1f : .05f), .5f);
        }
        
        public void Init(Song song, Action<Song> onSelect)
        {
            _song = song;
            _onSelect = onSelect;
            titleText.text = song.title;
            artistText.text = song.artist;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onSelect(_song);
        }

        public void OnPointerEnter()
        {
            panel.color = panel.color.SetAlpha(_selected ? .15f : .075f);
        }

        public void OnPointerExit()
        {
            panel.color = panel.color.SetAlpha(_selected ? .1f : .05f);
        }
    }
}