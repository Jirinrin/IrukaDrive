using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
public struct Note
{
    public int bar;
    public float beat;
    public string text; // will be split up into chars
    public float beatInterval;
}

public enum NoteResult
{
    Hit,
    WrongChar,
    Miss, // Not sure whether this one will exist
}

public struct RuntimeNote
{
    public float Beat;
    public char Char;
    public NoteResult? Result;
    public float? ResultTiming;
}

public class RuntimeWord
{
    public readonly float Beat;
    public readonly float LastBeat;
    public readonly List<RuntimeNote> CharNotes;
    private readonly Action<int?> _onChangeInputNoteIndex;
    
    private int _inputNoteIndex;
    private int _noteIndex;
    public RuntimeNote? CurrentInputNote;
    public RuntimeNote? CurrentNote;
    public bool Finished;
    private bool _passed;

    public RuntimeWord(List<RuntimeNote> charNotes, Action<int?> onChangeInputNoteIndex)
    {
        CharNotes = charNotes;
        if (!CharNotes.Any())
            throw new Exception("Empty word: " + this);
        
        Beat = CharNotes.First().Beat;
        LastBeat = CharNotes.Last().Beat;
        CurrentInputNote = CharNotes[0];
        CurrentNote = CharNotes[0];
        _onChangeInputNoteIndex = onChangeInputNoteIndex;
    }
    
    public void SetCurrentInputNote(int? index)
    {
        _onChangeInputNoteIndex.Invoke(index);
        if (index == null)
            CurrentInputNote = null;
        else
            CurrentInputNote = CharNotes[(int) index]; // c# 8
    }

    public void AdvanceInputNote()
    {
        if (Finished)
            return;
        
        _inputNoteIndex++;
        if (_inputNoteIndex >= CharNotes.Count)
        {
            Finished = true;
            SetCurrentInputNote(null);
            return;
        }

        SetCurrentInputNote(_inputNoteIndex);
    }

    public bool CheckPassedNote(float beatTime)
    {
        if (_passed || !(CurrentNote?.Beat < beatTime))
            return false;

        // Passed new note!
        _noteIndex++;
        if (_noteIndex >= CharNotes.Count)
        {
            _passed = true;
            CurrentNote = null;
        }
        else
            CurrentNote = CharNotes[_noteIndex];

        // todo: maybe make this flip in-between notes or sth
        // if (_inputNoteIndex < _noteIndex && CurrentInputNote?.Result == null)
        // {
        //     _inputNoteIndex = _noteIndex;
        //     SetCurrentInputNote(_passed ? (int?) null : _inputNoteIndex);
        // }

        return true;
    }
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
    public List<RuntimeNote> NoteTimings;
}

public class BeatmapManager : MonoBehaviour
{
    private SongManager _songManager;
    private TrackManager _trackManager;
    
    /* [NonSerialized] public static */ public Beatmap currentBeatmap;

    private const float SecondsBeforeNextWord = 1f;
    private const float MinimumSecondsAfterWord = .5f;

    private List<RuntimeWord> _runtimeNotes;
    
    private List<RuntimeWord>.Enumerator _wordsIterator;
    private RuntimeWord _currentWord;
    private RuntimeWord _nextWord;
    private float _switchNextWordThreshold;

    private bool _beatmapStarted;
    private bool _lastWordReached;
    private bool BeatmapFinished => _lastWordReached && _currentWord.Finished;

    private void Start()
    {
        _songManager = GetComponent<SongManager>();
        _trackManager = GetComponent<TrackManager>();
        LoadBeatmap();
    }

    private void LoadBeatmap()
    {
        _runtimeNotes = currentBeatmap.notes.Select(note =>
            new RuntimeWord(note.text.ToCharArray().Select((c, i) =>
                new RuntimeNote
                {
                    Beat = note.bar * currentBeatmap.beatsPerBar + note.beat + i * note.beatInterval,
                    Char = c
                }
            ).ToList(), OnChangeCurrentChar)
        ).ToList();
        
        _songManager.LoadSong(currentBeatmap);
        
        _trackManager.InitTrack(_runtimeNotes);
        
        _wordsIterator = _runtimeNotes.GetEnumerator();
        AdvanceWord(true);
        
        _lastWordReached = false;
        _beatmapStarted = true;
    }

    public BeatmapResult? GetResult()
    {
        // May comment out for testing purposes
        if (!BeatmapFinished)
            return null;

        return new BeatmapResult
        {
            NoteTimings = _runtimeNotes.SelectMany(word => word.CharNotes).ToList(),
        };
    }

    // todo: add exclamation points when c# 8.0 
    private void AdvanceWord(bool init = false)
    {
        if (init)
        {
            if (!_wordsIterator.MoveNext())
                throw new Exception("Empty beatmap");
            _nextWord = _wordsIterator.Current;
        }
        
        // todo: also make currentWord inactive after some time after the last beat
        _currentWord = _nextWord;
        OnChangeCurrentWord?.Invoke(_currentWord.Beat);
        OnChangeCurrentChar?.Invoke(0);
        
        if (!_wordsIterator.MoveNext())
        {
            _lastWordReached = true;
            _nextWord = null;
            return;
        }

        _nextWord = _wordsIterator.Current;
        // Put switch to next current word to x seconds before that word, or in-between the current and next
        _switchNextWordThreshold = _nextWord.Beat - SecondsBeforeNextWord;
        if (_switchNextWordThreshold <= _currentWord.LastBeat + MinimumSecondsAfterWord)
            _switchNextWordThreshold = (_currentWord.LastBeat + _nextWord.Beat) / 2;
    }

    private void CheckNextWord()
    {
        if (_lastWordReached || _songManager.SongPosBeats < _switchNextWordThreshold)
            return;
        
        // Next word threshold passed!
        AdvanceWord();
    }

    private void Update()
    {
        if (!_beatmapStarted)
            return;
        
        CheckNextWord();
        if (_currentWord.CheckPassedNote(_songManager.SongPosBeats))
            OnNote?.Invoke();
        
        _trackManager.DrawTrackNotes(_songManager.SongPosBeats);
    }
    
    private void Tap() { }

    private void OnChar(char character)
    {
        if (_currentWord.Finished)
            return;
        
        var currentCharNote = (RuntimeNote) _currentWord.CurrentInputNote; // c# 8
        OnInput?.Invoke(character, currentCharNote);

        if (character != currentCharNote.Char)
            currentCharNote.Result = NoteResult.WrongChar;
        else
        {
            currentCharNote.Result = NoteResult.Hit;
            var beatSnapshot = _songManager.SongPosBeats;
            currentCharNote.ResultTiming = beatSnapshot - currentCharNote.Beat;
            // todo: scale this by the bpm (maybe somewhere else than here)
        }

        _currentWord.AdvanceInputNote();
    }

    private void SongFinished() => OnBeatmapSongFinished?.Invoke();

    private void OnEnable() {
        PlayerInputManager.OnTap += Tap;
        PlayerInputManager.OnChar += OnChar;
        SongManager.OnSongFinished += SongFinished;
    }
    
    public static event Action OnNote;
    public static event Action<char, RuntimeNote> OnInput; // Pass in inputted char and expected note
    public static event Action OnBeatmapSongFinished;
    public static event Action<float> OnChangeCurrentWord; // Pass in beat
    public static event Action<int?> OnChangeCurrentChar; // Pass in index
}
