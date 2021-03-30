using Menu.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.Components
{
    [RequireComponent(typeof(Button))]
    public class KeyboardButtonTrigger : KeyboardCharTrigger
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            doOnTrigger = GetComponent<Button>().onClick;
            UnderlineFirstLetter.FormatText(GetComponentInChildren<TextMeshProUGUI>(), triggerCharacter);
        }
    }
}