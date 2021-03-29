using System;
using DG.Tweening;
using Shapes;
using Shared;
using Shared.Domain;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Menu.ScreenControllers.SongSelect.Components
{
    public class SongCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private static readonly Color TextColorSelected = C.CharColorHighlight;
        private static readonly Color TextColorUnselected = new Color(0f, 0.4f, 0f);

        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI artistText;
        [SerializeField] private Image panel;
        [SerializeField] private Line line;
        private RectTransform _rectTransform;

        private Song _song;
        private Action<Song> _onSelect;

        private bool _selected;
        private bool _hovering;

        private float _generalAlpha = 1f;
        private float _prevGeneralAlpha = 1f;

        public void SetSelected(bool selected)
        {
            _selected = selected;
            if (!selected)
                ColorTo(.3f, TextColorUnselected);
            else
                DoAlphaAnim(.4f, Anim.BigBlink);
            DOTween.To(() => line.Color, c => line.Color = c, selected ? TextColorSelected : TextColorUnselected, .5f);
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
            DoAlphaAnim(.2f, Anim.SmallBlink);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            ColorTo(.3f, TextColorUnselected);
        }

        private void Update()
        {
            if (_selected)
            {
                SetColor(TextColorSelected.SetAlpha(_generalAlpha * Anim.Pulsate(5, .4f)));
                line.Color = line.Color.SetAlpha(Anim.Pulsate(5, .4f, -.1f));
            }
            else if (_hovering)
                Anim.Periodic(1.5f, t => SetAlpha(_generalAlpha * Anim.SmallBlink(t) * Anim.Pulsate(5, .4f)));
            else if (_generalAlpha != _prevGeneralAlpha)
            {
                _prevGeneralAlpha = _generalAlpha;
                SetAlpha(_generalAlpha);
            }
        }

        private void ColorTo(float dur, Color c)
        {
            titleText.DOColor(c, dur);
            artistText.DOColor(c, dur);
        }

        private void SetAlpha(float a) => SetColor(titleText.color.SetAlpha(a));
        private void SetColor(Color c)
        {
            titleText.color = c;
            artistText.color = c;
        }

        private void DoAlphaAnim(float dur, Func<float, float> fn) =>
            Anim.DoAnim(dur, t => _generalAlpha = fn(t));

        private void OnEnable()
        {
            _rectTransform = GetComponent<RectTransform>();
            line.End = new Vector3(-_rectTransform.rect.width, 0, 0);
            SetSelected(false);
        }
    }
}