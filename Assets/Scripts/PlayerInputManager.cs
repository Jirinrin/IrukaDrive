using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputManager : MonoBehaviour
{
    [SerializeField] private GameObject game = null;
    private SongManager _songManager;
    private GameManager _gameManager;

    public static event Action OnTap;
    public static event Action<char> OnChar;

    private void Start()
    {
        _songManager = game.GetComponent<SongManager>();
        _gameManager = game.GetComponent<GameManager>();
       
    }
    
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
        Debug.Log("keyboard event");
        Debug.Log(character);
        OnChar?.Invoke(character);
    }
}
