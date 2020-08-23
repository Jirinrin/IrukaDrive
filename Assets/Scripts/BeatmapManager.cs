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
    public string text; // will be split up into chars
    public float beatInterval;
}

public struct RuntimeNote
{
    public float Beat;
    public char Char;
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

public struct BeatmapResult
{
    public List<float> NoteTimings;
}

public class BeatmapManager : MonoBehaviour
{
    private SongManager _songManager;
    private TrackManager _trackManager;
    
    /* [NonSerialized] public static */ public Beatmap currentBeatmap;

    private List<RuntimeNote> _runtimeNotes;
    private List<RuntimeNote>.Enumerator _noteIterator;
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
        _runtimeNotes = currentBeatmap.notes.SelectMany(note =>
            note.text.ToCharArray().Select((c, i) =>
                new RuntimeNote
                {
                    Beat = note.bar * currentBeatmap.beatsPerBar + note.beat + i * note.beatInterval,
                    Char = c
                }
            )
        ).ToList();
        
        _songManager.LoadSong(currentBeatmap);
        _noteIterator = _runtimeNotes.GetEnumerator();
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
        if (!_noteIterator.MoveNext())
        {
            _beatmapFinished = true;
            return;
        }
        
        _prevNote = _nextNote;
        _nextNote = _noteIterator.Current;
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
    
    private void Tap() { }

    private void OnChar(char character)
    {
        if (_beatmapFinished)
            return;
        
        var snapshot = _songManager.SongPosBeats;
        var timingLate = snapshot - _prevNote.Beat;
        var timingEarly = _nextNote.Beat - snapshot;
        // var snapToNote = 
        var timing = timingLate < timingEarly ? timingLate : -timingEarly;
        // todo: scale this by the bpm
        _noteResults.Add(timing);
        OnInput?.Invoke(character, timing);
        
    }

    private void SongFinished() => OnBeatmapSongFinished?.Invoke();

    private void OnEnable() {
        PlayerInputManager.OnTap += Tap;
        SongManager.OnSongFinished += SongFinished;
    }
    
    public static event Action OnNote;
    public static event Action<char, float> OnInput; // pass in char and timing
    public static event Action OnBeatmapSongFinished;
}
