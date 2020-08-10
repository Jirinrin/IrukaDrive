using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Beatmap
{
    public float bpm;
    public AudioClip song;
}

public class BeatmapManager : MonoBehaviour
{
    private SongManager _songManager;
    
    public Beatmap currentBeatmap;
    // [NonSerialized] public Beatmap currentBeatmap;
    
    private void Start()
    {
        _songManager = GetComponent<SongManager>();
        _songManager.LoadSong(currentBeatmap.song, currentBeatmap.bpm);
    }

    private void Update()
    {
        
    }

    public void Tap()
    {
        
    }
}
