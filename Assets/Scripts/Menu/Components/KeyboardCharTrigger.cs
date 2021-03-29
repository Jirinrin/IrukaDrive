using Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Menu.Components
{
    public abstract class KeyboardCharTrigger : MonoBehaviour
    {
        [SerializeField] protected UnityEvent doOnTrigger;
        [SerializeField] private char triggerCharacter;

        private void OnChar(char c)
        {
            if (c == triggerCharacter)
                doOnTrigger.Invoke();
        }

        protected virtual void OnEnable()
        {
            if (triggerCharacter == char.MinValue)
            {
                Debug.LogWarning($"No trigger char was set for {gameObject}");
                return;
            }
            InputManager.OnChar += OnChar;
        }

        protected virtual void OnDisable() => InputManager.OnChar -= OnChar;
    }
}