using System;
using System.Linq;
using JetBrains.Annotations;
using Shared;
using Shared.Domain;
using Tools;
using UnityEngine;

namespace Gameplay.Domain
{
    public sealed class RuntimeWord : ParsedWord<RuntimeChar>
    {
        public readonly string text;
        private bool _passed;
        public bool Finished => _inputNoteIndex >= CharNotes.Count;
        public float CurrentBeat => IsChord ? Beat : (currentInputChar?.beatAbs ?? Beat);
        public bool WasHit => _inputNoteIndex > 0;

        private int _inputNoteIndex;
        [CanBeNull] public RuntimeChar currentInputChar;

        // Exclusive to Word
        private int _noteIndex;
        [CanBeNull] private RuntimeChar _currentChar;
        
        // Exclusive to Chord
        private string _remainingChordText;
        private string _chordWrongHits = "";

        public RuntimeWord(BeatmapWord word)
        {
            if (word.text.Match(@"^xxx*$").Success)
                word = word.Clone(textOverride: Dict.DictEn.GetRandomWordOfLength(word.text.Length));
            
            Beat = word.beat;
            LastBeat = word.LastBeat();
            IsChord = word.isChord;
            CharNotes = word.ParseNotes().Select(note => new RuntimeChar(note, word.beat + note.beat)).ToList();
            if (!CharNotes.Any())
                throw new Exception("Empty word: " + this);
            text = word.text;

            currentInputChar = CharNotes[0];

            if (IsChord)
                _remainingChordText = text;
            else
                _currentChar = CharNotes[0];
        }

        private void SetCurrentInputNote(int? index)
        {
            currentInputChar = index == null ? null : CharNotes[(int) index];
        }

        // Returns null if finished
        public int? AdvanceInputNote()
        {
            if (Finished)
                return null;

            // todo: register here if missed a note?

            _inputNoteIndex++;
            if (Finished)
            {
                SetCurrentInputNote(null);
                return null;
            }

            SetCurrentInputNote(_inputNoteIndex);
            return _inputNoteIndex;
        }
        
        // WORD STUFF

        private static NoteResult TimingToResult(float timingMs) =>
            Math.Abs(timingMs) < C.TimingWindowPerfect
                ? NoteResult.HitPerfect
                : timingMs < 0 ? NoteResult.HitEarly : NoteResult.HitLate;

        public RuntimeChar HitOnWord(char c, float timingMs)
        {
            if (Finished || currentInputChar == null)
                return null;

            currentInputChar.resultTiming = timingMs;
            if (c == currentInputChar.character)
                currentInputChar.result = TimingToResult(timingMs);
            else
            {
                currentInputChar.result = NoteResult.WrongChar;
                currentInputChar.wrongChar = c;
            }

            return currentInputChar;
        }
        
        // CHORD STUFF

        // Returns whether the chord is finished
        public bool HitOnChord(char c, float timingMs)
        {
            if (Finished) return true;

            AdvanceInputNote();
            
            var i = _remainingChordText.IndexOf(c);
            if (i == -1)
            {
                _chordWrongHits += c;
                return Finished;
            }

            _remainingChordText = _remainingChordText.Remove(i, 1);

            var charNote = CharNotes.First(ch => ch.character == c);
            charNote.result = TimingToResult(timingMs);
            charNote.resultTiming = timingMs;

            return Finished;
        }

        public void FinishChord()
        {
            if (Finished)
            {
                Debug.LogWarning($"Tried to finish already finished chord: {text}");
                return;
            }

            _inputNoteIndex = CharNotes.Count;
            var chordWrongIndex = -1;

            foreach (var ch in CharNotes.Where(ch => ch.result == NoteResult.Null))
            {
                if (++chordWrongIndex < _chordWrongHits.Length)
                {
                    ch.result = NoteResult.WrongChar;
                    ch.wrongChar = _chordWrongHits[chordWrongIndex];
                }
                else
                    ch.result = NoteResult.Miss;
            }
        }
        
        // MISC
        
        // あくまでのsafeguard: in theory this should never trigger
        public void FinishWord(bool errorOnTrigger = true)
        {
            if (Finished || IsChord)
                return;

            if (errorOnTrigger)
                Debug.LogError($"Word was unfinished, which it shouldn't be!! {text} -- {currentInputChar!.character}");

            do currentInputChar!.result = NoteResult.Miss;
            while (AdvanceInputNote() != null);
        }
        
        public bool CheckPassedNote(float beatTime)
        {
            if (_passed || CurrentBeat > beatTime)
                return false;

            // Passed new note!
            if (IsChord)
                _passed = true;
            else
            {
                _noteIndex++;
                if (_noteIndex >= CharNotes.Count)
                {
                    _passed = true;
                    _currentChar = null;
                }
                else
                    _currentChar = CharNotes[_noteIndex];
            }
            return true;
        }
    }
}