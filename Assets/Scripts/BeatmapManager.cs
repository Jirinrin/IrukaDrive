using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Internal;

[Serializable]
public class Note
{
    public int bar;
    public int beat;
    public int subBeat;
}

[Serializable]
public struct Beatmap
{
    public AudioClip song;
    public float bpm;
    public float beatOffset;
    [Range(2,4)] public int beatsPerBar;
    public int barOffset;
    public List<Note> notes;
}

public class BeatmapManager : MonoBehaviour
{
    private SongManager _songManager;
    
    public Beatmap currentBeatmap;
    // [NonSerialized] public Beatmap currentBeatmap;
    
    private void Start()
    {
        _songManager = GetComponent<SongManager>();
        _songManager.LoadSong(currentBeatmap);
    }

    private void Update()
    {
        
    }

    public void Tap()
    {
        
    }
}
