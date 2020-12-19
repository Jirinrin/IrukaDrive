using System;
using Tools.Commons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputManager : Singleton<PlayerInputManager>
    {
        public static event Action OnTap;
        public static event Action<char> OnChar;

        public void OnConfirm()
        {
            OnTap?.Invoke();
        }

        public void OnEnable()
        {
            InputSystem.DisableDevice(Mouse.current);
            Keyboard.current.onTextInput += OnKeyboardEvent;
        }
        public void OnDisable()
        {
            Keyboard.current.onTextInput -= OnKeyboardEvent;
        }

        public void OnKeyboardEvent(char character)
        {
            OnChar?.Invoke(character);
        }
    }
}
