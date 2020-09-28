using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputManager : MonoBehaviour
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

    private void OnKeyboardEvent(char character)
    {
        OnChar?.Invoke(character);
    }
}
