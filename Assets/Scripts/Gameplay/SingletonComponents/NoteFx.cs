using System.Collections.Generic;
using System.Linq;
using Gameplay.Components;
using Gameplay.Domain;
using Shared;
using TMPro;
using Tools;
using Tools.Commons;
using UnityEngine;
using UnityEngine.UI;

// todo: make pulse animation fancy
namespace Gameplay.SingletonComponents
{
    public class NoteFx : Singleton<NoteFx>
    {
        [SerializeField] private CharacterHitAnim characterPrefab = null;
    
        [SerializeField] private Image circleSpriteNote = null;
        [SerializeField] private Image circleSpriteTap = null;

        [SerializeField] private Animation circlePulseAnimation = null;

        private const float DefaultCircleScale = 1f;
        private const float MaxCircleScale = 2f;
    
        private float _noteCircleScale = DefaultCircleScale;
        private float _tapCircleScale = DefaultCircleScale;

        private RecyclerPool<CharacterHitAnim> _characterAnimObjPool;

        private void Pulse()
        {
            _noteCircleScale = MaxCircleScale;
        }
    
        private void OnTap(char _)
        {
            _tapCircleScale = MaxCircleScale;
        }

        private void OnHit(RuntimeChar c) => OnHit(c, 0);
        private void OnHit(RuntimeChar c, float yOffset)
        {
            var obj = _characterAnimObjPool.Request();

            obj.text.text = c.result == NoteResult.WrongChar ? c.wrongChar.ToString() : c.character.ToString();
            obj.StartAnim(c.result);
            obj.onFinish = () => _characterAnimObjPool.Add(obj);
            obj.transform.localPosition = new Vector3(0, yOffset, 0);
        }

        private void OnHitChord(IEnumerable<RuntimeChar> chars)
        {
            var charsArr = chars.ToArray();
            var char0Offset = (charsArr.Length-1) / 2f;
            foreach (var (c, i) in charsArr.WithIndex())
                if (c.result != NoteResult.Miss)
                    OnHit(c, (char0Offset - i) * C.ChordDefaultHeightDiff);
        }

        private void Awake()
        {
            _characterAnimObjPool = new RecyclerPool<CharacterHitAnim>(() => Instantiate(characterPrefab, transform), 6);
            circlePulseAnimation.Play("CirclePulse");
            circlePulseAnimation["CirclePulse"].speed = 0;
        }

        private void Update()
        {
            var scaleDownFactor = 3f * Time.deltaTime;
            
            circleSpriteNote.transform.localScale = new Vector3(_noteCircleScale, _noteCircleScale);
            if (_noteCircleScale > DefaultCircleScale)
                _noteCircleScale -= (MaxCircleScale - DefaultCircleScale) * scaleDownFactor;

            circleSpriteTap.transform.localScale = new Vector3(_tapCircleScale, _tapCircleScale);
            if (_tapCircleScale > DefaultCircleScale)
                _tapCircleScale -= (MaxCircleScale - DefaultCircleScale) * scaleDownFactor;

            circlePulseAnimation["CirclePulse"].time = SongManager.Instance.BeatTiming;
        }
    
        private void OnEnable()
        {
            GameplayManager.OnNote += Pulse;
            GameplayManager.OnHit += OnHit;
            GameplayManager.OnHitChord += OnHitChord;
            InputManager.OnChar += OnTap;
        }
        private void OnDisable()
        {
            GameplayManager.OnNote -= Pulse;
            GameplayManager.OnHit -= OnHit;
            GameplayManager.OnHitChord -= OnHitChord;
            InputManager.OnChar -= OnTap;
        }
    }
}
