using Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.Components
{
    [RequireComponent(typeof(Button))]
    public class KeyboardButtonTrigger : MonoBehaviour
    {
        [SerializeField] private char triggerCharacter;

        private Button _button;

        private void OnChar(char c)
        {
            if (c == triggerCharacter)
                _button.onClick.Invoke();
        }

        private void OnEnable()
        {
            if (triggerCharacter == char.MinValue)
            {
                Debug.LogWarning($"No trigger char was set for {gameObject}");
                return;
            }

            _button = GetComponent<Button>();
            var txt = GetComponentInChildren<TextMeshProUGUI>();
            if (!txt.text.StartsWith("<"))
                txt.text = $"<voffset=0.1em><u><b>{txt.text.Substring(0,1)}</b></u></voffset>{txt.text.Substring(1)}";
            InputManager.OnChar += OnChar;
        }
        private void OnDisable()
        {
            InputManager.OnChar -= OnChar;
        }
    }
}