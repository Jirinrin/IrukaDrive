using System;
using System.Linq;
using JetBrains.Annotations;
using Shared.Domain;
using Tools;
using UnityEngine;

namespace Gameplay.Domain
{
    public class RuntimeWord : ParsedWord<RuntimeChar>
    {
        public readonly string text;

        private int _inputNoteIndex;
        private int _noteIndex;
        [CanBeNull] public RuntimeChar currentInputChar;
        [CanBeNull] public RuntimeChar currentChar;
        public bool Finished => _inputNoteIndex >= CharNotes.Count;

        private bool _passed;

        public RuntimeWord(BeatmapWord word)
        {
            if (word.text.Match(@"^xxx*$").Success)
                word = word.Clone(textOverride: Dict.DictEn.GetRandomWordOfLength(word.text.Length));

            CharNotes = word.ParseNotes().Select(note => new RuntimeChar(note, word.beat + note.beat)).ToList();
            if (!CharNotes.Any())
                throw new Exception("Empty word: " + this);
        
            Beat = word.beat;
            LastBeat = word.LastBeat();
            currentInputChar = CharNotes[0];
            currentChar = CharNotes[0];

            text = word.text;
        }

        // あくまでのsafeguard: in theory this should never trigger
        public void Finish(bool errorOnTrigger = true)
        {
            if (Finished)
                return;

            if (errorOnTrigger)
                Debug.LogError($"Word was unfinished, which it shouldn't be!! {text} -- {currentInputChar.character}");

            do currentInputChar.result = NoteResult.Miss;
            while (AdvanceInputNote() != null);
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

        public bool CheckPassedNote(float beatTime)
        {
            if (_passed || !(currentChar?.beatAbs < beatTime))
                return false;

            // Passed new note!
            _noteIndex++;
            if (_noteIndex >= CharNotes.Count)
            {
                _passed = true;
                currentChar = null;
            }
            else
                currentChar = CharNotes[_noteIndex];

            // todo: maybe make this flip in-between notes or sth
            // if (_inputNoteIndex < _noteIndex && CurrentInputNote?.Result == null)
            // {
            //     _inputNoteIndex = _noteIndex;
            //     SetCurrentInputNote(_passed ? (int?) null : _inputNoteIndex);
            // }

            return true;
        }
    }
    
    // todo: chord version + base; difficult because multiple inheritance kinda needed
}