using TMPro;
using UnityEngine;

namespace Gameplay.SingletonComponents
{
    public class ComboDisplay : MonoBehaviour
    {
        private TextMeshProUGUI _comboText;

        private void Awake()
        {
            _comboText = GetComponent<TextMeshProUGUI>();
            _comboText.text = 0.ToString("D4");
        }
        
        private void OnComboChange(int combo)
        {
            // todo: animation triggers on certain combo values / on drop
            _comboText.text = combo.ToString("D4");
        }
        
        private void OnEnable()
        {
            GameplayManager.OnComboChange += OnComboChange;
        }
        private void OnDisable()
        {
            GameplayManager.OnComboChange -= OnComboChange;
        }
    }
}
