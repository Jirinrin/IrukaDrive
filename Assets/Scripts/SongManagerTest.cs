using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongManagerTest : MonoBehaviour
{
    public Text outputTextBox;
        
    private SongManager _songman;

    [NonSerialized] private float _lastTiming;
    
    private void Start()
    {
        _songman = GetComponent<SongManager>();
    }

    private void Update()
    {
        outputTextBox.text = String.Join(
            Environment.NewLine,
            $"{_songman.Beatmap.bpm} BPM",
            $"{_songman.SongPosSec} seconds in",
            $"{_songman.SongPosBeatsMod} beats in",
            $"{_songman.SongPosBars} bars in",
            $"{_lastTiming} is last timing"
        );
    }

    private void UpdateTiming()
    {
        _lastTiming = _songman.Timing;
    }
    
    private void OnEnable()
    {
        PlayerInputManager.OnTap += UpdateTiming;
    }
}
