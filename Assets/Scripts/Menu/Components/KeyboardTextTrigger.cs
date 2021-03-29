using Menu.Util;
using TMPro;
using UnityEngine;

namespace Menu.Components
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class KeyboardTextTrigger : KeyboardCharTrigger
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            UnderlineFirstLetter.FormatText(GetComponent<TextMeshProUGUI>());
        }
    }
}