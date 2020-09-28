using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

public class BeatmapManager : Singleton<BeatmapManager>
{
    [NonSerialized] public Beatmap currentBeatmap;
    
    private const float SecondsBeforeNextWord = 1f;
    private const float MinimumSecondsAfterWord = .5f;
    private const float MaxJudgementBeatTimeWindow = 2f;

    private List<RuntimeWord> _runtimeWords;
    
    private List<RuntimeWord>.Enumerator _wordsIterator;
    private RuntimeWord _currentWord;
    private RuntimeWord _nextWord;
    private float _switchNextWordThreshold;

    private bool _beatmapStarted;
    private bool _lastWordReached;
    private bool BeatmapFinished => _lastWordReached && _currentWord.Finished;

    private void Start()
    {
        LoadBeatmap();
    }

    private void LoadBeatmap()
    {
        currentBeatmap = SerializationHelpers.LoadBeatmap(@"C:\Users\侍鈴\Documents\Unity\IrukaDive\Build\bla.blarr");

        _runtimeWords = currentBeatmap.words.Select(word => new RuntimeWord(word)).ToList();
        
        SongManager.Instance.LoadSong(currentBeatmap);
        
        TrackManager.Instance.InitTrack(currentBeatmap, _runtimeWords);
        
        _wordsIterator = _runtimeWords.GetEnumerator();
        AdvanceWord(true);
        
        _lastWordReached = false;
        _beatmapStarted = true;
    }

    public BeatmapResult? GetResult()
    {
        // May comment out for testing purposes
        // if (!BeatmapFinished)
        //     return null;

        return new BeatmapResult
        {
            NoteResults = _runtimeWords.SelectMany(word => word.CharNotes).ToList(),
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
        if (_lastWordReached || SongManager.Instance.SongPosBeats < _switchNextWordThreshold)
            return;
        
        // Next word threshold passed!
        AdvanceWord();
    }

    private void Update()
    {
        if (!_beatmapStarted)
            return;
        
        CheckNextWord();
        if (_currentWord.CheckPassedNote(SongManager.Instance.SongPosBeats))
            OnNote?.Invoke();
        
        TrackManager.Instance.UpdateProgress(SongManager.Instance.SongPosBeats);
    }

    private void Tap() { }

    private void OnChar(char character)
    {
        if (_currentWord.Finished)
            return;
        
        var currentCharNote = (RuntimeNote) _currentWord.CurrentInputNote; // c# 8

        if (character != currentCharNote.Char)
        {
            currentCharNote.Result = NoteResult.WrongChar;
            OnHit?.Invoke(currentCharNote.Char, NoteResult.WrongChar, null);
        }
        else
        {
            var beatSnapshot = SongManager.Instance.SongPosBeats;
            var timing = beatSnapshot - currentCharNote.BeatAbs;
            if (Math.Abs(timing) < MaxJudgementBeatTimeWindow)
            {
                currentCharNote.ResultTiming = timing;
                currentCharNote.Result = NoteResult.Hit;
                // todo: scale this by the bpm (maybe somewhere else than here)
                OnHit?.Invoke(currentCharNote.Char, NoteResult.Hit, timing);
            }
            else
                currentCharNote.Result = null;
        }

        _currentWord.AdvanceInputNote();
    }

    private void SongFinished() => OnBeatmapSongFinished?.Invoke();

    private void OnEnable()
    {
        PlayerInputManager.OnTap += Tap;
        PlayerInputManager.OnChar += OnChar;
        SongManager.OnSongFinished += SongFinished;
    }

    public static void ChangeCurrentChar(int? val) => OnChangeCurrentChar?.Invoke(val);

    public static event Action OnNote;
    public static event Action<char, NoteResult, float?> OnHit; // Pass in timing, char, noteresult
    public static event Action OnBeatmapSongFinished;
    public static event Action<float> OnChangeCurrentWord; // Pass in beat
    public static event Action<int?> OnChangeCurrentChar; // Pass in index
}

[Serializable]
public class BeatmapWord
{
    public float beat;
    public string text = ""; // will be split up into chars
    public float beatInterval = C.DefaultBeatInterval;

    public BeatmapWord() { }
    public BeatmapWord(float beat)
    {
        this.beat = beat;
    }
}

public enum NoteResult
{
    Hit,
    WrongChar,
    // Miss = null,
}

public class RuntimeNote : ParsedNote
{
    public NoteResult? Result;
    public float? ResultTiming;
    public readonly float BeatAbs; // Beat relative to the start of the song instead of the start of the word
    public RuntimeNote(ParsedNote baseNote, float beatAbs)
    {
        Beat = baseNote.Beat;
        Char = baseNote.Char;
        BeatAbs = beatAbs;
    }
}

public class RuntimeWord
{
    public readonly float Beat;
    public readonly float LastBeat;
    public readonly List<RuntimeNote> CharNotes;
    
    private int _inputNoteIndex;
    private int _noteIndex;
    public RuntimeNote CurrentInputNote; // c# 8: add nullable question mark
    public RuntimeNote CurrentNote; // c# 8: add nullable question mark
    public bool Finished;
    private bool _passed;

    public RuntimeWord(BeatmapWord word)
    {
        CharNotes = word.ParseNotes().Select(note => new RuntimeNote(note, word.beat + note.Beat)).ToList();
        if (!CharNotes.Any())
            throw new Exception("Empty word: " + this);
        
        Beat = word.beat;
        LastBeat = word.LastBeat();
        CurrentInputNote = CharNotes[0];
        CurrentNote = CharNotes[0];
    }

    private void SetCurrentInputNote(int? index)
    {
        BeatmapManager.ChangeCurrentChar(index);
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
        if (_passed || !(CurrentNote?.BeatAbs < beatTime))
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
    public string songFile;
    [NonSerialized][XmlIgnore] public AudioClip song;
    [NonSerialized][XmlIgnore] public string filePath;
    public float bpm;
    public float beatOffset;
    [Range(2,4)] public int beatsPerBar = 4;
    public int barOffset;
    // Expected to already be sorted
    [FormerlySerializedAs("notes")] public List<BeatmapWord> words;
}

public struct BeatmapResult
{
    public List<RuntimeNote> NoteResults;
}
