using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerInputManager : MonoBehaviour
{
    [SerializeField] private GameObject game;
    private SongManager _songManager;
    private GameManager _gameManager;

    public delegate void InputEventDelegate();

    public static event InputEventDelegate OnTap;

    private void Start()
    {
        _songManager = game.GetComponent<SongManager>();
        _gameManager = game.GetComponent<GameManager>();
    }
    
    public void OnConfirm()
    {
        OnTap?.Invoke();
    }
}
