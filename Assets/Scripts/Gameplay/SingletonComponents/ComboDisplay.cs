using Gameplay.Domain;
using TMPro;
using UnityEngine;

namespace Gameplay.SingletonComponents
{
    [RequireComponent(typeof(Animator))]
    public class ComboDisplay : MonoBehaviour
    {
        private static readonly int ComboBreakKey = Animator.StringToHash("ComboBreak");
        private static readonly int PulseKey = Animator.StringToHash("Pulse");
        private static readonly int FcStreakKey = Animator.StringToHash("FcStreak");

        [SerializeField] private TextMeshProUGUI comboText;
        
        private Animator _anim;
        
        private bool _fullComboStreak = true;

        private void Awake()
        {
            comboText.text = 0.ToString("D4");
            _anim = GetComponent<Animator>();
        }
        
        private void OnComboChange(int combo)
        {
            // todo: animation triggers on certain combo values / on drop
            comboText.text = combo.ToString("D4");
            if (combo == 0)
            {
                if (_fullComboStreak)
                {
                    _fullComboStreak = false;
                    _anim.SetBool(FcStreakKey, false);
                }
                _anim.SetTrigger(ComboBreakKey);
            }
            else if (combo <= 99 ? (combo == 10 || combo == 20 || combo == 50) : (combo % 100 == 0))
                _anim.SetTrigger(PulseKey);
        }
        
        private void OnEnable()
        {
            BeatmapDisplayScore.OnComboChange += OnComboChange;
        }
        private void OnDisable()
        {
            BeatmapDisplayScore.OnComboChange -= OnComboChange;
        }
    }
}
