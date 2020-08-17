using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongManagerTest : MonoBehaviour
{
    public Text outputTextBox;
        
    private SongManager _songManager;
    private BeatmapManager _beatmapManager;

    [NonSerialized] private float _lastTiming;
    
    private void Start()
    {
        _songManager = GetComponent<SongManager>();
        _beatmapManager = GetComponent<BeatmapManager>();
    }

    private void Update()
    {
        outputTextBox.text = String.Join(
            Environment.NewLine,
            $"{_beatmapManager.currentBeatmap.bpm} BPM",
            $"{_songManager.SongPosSec} seconds in",
            $"{_songManager.SongPosBeatsMod} beats in",
            $"{_songManager.SongPosBars} bars in",
            $"{_lastTiming} is last timing"
        );
    }

    private void UpdateTiming()
    {
        _lastTiming = _songManager.Timing;
    }
    
    private void OnEnable()
    {
        PlayerInputManager.OnTap += UpdateTiming;
    }
}
