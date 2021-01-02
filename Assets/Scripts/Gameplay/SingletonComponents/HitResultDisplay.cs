using System.Collections;
using Gameplay.Domain;
using TMPro;
using UnityEngine;

namespace Gameplay.SingletonComponents
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class HitResultDisplay : MonoBehaviour
    {
        private const float DisappearTime = 1f;

        private static readonly string[] ResultStrings = { "MISS", "PERFECT", "EARLY", "LATE", "WRONG" };
        private static readonly Color[] ResultColors = { Color.red, Color.white, Color.gray, Color.gray, Color.red };

        private TextMeshProUGUI _text;
        // private Animator _anim;
        
        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _text.text = "";
        }

        private void OnHitResult(NoteResult result)
        {
            var index = (int) result;
            _text.text = ResultStrings[index];
            _text.color = ResultColors[index];
            StopAllCoroutines();
            StartCoroutine(DisappearAfterTime(DisappearTime));
        }

        private IEnumerator DisappearAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            _text.text = "";
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
