using Gameplay.Components;
using Gameplay.Domain;
using Shared;
using TMPro;
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

        private void OnHit(char c, NoteResult result, float? timing)
        {
            var obj = _characterAnimObjPool.Request();

            obj.text.text = c.ToString();
            obj.StartAnim(result);
            obj.onFinish = () => _characterAnimObjPool.Add(obj);
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
            InputManager.OnChar += OnTap;
        }
        private void OnDisable()
        {
            GameplayManager.OnNote -= Pulse;
            GameplayManager.OnHit -= OnHit;
            InputManager.OnChar -= OnTap;
        }
    }
}
