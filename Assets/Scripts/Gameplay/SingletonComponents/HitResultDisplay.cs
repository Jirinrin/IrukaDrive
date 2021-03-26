using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        
        private static readonly string[] ResultStrings = { "MISS", "PERFECT", "EARLY", "LATE", "ERROR", "PERFECT", "GOOD", "ERROR", "PARTIAL" };

        private TextMeshProUGUI _text;
        private Animator _anim;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _text.text = "";
            _anim = GetComponent<Animator>();
        }

        private void OnHitResult(int index)
        {
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

        private static ChordResult ChordResultsToDisplayIndex(IEnumerable<NoteResult> results)
        {
            var wrongFound = false;
            var goodFound = false;
            var perfectFound = false;
            foreach (var r in results)
            {
                if (r == NoteResult.Miss || r == NoteResult.WrongChar) wrongFound = true;
                else if (r == NoteResult.HitPerfect) perfectFound = true;
                else goodFound = true;
            }
            if (wrongFound)
                return goodFound || perfectFound ? ChordResult.Partial : ChordResult.AllWrong;
            return goodFound ? ChordResult.AllGood : ChordResult.AllPerfect;
        }
        
        private void OnHit(RuntimeChar c) => OnHitResult((int) c.result);
        private void OnHitChord(IEnumerable<RuntimeChar> chars) => OnHitResult((int) ChordResultsToDisplayIndex(chars.Select(c => c.result)));
        private void OnMiss() => OnHitResult((int) NoteResult.Miss);
        
        private void OnEnable()
        {
            GameplayManager.OnHit += OnHit;
            GameplayManager.OnHitChord += OnHitChord;
            GameplayManager.OnMiss += OnMiss;
            GameplayManager.OnMissChord += OnMiss;
        }
        private void OnDisable()
        {
            GameplayManager.OnHit -= OnHit;
            GameplayManager.OnHitChord -= OnHitChord;
            GameplayManager.OnMiss -= OnMiss;
            GameplayManager.OnMissChord -= OnMiss;
        }
    }
}
