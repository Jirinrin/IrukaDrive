using System;
using System.Linq;
using JetBrains.Annotations;
using Shared.Domain;
using Tools;
using UnityEngine;

namespace Gameplay.Domain
{
    public sealed class RuntimeWord : ParsedWord<RuntimeChar>
    {
        private readonly string _text;
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
            _text = word.text;

            currentInputChar = CharNotes[0];

            if (IsChord)
                _remainingChordText = _text;
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

        public (bool hit, RuntimeChar c) HitOnWord(char c)
        {
            if (Finished) return (false, null);

            return (c == currentInputChar?.character, currentInputChar);
        }
        
        // CHORD STUFF

        public (bool hit, RuntimeChar currentInputChar, bool finished) HitOnChord(char c)
        {
            if (Finished) return (false, null, true);

            var currentC = currentInputChar;
            AdvanceInputNote();
            
            var i = _remainingChordText.IndexOf(c);
            if (i == -1)
                return (false, currentC, Finished);

            _remainingChordText = _remainingChordText.Remove(i, 1);

            return (true, currentC, Finished);
        }

        public void FinishChord(Action<RuntimeChar, NoteResult> setNoteResult)
        {
            if (Finished)
            {
                Debug.LogWarning($"Tried to finish already finished chord: {_text}");
                return;
            }

            do setNoteResult(currentInputChar, NoteResult.Miss);
            while (AdvanceInputNote() != null);
        }
        
        // MISC
        
        // あくまでのsafeguard: in theory this should never trigger
        public void FinishWord(bool errorOnTrigger = true)
        {
            if (Finished || IsChord)
                return;

            if (errorOnTrigger)
                Debug.LogError($"Word was unfinished, which it shouldn't be!! {_text} -- {currentInputChar!.character}");

            do currentInputChar!.result = NoteResult.Miss;
            while (AdvanceInputNote() != null);
        }
        
        public bool CheckPassedNote(float beatTime)
        {
            if (_passed || (IsChord ? Beat : _currentChar?.beatAbs) > beatTime)
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