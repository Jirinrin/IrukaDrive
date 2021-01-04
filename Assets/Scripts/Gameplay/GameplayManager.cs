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
        private float? _currentCharMissThreshold;

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
                PrepGameplay(SerializationHelpers.LoadBeatmap($"{Application.streamingAssetsPath}/Beatmaps/Tutorial/advanced.drive"), autoplay: false);
            }

            LoadBeatmap();
        }

        public void LoadBeatmap()
        {
            SongManager.Instance.LoadSong(CurrentBeatmap, BeatmapStartTime);
        
            Track.Instance.InitTrack(CurrentBeatmap, RuntimeWords);
        
            displayScore = new BeatmapDisplayScore(CurrentBeatmap.NotesCount);
        
            _wordsIterator = RuntimeWords.ToList().GetEnumerator();

            // Setup the first word coming up
            if (!_wordsIterator.MoveNext()) throw new Exception("Empty beatmap");
            _nextWord = _wordsIterator.Current;
            AdvanceWord();

            beatmapStarted = true;
            beatmapFinished = false;
            _lastWordReached = false;
            
            if (BeatmapStartTime > .1f)
            {
                var beatmapStartBeat = CurrentBeatmap.SecToBeats(BeatmapStartTime);
                Track.Instance.UpdateProgress(beatmapStartBeat);
                Track.Instance.ForceRefresh();

                while (_switchNextWordThreshold < SongManager.Instance.songPosBeats && !_lastWordReached)
                {
                    _currentWord.Finish(false);
                    AdvanceWord(_nextWord.LastBeat >= beatmapStartBeat);
                }
                while (_currentCharMissThreshold < SongManager.Instance.songPosBeats)
                    AdvanceChar();
            }
        }

        private void AdvanceWord(bool triggerEvent = true)
        {
            _currentWord = _nextWord;
            if (triggerEvent)
                OnChangeCurrentWord?.Invoke(_currentWord.Beat);
            SetCurrentCharMissThreshold();
        
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
            if (SongManager.Instance.songPosBeats > _currentCharMissThreshold)
            {
                SetNoteResult(_currentWord.currentInputNote, NoteResult.Miss);
                OnMiss?.Invoke();
                AdvanceChar();
            }
            
            if (_lastWordReached || SongManager.Instance.songPosBeats < _switchNextWordThreshold)
                return;
        
            // Next word threshold passed!
            _currentWord.Finish();
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
                if (_currentWord?.currentInputNote?.beatAbs <= SongManager.Instance.songPosBeats + C.TimingWindowPerfectSec*CurrentBeatmap.BeatsPerSec/8f)
                    InputManager.Instance.OnKeyboardEvent(_currentWord.currentInputNote.character);
            }
        }

        private void OnChar(char character)
        {
            if (_currentWord.Finished)
                return;
        
            var currentCharNote = _currentWord.currentInputNote;

            var beatTiming = SongManager.Instance.songPosBeats - currentCharNote.beatAbs;
            var timingMs = beatTiming / CurrentBeatmap.BeatsPerSec * 1000;
            var timingMsAbs = Math.Abs(timingMs);

            if (timingMsAbs > C.TimingWindowGood)
                return;
            
            if (character == currentCharNote.character)
            {
                currentCharNote.resultTiming = timingMs;
                var result = timingMsAbs < C.TimingWindowPerfect 
                    ? NoteResult.HitPerfect 
                    : (timingMs < 0 ? NoteResult.HitEarly : NoteResult.HitLate);
                SetNoteResult(currentCharNote, result);
                OnHit?.Invoke(currentCharNote.character, result, beatTiming);
            }
            else
            {
                SetNoteResult(currentCharNote, NoteResult.WrongChar);
                OnHit?.Invoke(character, NoteResult.WrongChar, null);
            }

            AdvanceChar();
        }

        private void SetNoteResult(RuntimeNote note, NoteResult result)
        {
            note.result = result;
            displayScore.AddNoteResult(result);
        }

        private void AdvanceChar()
        {
            var newCurrentChar = _currentWord.AdvanceInputNote();
            OnChangeCurrentChar?.Invoke(newCurrentChar);
            if (newCurrentChar == null)
            {
                _currentCharMissThreshold = null;
                if (_lastWordReached)
                    beatmapFinished = true;
            }
            else
                SetCurrentCharMissThreshold();
        }

        private void SetCurrentCharMissThreshold()
        {
            _currentCharMissThreshold =
                _currentWord.Beat + _currentWord.currentInputNote.beat + C.TimingWindowGoodSec * CurrentBeatmap.BeatsPerSec;
        }

        private void SongFinished()
        {
            beatmapFinished = true;
            var result = new BeatmapScore(RuntimeWords.GetNotes().ToList());
            Local.Scores.AddScore(CurrentBeatmap.id, result);
            Local.CommitScores();
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
        public static event Action<char, NoteResult, float?> OnHit; // Pass in char, note result, timing
        public static event Action OnMiss;
        public static event Action<float> OnChangeCurrentWord; // Pass in beat
        public static event Action<int?> OnChangeCurrentChar; // Pass in index
    }
}