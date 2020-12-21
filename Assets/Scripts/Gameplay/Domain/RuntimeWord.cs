using System;
using System.Linq;
using Shared.Domain;
using Tools;
using UnityEngine;

namespace Gameplay.Domain
{
    public class RuntimeWord : ParsedWord<RuntimeNote>
    {
        public readonly float lastBeat;

        private int _inputNoteIndex;
        private int _noteIndex;
        public RuntimeNote currentInputNote; // c# 8: add nullable question mark
        public RuntimeNote currentNote; // c# 8: add nullable question mark
        public bool Finished => _inputNoteIndex >= CharNotes.Count;

        private bool _passed;

        public RuntimeWord(BeatmapWord word)
        {
            CharNotes = word.ParseNotes().Select(note => new RuntimeNote(note, word.beat + note.beat)).ToList();
            if (!CharNotes.Any())
                throw new Exception("Empty word: " + this);
        
            Beat = word.beat;
            lastBeat = word.LastBeat();
            currentInputNote = CharNotes[0];
            currentNote = CharNotes[0];
        }

        // あくまでのsafeguard: in theory this should never trigger
        public void Finish(bool errorOnTrigger = true)
        {
            if (Finished)
                return;

            if (errorOnTrigger)
                Debug.LogError("Word was unfinished, which it shouldn't be!!");

            do currentInputNote.result = NoteResult.Miss;
            while (AdvanceInputNote() != null);
        }

        private void SetCurrentInputNote(int? index)
        {
            currentInputNote = index == null ? null : CharNotes[(int) index];
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
            if (_passed || !(currentNote?.beatAbs < beatTime))
                return false;

            // Passed new note!
            _noteIndex++;
            if (_noteIndex >= CharNotes.Count)
            {
                _passed = true;
                currentNote = null;
            }
            else
                currentNote = CharNotes[_noteIndex];

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