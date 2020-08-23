using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct Note
{
    public int bar;
    public float beat;
}

public struct RuntimeNote
{
    public float Beat;
}

public struct BeatmapResult
{
    public List<float> NoteTimings;
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
    private TrackManager _trackManager;
    
    /* [NonSerialized] public static */ public Beatmap currentBeatmap;

    private List<RuntimeNote> _runtimeNotes;
    private List<RuntimeNote>.Enumerator _noteCollection;
    private RuntimeNote _prevNote;
    private RuntimeNote _nextNote;

    private List<float> _noteResults = new List<float>();

    private bool _beatmapStarted;
    private bool _beatmapFinished;

    private void Start()
    {
        _songManager = GetComponent<SongManager>();
        _trackManager = GetComponent<TrackManager>();
        LoadBeatmap();
    }

    private void LoadBeatmap()
    {
        _songManager.LoadSong(currentBeatmap);
        _runtimeNotes = currentBeatmap.notes.Select(note => new RuntimeNote {
            Beat = note.bar * currentBeatmap.beatsPerBar + note.beat
        }).ToList();
        _noteCollection = _runtimeNotes.GetEnumerator();
        _noteResults = new List<float>();
        _trackManager.InitTrack();
        AdvanceNote();
        _beatmapFinished = false;
        _beatmapStarted = true;
    }

    public BeatmapResult? GetResult()
    {
        if (!_beatmapFinished)
            return null;

        return new BeatmapResult
        {
            NoteTimings = _noteResults,
        };
    }

    private void AdvanceNote()
    {
        if (!_noteCollection.MoveNext())
        {
            _beatmapFinished = true;
            return;
        }
        
        _prevNote = _nextNote;
        _nextNote = _noteCollection.Current;
    }

    private void CheckNextNote()
    {
        if (_beatmapFinished || _songManager.SongPosBeats < _nextNote.Beat)
            return;
        
        // Next note passed!
        
        // todo: some check to see if the player registered this note at all

        do AdvanceNote();
        while (!_beatmapFinished && _songManager.SongPosBeats >= _nextNote.Beat);
        
        OnNote?.Invoke();
    }
    
    private void Update()
    {
        if (!_beatmapStarted)
            return;
        
        CheckNextNote();
        _trackManager.DrawTrackNotes(_songManager.SongPosBeats, _runtimeNotes);
    }
    
    private void Tap()
    {
        if (_beatmapFinished)
            return;
        
        var snapshot = _songManager.SongPosBeats;
        var timingLate = snapshot - _prevNote.Beat;
        var timingEarly = _nextNote.Beat - snapshot;
        var timing = timingLate < timingEarly ? timingLate : -timingEarly;
        // todo: scale this by the bpm
        _noteResults.Add(timing);
        OnTapped?.Invoke(timing);
    }

    private void SongFinished() => OnBeatmapSongFinished?.Invoke();

    private void OnEnable() {
        PlayerInputManager.OnTap += Tap;
        SongManager.OnSongFinished += SongFinished;
    }
    
    public static event EventDelegate OnNote;
    public static event EventDelegateFloatIn OnTapped; // pass in timing
    public static event EventDelegate OnBeatmapSongFinished;
}
