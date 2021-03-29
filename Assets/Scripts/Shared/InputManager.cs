// ReSharper disable InconsistentNaming

using System;
using Tools.Commons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shared
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputManager : Singleton<InputManager>
    {
        // 略 for IfEnabled
        private void IE(Action todo)
        {
            if (enabled) todo();
        }

        public static event Action PressConfirm;
        public static event Action PressBack;
        public static event Action<char> OnChar;

        public void OnConfirm() => IE(() => PressConfirm?.Invoke());
        public void OnBack() => IE(() => PressBack?.Invoke());

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
            if (enabled)
                OnChar?.Invoke(character);
        }
    }
}
