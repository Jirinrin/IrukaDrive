using TMPro;
using UnityEngine;

namespace Menu.ScreenControllers.SongSelect.Components
{
    public class TitleText : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        public string Text
        {
            get => _text.text;
            set
            {
                _text.text = value;
                var l = value.Length;
                _text.fontSize = l <= 12 ? 30 : l < 19 ? 20 : 15;
            }
        }

        private void OnEnable()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }
    }
}
