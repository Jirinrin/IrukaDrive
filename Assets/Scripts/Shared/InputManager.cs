using System;
using Tools.Commons;
using UnityEngine;
using UnityEngine.InputSystem;
// ReSharper disable InconsistentNaming

namespace Shared
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputManager : Singleton<InputManager>
    {
        public static event Action PressConfirm;
        public static event Action PressBack;
        public static event Action PressPlay;
        public static event Action<char> OnChar;

        public void OnConfirm() => PressConfirm?.Invoke();
        public void OnBack() => PressBack?.Invoke();
        public void OnPlay() => PressPlay?.Invoke();

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
