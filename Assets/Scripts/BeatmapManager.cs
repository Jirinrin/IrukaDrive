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

        Debug.Log(note.bar);
        Debug.Log(note.beat);
        
        _nextNoteBeat = note.bar * currentBeatmap.beatsPerBar + note.beat;
        Debug.Log(_nextNoteBeat);
    }

    private void CheckNextNote()
    {
        if (_beatmapFinished || _songManager.SongPosBeats < _nextNoteBeat)
            return;
        
        Debug.Log("check next note");
    
        while (!_beatmapFinished && _songManager.SongPosBeats >= _nextNoteBeat)
            AdvanceNote();
        
        OnNote?.Invoke();
    }
    
    private void Update()
    {
        CheckNextNote();
    }
    
    public delegate void SimpleEventDelegate();

    public static event SimpleEventDelegate OnNote;

    public void Tap()
    {
        
    }
}
