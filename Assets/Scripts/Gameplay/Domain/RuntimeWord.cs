using System;
using System.Linq;
using Shared.Domain;
using Tools;

namespace Gameplay.Domain
{
    public class RuntimeWord : ParsedWord<RuntimeNote>
    {
        public readonly float LastBeat;
    
        private int _inputNoteIndex;
        private int _noteIndex;
        public RuntimeNote CurrentInputNote; // c# 8: add nullable question mark
        public RuntimeNote CurrentNote; // c# 8: add nullable question mark
        public bool Finished;
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

        private void SetCurrentInputNote(int? index)
        {
            GameplayManager.Instance.ChangeCurrentChar(index);
            if (index == null)
                CurrentInputNote = null;
            else
                CurrentInputNote = CharNotes[(int) index]; // c# 8
        }

        public void AdvanceInputNote()
        {
            if (Finished)
                return;
        
            _inputNoteIndex++;
            if (_inputNoteIndex >= CharNotes.Count)
            {
                Finished = true;
                SetCurrentInputNote(null);
                return;
            }

            SetCurrentInputNote(_inputNoteIndex);
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