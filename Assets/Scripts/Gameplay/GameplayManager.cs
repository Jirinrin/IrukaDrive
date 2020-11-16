using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Domain;
using Gameplay.SingletonComponents;
using JetBrains.Annotations;
using Shared;
using Shared.Domain;
using Tools;
using Tools.Commons;
using UnityEngine;

namespace Gameplay
{
    public class GameplayManager : Singleton<GameplayManager>
    {
        public static Beatmap CurrentBeatmap { get; private set; }
        public static float BeatmapStartTime { get; private set; }
        public static bool EditorPlay { get; private set; }
        [CanBeNull] public static IEnumerable<RuntimeWord> RuntimeWords { get; private set; }
        
        public static void PrepGameplay(Beatmap beatmap, float startTime = 0, bool comingFromEditor = false)
        {
            CurrentBeatmap = beatmap;
            BeatmapStartTime = startTime;
            EditorPlay = comingFromEditor;
            RuntimeWords = CurrentBeatmap.words.Select(word => new RuntimeWord(word)).ToList();
        }

        private List<RuntimeWord>.Enumerator _wordsIterator;
        private RuntimeWord _currentWord;
        private RuntimeWord _nextWord;
        private float _switchNextWordThreshold;
        private float? _currentCharMissThreshold;

        [NonSerialized] public bool BeatmapStarted;
        [NonSerialized] public bool BeatmapFinished;
        private bool _lastWordReached;

        private void Start()
        {
            // For dev
            if (CurrentBeatmap == null)
            {
                PrepGameplay(SerializationHelpers.LoadBeatmap(Environment.ExpandEnvironmentVariables(
                    @"%USERPROFILE%\Documents\Unity\IrukaDrive\Assets\Beatmaps\Tutorial\bla3.blarr")));
                GameManager.SetState(GameState.Gameplay);
            }

            LoadBeatmap();
        }

        public void LoadBeatmap()
        {
            SongManager.Instance.LoadSong(CurrentBeatmap);
        
            Track.Instance.InitTrack(CurrentBeatmap, RuntimeWords);
        
            _wordsIterator = RuntimeWords.ToList().GetEnumerator();
            AdvanceWord(true);
        
            BeatmapStarted = true;
            BeatmapFinished = false;
            _lastWordReached = false;
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
            SetCurrentCharMissThreshold();
        
            if (!_wordsIterator.MoveNext())
            {
                _lastWordReached = true;
                _nextWord = null;
                return;
            }

            _nextWord = _wordsIterator.Current;
            // Put switch to next current word to x seconds before that word, or in-between the current and next
            _switchNextWordThreshold = _currentWord.LastBeat + C.TimingWindowGoodSec * SongManager.Instance.BeatsPerSec;
            if (_switchNextWordThreshold > _nextWord.Beat - C.TimingWindowGoodSec * SongManager.Instance.BeatsPerSec)
                _switchNextWordThreshold = (_currentWord.LastBeat + _nextWord.Beat) / 2;
        }

        private void CheckNextWord()
        {
            if (SongManager.Instance.SongPosBeats > _currentCharMissThreshold)
            {
                _currentWord.CurrentInputNote.Result = NoteResult.Miss;
                OnMiss?.Invoke();
                AdvanceChar();
            }
            
            if (_lastWordReached || SongManager.Instance.SongPosBeats < _switchNextWordThreshold)
                return;
        
            // Next word threshold passed!
            _currentWord.Finish();
            AdvanceWord();
        }

        private void Update()
        {
            if (!BeatmapStarted)
                return;
        
            CheckNextWord();
            if (_currentWord.CheckPassedNote(SongManager.Instance.SongPosBeats))
                OnNote?.Invoke();
        
            Track.Instance.UpdateProgress(SongManager.Instance.SongPosBeats);
        }

        private void Tap() { }

        private void OnChar(char character)
        {
            if (_currentWord.Finished)
                return;
        
            var currentCharNote = (RuntimeNote) _currentWord.CurrentInputNote; // c# 8

            var beatTiming = SongManager.Instance.SongPosBeats - currentCharNote.BeatAbs;
            var timingMs = beatTiming / SongManager.Instance.BeatsPerSec * 1000;
            var timingMsAbs = Math.Abs(timingMs);

            if (timingMsAbs > C.TimingWindowGood)
                return;
            
            if (character == currentCharNote.Char)
            {
                currentCharNote.ResultTiming = timingMs;
                var result = timingMsAbs < C.TimingWindowPerfect 
                    ? NoteResult.HitPerfect 
                    : (timingMs < 0 ? NoteResult.HitEarly : NoteResult.HitLate);
                currentCharNote.Result = result;
                OnHit?.Invoke(currentCharNote.Char, result, beatTiming);
            }
            else
            {
                currentCharNote.Result = NoteResult.WrongChar;
                OnHit?.Invoke(currentCharNote.Char, NoteResult.WrongChar, null);
            }

            AdvanceChar();
        }

        private void AdvanceChar()
        {
            var newCurrentChar = _currentWord.AdvanceInputNote();
            OnChangeCurrentChar?.Invoke(newCurrentChar);
            if (newCurrentChar == null)
            {
                _currentCharMissThreshold = null;
                if (_lastWordReached)
                    BeatmapFinished = true;
            }
            else
                SetCurrentCharMissThreshold();
        }

        private void SetCurrentCharMissThreshold()
        {
            _currentCharMissThreshold =
                _currentWord.Beat + _currentWord.CurrentInputNote.Beat + C.TimingWindowGoodSec * SongManager.Instance.BeatsPerSec;
        }

        private void SongFinished()
        {
            BeatmapFinished = true;
            var result = new BeatmapScore(RuntimeWords.GetNotes().ToList());
            Local.Scores.AddScore(CurrentBeatmap.id, result);
            Local.CommitScores();
            GameManager.EndGameplay(result);
        }

        private void OnEnable()
        {
            PlayerInputManager.OnTap += Tap;
            PlayerInputManager.OnChar += OnChar;
            SongManager.OnSongFinished += SongFinished;
        }

        public void BackToMainMenu() => GameManager.ToMainMenu();

        public static event Action OnNote;
        public static event Action<char, NoteResult, float?> OnHit; // Pass in char, noteresult, timing
        public static event Action OnMiss;
        public static event Action<float> OnChangeCurrentWord; // Pass in beat
        public static event Action<int?> OnChangeCurrentChar; // Pass in index
    }
}