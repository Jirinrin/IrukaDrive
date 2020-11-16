using System;
using System.Linq;
using Shared.Domain;
using Tools;
using UnityEngine;

namespace Gameplay.Domain
{
    public class RuntimeWord : ParsedWord<RuntimeNote>
    {
        public readonly float LastBeat;

        private int _inputNoteIndex;
        private int _noteIndex;
        public RuntimeNote CurrentInputNote; // c# 8: add nullable question mark
        public RuntimeNote CurrentNote; // c# 8: add nullable question mark
        public bool Finished => _inputNoteIndex >= CharNotes.Count;

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

        public void Finish()
        {
            if (Finished)
                return;

            do CurrentInputNote.Result = NoteResult.Miss;
            while (AdvanceInputNote() != null);
        }

        private void SetCurrentInputNote(int? index)
        {
            CurrentInputNote = index == null ? null : CharNotes[(int) index];
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
}