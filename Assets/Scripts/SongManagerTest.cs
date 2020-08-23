using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongManagerTest : MonoBehaviour
{
    public Text outputTextBox;
    public Text resultsTextBox;
        
    private SongManager _songManager;
    private BeatmapManager _beatmapManager;

    [NonSerialized] private float _lastTiming;

    private BeatmapResult? _beatmapResult;
    
    private void Start()
    {
        _songManager = GetComponent<SongManager>();
        _beatmapManager = GetComponent<BeatmapManager>();
        // Invoke(nameof(ShowResult), 3f);
    }

    private void Update()
    {
        outputTextBox.text = string.Join(Environment.NewLine,
            "Diagnostics:",
            $"{_beatmapManager.currentBeatmap.bpm} BPM",
            $"{_songManager.SongPosSec} seconds in",
            $"{_songManager.SongPosBeatsMod} beats in",
            $"{_songManager.SongPosBars} bars in",
            $"{_lastTiming} is last timing"
        );
        if (_beatmapResult != null)
            resultsTextBox.text = string.Join(Environment.NewLine,
                "Results:",
                string.Join(", ", _beatmapResult?.NoteTimings)
            );
    }

    private void ShowResult()
    {
        _beatmapResult = _beatmapManager.GetResult();
    }

    private void UpdateTiming(float timing)
    {
        _lastTiming = timing;
    }
    
    private void OnEnable()
    {
        BeatmapManager.OnTapped += UpdateTiming;
        BeatmapManager.OnBeatmapSongFinished += ShowResult;
    }
}
