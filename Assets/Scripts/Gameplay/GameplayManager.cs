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
        public static bool AutoPlay { get; private set; }
        [CanBeNull] public static IEnumerable<RuntimeWord> RuntimeWords { get; private set; }
        
        public static void PrepGameplay(Beatmap beatmap, float startTime = 0, bool comingFromEditor = false, bool autoplay = false)
        {
            CurrentBeatmap = beatmap;
            BeatmapStartTime = startTime;
            EditorPlay = comingFromEditor;
            AutoPlay = autoplay;
            RuntimeWords = CurrentBeatmap.words.Select(word => new RuntimeWord(word)).ToList();
        }

        private List<RuntimeWord>.Enumerator _wordsIterator;
        private RuntimeWord _currentWord;
        private RuntimeWord _nextWord;
        private float _switchNextWordThreshold;
        private float? _currentNoteMissThreshold;

        [NonSerialized] public bool beatmapStarted;
        [NonSerialized] public bool beatmapFinished;
        private bool _lastWordReached;

        public BeatmapDisplayScore displayScore;

        private void Start()
        {
            // For dev
            if (GameManager.State != GameState.Gameplay)
            {
                GameManager.SetState(GameState.Gameplay);
                PrepGameplay(SerializationHelpers.LoadBeatmap($"{Application.streamingAssetsPath}/DriveCharts/OfficialCharts/SDVX Tutorial/2_advanced.drive"), autoplay: false);
            }

            LoadCurrentBeatmap();
        }

        private async void LoadCurrentBeatmap()
        {
            await SongManager.Instance.LoadSong(CurrentBeatmap.song, BeatmapStartTime, false, CurrentBeatmap.finishTimestamp);
        
            Track.Instance.InitTrack(CurrentBeatmap, RuntimeWords);
        
            displayScore = new BeatmapDisplayScore(CurrentBeatmap.NotesCount);
        
            _wordsIterator = RuntimeWords.ToList().GetEnumerator();

            // Setup the first word coming up
            if (!_wordsIterator.MoveNext()) throw new Exception("Empty beatmap");
            _nextWord = _wordsIterator.Current;

            beatmapStarted = true;
            beatmapFinished = false;
            _lastWordReached = false;
            
            AdvanceWord();
            
            if (BeatmapStartTime > .1f)
            {
                var beatmapStartBeat = CurrentBeatmap.song.SecToBeats(BeatmapStartTime);
                Track.Instance.UpdateProgress(beatmapStartBeat);
                Track.Instance.ForceRefresh();

                while (_switchNextWordThreshold < SongManager.Instance.songPosBeats && !_lastWordReached)
                {
                    _currentWord.FinishWord(false);
                    AdvanceWord(_nextWord.LastBeat >= beatmapStartBeat);
                }
                if (!_currentWord.IsChord)
                    while (_currentNoteMissThreshold < SongManager.Instance.songPosBeats)
                        AdvanceCharInWord();
            }
        }

        private void AdvanceWord(bool triggerEvent = true)
        {
            _currentWord = _nextWord;
            if (triggerEvent)
                OnChangeCurrentWord?.Invoke(_currentWord.Beat);

            SetCurrentCharMissThreshold(_currentWord);
        
            if (!_wordsIterator.MoveNext())
            {
                _lastWordReached = true;
                _nextWord = null;
                return;
            }

            _nextWord = _wordsIterator.Current;
            // Put switch to next current word to x seconds before that word, or in-between the current and next
            _switchNextWordThreshold = _currentWord.LastBeat + C.TimingWindowGoodSec * CurrentBeatmap.BeatsPerSec;
            if (_switchNextWordThreshold > _nextWord.Beat - C.TimingWindowGoodSec * CurrentBeatmap.BeatsPerSec)
                _switchNextWordThreshold = (_currentWord.LastBeat + _nextWord.Beat) / 2;
        }

        private void CheckNextWord()
        {
            // Note missed. If word was not missed _currentNoteMissThreshold will be null making this condition false
            if (SongManager.Instance.songPosBeats > _currentNoteMissThreshold)
            {
                if (_currentWord.IsChord)
                {
                    if (!_currentWord.Finished)
                        OnChordFinished(_currentWord);
                    _currentNoteMissThreshold = null;
                }
                else
                {
                    if (_currentWord.currentInputChar == null)
                        Debug.LogWarning("Current input char should not be null");
                    else
                    {
                        _currentWord.currentInputChar.result = NoteResult.Miss;
                        displayScore.AddNoteResult(_currentWord.currentInputChar.result);
                        OnMiss?.Invoke();
                        AdvanceCharInWord();
                    }
                }
            }
            
            if (_lastWordReached || SongManager.Instance.songPosBeats < _switchNextWordThreshold)
                return;
        
            // Next word threshold passed!
            _currentWord.FinishWord();
            AdvanceWord();
        }

        private void Update()
        {
            if (!beatmapStarted)
                return;

            CheckNextWord();
            if (_currentWord.CheckPassedNote(SongManager.Instance.songPosBeats))
                OnNote?.Invoke();
        
            Track.Instance.UpdateProgress(SongManager.Instance.songPosBeats);

            if (AutoPlay)
            {
                if (_currentWord.CurrentBeat <= SongManager.Instance.songPosBeats + C.TimingWindowPerfectSec*CurrentBeatmap.BeatsPerSec/8f)
                {
                    if (_currentWord.IsChord)
                        foreach (var c in _currentWord.CharNotes)
                            InputManager.Instance.OnKeyboardEvent(c.character);
                    else if (_currentWord.currentInputChar != null)
                        InputManager.Instance.OnKeyboardEvent(_currentWord.currentInputChar.character);
                }
            }
        }

        private void OnChar(char character)
        {
            if (character == ' ') character = C.CustomSpaceChar;
            
            if (_currentWord.Finished)
                return;

            var beatTiming = SongManager.Instance.songPosBeats - _currentWord.CurrentBeat;
            var timingMs = beatTiming / CurrentBeatmap.BeatsPerSec * 1000;

            if (Math.Abs(timingMs) > C.TimingWindowGood)
                return;
            
            if (_currentWord.IsChord)
            {
                var finished = _currentWord.HitOnChord(character, timingMs);
                if (finished)
                    OnChordFinished(_currentWord);
            }
            else
            {
                var c = _currentWord.HitOnWord(character, timingMs);
                displayScore.AddNoteResult(c.result);
                OnHit?.Invoke(c);
                AdvanceCharInWord();
            }
        }

        private void OnChordFinished(RuntimeWord word)
        {
            if (!word.IsChord)
            {
                Debug.LogWarning($"This word isn't even a chord! {word.text}");
                return;
            }

            word.FinishChord();

            var wasHit = _currentWord.WasHit;
            if (wasHit)
                OnHitChord?.Invoke(_currentWord.CharNotes);
            else
                OnMissChord?.Invoke();
            foreach (var ch in _currentWord.CharNotes)
                displayScore.AddNoteResult(ch.result);

            _currentNoteMissThreshold = null;
            if (_lastWordReached)
                beatmapFinished = true;
        }

        private void AdvanceCharInWord()
        {
            var newCurrentChar = _currentWord.AdvanceInputNote();
            OnChangeCurrentChar?.Invoke(newCurrentChar);
            if (newCurrentChar == null) // Word finished
            {
                _currentNoteMissThreshold = null;
                if (_lastWordReached)
                    beatmapFinished = true;
            }
            else
                SetCurrentCharMissThreshold(_currentWord);
        }

        private void SetCurrentCharMissThreshold(RuntimeWord w)
        {
            _currentNoteMissThreshold = w.CurrentBeat + C.TimingWindowGoodSec * CurrentBeatmap.BeatsPerSec;
        }

        private void SongFinished()
        {
            beatmapFinished = true;
            BeatmapScore result = null;
            if (!EditorPlay)
            {
                result = new BeatmapScore(RuntimeWords.GetNotes().ToList());
                Local.Scores.AddScore(CurrentBeatmap.id, result);
                Local.CommitScores();
            }
            GameManager.EndGameplay(result);
        }

        public void ExitGameplay()
        {
            if (EditorPlay)
                GameManager.ToBeatmapEditor();
            else
                GameManager.ToSongSelect();
        }

        private void OnEnable()
        {
            InputManager.OnChar += OnChar;
            InputManager.PressBack += ExitGameplay;
            SongManager.OnSongFinished += SongFinished;
        }
        private void OnDisable()
        {
            InputManager.OnChar -= OnChar;
            InputManager.PressBack -= ExitGameplay;
            SongManager.OnSongFinished -= SongFinished;
        }

        public static event Action OnNote;
        public static event Action<RuntimeChar> OnHit;
        public static event Action<IEnumerable<RuntimeChar>> OnHitChord;
        public static event Action OnMiss;
        public static event Action OnMissChord;
        public static event Action<float> OnChangeCurrentWord; // Pass in beat
        public static event Action<int?> OnChangeCurrentChar; // Pass in index
    }
}