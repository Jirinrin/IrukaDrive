using System;
using DG.Tweening;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Menu
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Animator))]
    public class LeetButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const float HoverDur = .4f;
        private static readonly AnimationCurve BlinkCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(.06f, 0f),
            new Keyframe(.06f, 1f),
            new Keyframe(.16f, 0f),
            new Keyframe(.16f, 1f),
            new Keyframe(.34f, 0f),
            new Keyframe(.34f, 1f)
            );
        private static readonly int HoverKey = Animator.StringToHash("Hover");

        [SerializeField] private Color textColor = Color.green;

        private Button _button;
        private TextMeshProUGUI _text;
        private Animator _anim;

        private bool _hovering;
        private float _hoverT;

        private void HoverTransition(float newT)
        {
            _hoverT = newT;
            var tt = _hoverT * HoverDur;
            var a = Interp.BlinkDownUp(.01f, .05f, tt) * Interp.BlinkDownUp(.13f, .05f, tt) * Interp.BlinkDownUp(.35f, .01f, tt);
            _text.color = textColor.SetAlpha(a);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
            // Cursor.SetCursor()
            _hoverT = 0f;
            DOTween.To(() => _hoverT, HoverTransition, 1f, HoverDur)
                .OnComplete(() => _anim.SetBool(HoverKey, _hovering));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            _text.color = textColor;
            _anim.SetBool(HoverKey, false);
            // t = 1f;
            // _text.DOColor(textColor, HoverDur);
            // DOTween.To(() => t, HoverTransition, 0f, HoverDur);
        }

        private void OnDisable()
        {
            _text.color = textColor;
        }
        private void OnEnable()
        {
            _text = GetComponentInChildren<TextMeshProUGUI>();
            _text.color = textColor;
            _button = GetComponent<Button>();
            _anim = GetComponent<Animator>();
            _anim.SetBool(HoverKey, false);
        }
    }
}
