using System;
using Tools.Commons;
using UnityEngine;
using UnityEngine.InputSystem;
// ReSharper disable InconsistentNaming

namespace Gameplay
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputManager : Singleton<PlayerInputManager>
    {
        public static event Action Confirm;
        public static event Action Back;
        public static event Action<char> OnChar;

        public void OnConfirm() => Confirm?.Invoke();
        public void OnBack() => Back?.Invoke();

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
