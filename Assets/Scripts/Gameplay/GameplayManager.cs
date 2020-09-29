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

        private const float SecondsBeforeNextWord = 1f;
        private const float MinimumSecondsAfterWord = .5f;
        private const float MaxJudgementBeatTimeWindow = 2f;

        private List<RuntimeWord>.Enumerator _wordsIterator;
        private RuntimeWord _currentWord;
        private RuntimeWord _nextWord;
        private float _switchNextWordThreshold;

        [NonSerialized] public bool BeatmapStarted;
        [NonSerialized] public bool BeatmapFinished;
        private bool _lastWordReached;

        private void Start()
        {
            // For dev
            if (CurrentBeatmap == null)
                PrepGameplay(SerializationHelpers.LoadBeatmap(@"C:\Users\侍鈴\Documents\Unity\IrukaDive\Assets\Beatmaps\Tutorial\bla3.blarr"));

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

        private void SongFinished()
        {
            OnBeatmapSongFinished?.Invoke();
            GameManager.EndGameplay();
        }

        private void OnEnable()
        {
            PlayerInputManager.OnTap += Tap;
            PlayerInputManager.OnChar += OnChar;
            SongManager.OnSongFinished += SongFinished;
        }

        public void ChangeCurrentChar(int? val)
        {
            if (_lastWordReached && val == null) 
                BeatmapFinished = true;
            OnChangeCurrentChar?.Invoke(val);
        }
        
        public void BackToMainMenu() => GameManager.ToMainMenu();

        public static event Action OnNote;
        public static event Action<char, NoteResult, float?> OnHit; // Pass in timing, char, noteresult
        public static event Action OnBeatmapSongFinished; // todo: remove, it's not necessary
        public static event Action<float> OnChangeCurrentWord; // Pass in beat
        public static event Action<int?> OnChangeCurrentChar; // Pass in index
    }
}