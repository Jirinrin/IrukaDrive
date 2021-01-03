using System.Collections;
using Gameplay.Domain;
using TMPro;
using UnityEngine;

namespace Gameplay.SingletonComponents
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [RequireComponent(typeof(Animator))]
    public class HitResultDisplay : MonoBehaviour
    {
        private const float DisappearTime = 1f;

        private static readonly int HitResultKey = Animator.StringToHash("HitResult");
        
        private static readonly string[] ResultStrings = { "NULL", "PERFECT", "EARLY", "LATE", "ERROR" };

        private TextMeshProUGUI _text;
        private Animator _anim;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _text.text = "";
            _anim = GetComponent<Animator>();
        }

        private void OnHitResult(NoteResult result)
        {
            var index = (int) result;
            _text.text = ResultStrings[index];
            _anim.SetInteger(HitResultKey, index);
            StopAllCoroutines();
            StartCoroutine(DisappearAfterTime(DisappearTime));
        }

        private IEnumerator DisappearAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            _anim.SetInteger(HitResultKey, -1);
        }
        
        private void OnHit(char c, NoteResult result, float? _) => OnHitResult(result);
        private void OnMiss() => OnHitResult(NoteResult.Miss);
        
        private void OnEnable()
        {
            GameplayManager.OnHit += OnHit;
            GameplayManager.OnMiss += OnMiss;
        }
        private void OnDisable()
        {
            GameplayManager.OnHit -= OnHit;
            GameplayManager.OnMiss -= OnMiss;
        }
    }
}
