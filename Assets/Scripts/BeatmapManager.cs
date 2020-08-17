using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Note
{
    public int bar;
    public float beat;
}

[Serializable]
public struct Beatmap
{
    public AudioClip song;
    public float bpm;
    public float beatOffset;
    [Range(2,4)] public int beatsPerBar;
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

    private bool _beatmapFinished;

    private void Start()
    {
        _songManager = GetComponent<SongManager>();
        _songManager.LoadSong(currentBeatmap);
        _noteCollection = currentBeatmap.notes.GetEnumerator();
        _beatmapFinished = false;
        AdvanceNote();
    }

    private void AdvanceNote()
    {
        Debug.Log("advance");
        _noteCollection.MoveNext();
        var note = _noteCollection.Current;
        if (note == null)
        {
            _beatmapFinished = true;
            return;
        }

        _prevNoteBeat = _nextNoteBeat;
        _nextNoteBeat = note.bar * currentBeatmap.beatsPerBar + note.beat;
    }

    private void CheckNextNote()
    {
        if (_beatmapFinished || _songManager.SongPosBeats < _nextNoteBeat)
            return;
        
        while (!_beatmapFinished && _songManager.SongPosBeats >= _nextNoteBeat)
            AdvanceNote();
        
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
        OnTapped?.Invoke(timing);
    }

    private void OnEnable() => PlayerInputManager.OnTap += Tap;

    public static event EventDelegate OnNote;
    public static event EventDelegateFloatIn OnTapped;
}
