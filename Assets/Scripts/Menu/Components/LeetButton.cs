using System;
using DG.Tweening;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Menu.Components
{
    // todo: make this directly extend Button?
    [RequireComponent(typeof(Button))]
    public class LeetButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // todo: on change this in editor view, also change it in the underlying text child
        [SerializeField] private Color textColor = Color.green;
        [SerializeField] [Min(.1f)] private float rgbCycleTimeSeconds = 1.5f;
        [SerializeField] private bool ambientGlow;
        public float ambientGlowSpeed = 5f;
        [SerializeField] [Range(0f,1f)] private float ambientGlowAmpl = .2f;

        private Button _button;
        private TextMeshProUGUI _text;

        private bool _hovering;

        public Button.ButtonClickedEvent OnClick => (_button ??= GetComponent<Button>()).onClick;
        public void SetInteractable(bool val) => _button.interactable = val;
        public void SetColor(Color col) => (_text ??= GetComponentInChildren<TextMeshProUGUI>()).color = textColor = col;
        public void SetText(string txt) => (_text ??= GetComponentInChildren<TextMeshProUGUI>()).text = txt;

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
            // Cursor.SetCursor()
            DoAlphaAnim(.2f, Anim.BigBlink);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            _text.DOColor(textColor, .3f);
        }

        private void DoAlphaAnim(float dur, Func<float, float> fn) =>
            Anim.DoAnim(dur, t => _text.color = _text.color.SetAlpha(fn(t)));

        private void Update()
        {
            if (ambientGlow && !_hovering && _button.interactable)
                _text.color = _text.color.SetAlpha(Anim.Pulsate(ambientGlowSpeed, ambientGlowAmpl));

            if (_hovering)
            {
                Anim.PeriodicNorm(rgbCycleTimeSeconds, t =>
                {
                    var (r,g,b) = Color.HSVToRGB(t,1,1);
                    _text.color = _text.color.SetRGB(r, g, b);
                });
            }
        }

        private void OnEnable()
        {
            _text = GetComponentInChildren<TextMeshProUGUI>();
            _text.color = textColor;
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => DoAlphaAnim(.3f, Anim.SmallBlink));
        }
        private void OnDisable()
        {
            _text.color = textColor;
            _hovering = false;
        }

        // private static readonly AnimationCurve BlinkCurve = new AnimationCurve(
        //     new Keyframe(0f, 1f),
        //     new Keyframe(.06f, 0f), new Keyframe(.06f, 1f),
        //     new Keyframe(.16f, 0f), new Keyframe(.16f, 1f),
        //     new Keyframe(.34f, 0f), new Keyframe(.34f, 1f)
        //     );
    }
}
