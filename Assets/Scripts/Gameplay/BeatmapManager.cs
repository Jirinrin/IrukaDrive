using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Domain;
using Shared.Domain;
using Tools;
using Tools.Commons;

namespace Gameplay
{
    public class BeatmapManager : Singleton<BeatmapManager>
    {
        [NonSerialized] public Beatmap currentBeatmap;
    
        private const float SecondsBeforeNextWord = 1f;
        private const float MinimumSecondsAfterWord = .5f;
        private const float MaxJudgementBeatTimeWindow = 2f;

        private IEnumerable<RuntimeWord> _runtimeWords;
    
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
        
            _wordsIterator = _runtimeWords.ToList().GetEnumerator();
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
}