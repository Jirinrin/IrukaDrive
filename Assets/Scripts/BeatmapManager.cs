using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Note
{
    public int bar;
    public float beat;
}

[Serializable]
public class Beatmap
{
    public AudioClip song;
    public float bpm;
    public float beatOffset;
    [Range(2,4)] public int beatsPerBar = 4;
    public int barOffset;
    // Expected to already be sorted
    public List<Note> notes;
}

public class BeatmapManager : MonoBehaviour
{
    private SongManager _songManager;
    
    /* [NonSerialized] public static */ public Beatmap currentBeatmap;

    private List<Note>.Enumerator _noteCollection;
    private float _prevNoteBeat;
    private float _nextNoteBeat;

    private List<float> _noteResults = new List<float>();

    private bool _beatmapFinished;

    private void Start()
    {
        _songManager = GetComponent<SongManager>();
        LoadBeatmap();
    }

    private void LoadBeatmap()
    {
        _songManager.LoadSong(currentBeatmap);
        _noteCollection = currentBeatmap.notes.GetEnumerator();
        _beatmapFinished = false;
        _noteResults = new List<float>();
        AdvanceNote();
    }

    private void AdvanceNote()
    {
        if (!_noteCollection.MoveNext())
        {
            _beatmapFinished = true;
            return;
        }
        
        _prevNoteBeat = _nextNoteBeat;
        var note = _noteCollection.Current;
        _nextNoteBeat = note.bar * currentBeatmap.beatsPerBar + note.beat;
    }

    private void CheckNextNote()
    {
        if (_beatmapFinished || _songManager.SongPosBeats < _nextNoteBeat)
            return;
        
        // Next note passed!
        
        // todo: some check to see if the player registered this note

        do AdvanceNote();
        while (!_beatmapFinished && _songManager.SongPosBeats >= _nextNoteBeat);
        
        OnNote?.Invoke();
    }
    
    private void Update()
    {
        CheckNextNote();
    }
    
    private void Tap()
    {
        if (_beatmapFinished)
            return;
        
        var snapshot = _songManager.SongPosBeats;
        var timingLate = snapshot - _prevNoteBeat;
        var timingEarly = _nextNoteBeat - snapshot;
        var timing = timingLate < timingEarly ? timingLate : -timingEarly;
        // todo: scale this by the bpm
        _noteResults.Add(timing);
        OnTapped?.Invoke(timing);
    }

    private void OnEnable() => PlayerInputManager.OnTap += Tap;

    public static event EventDelegate OnNote;
    public static event EventDelegateFloatIn OnTapped;
}
