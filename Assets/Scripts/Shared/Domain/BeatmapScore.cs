using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Gameplay.Domain;
using UnityEngine;

namespace Shared.Domain
{
    [Serializable]
    public class BeatmapScore : IComparable<BeatmapScore>
    {
        private const int MaxScore = 10000000;
        
        private float _scorePerNote;
        
        public int totalNotes;
        public int perfects;
        public int earlies;
        public int lates;
        public int misses;
        public int maxCombo; // todo: implement
        
        public int Score => Mathf.FloorToInt((perfects + (earlies + lates) * .5f) * _scorePerNote); // Range 0-10000000
        
        public BeatmapScore(IReadOnlyCollection<RuntimeNote> notes)
        {
            _scorePerNote = (float)MaxScore / notes.Count;

            foreach (var note in notes)
            {
                if (note.result == null)
                    Debug.LogError("Found null note in results");
                AddNoteResult(note.result ?? NoteResult.Miss);
            }
            
            perfects = notes.Count(n => n.result == NoteResult.HitPerfect);
            earlies = notes.Count(n => n.result == NoteResult.HitEarly);
            lates = notes.Count(n => n.result == NoteResult.HitLate);
            misses = notes.Count(n => n.result == NoteResult.WrongChar || n.result == NoteResult.Miss);
        }

        public BeatmapScore(int notesCount)
        {
            _scorePerNote = (float)MaxScore / notesCount;
        }

        public void AddNoteResult(NoteResult noteResult)
        {
            switch (noteResult)
            {
                case NoteResult.HitPerfect:
                    perfects++;
                    break;
                case NoteResult.HitEarly:
                    earlies++;
                    break;
                case NoteResult.HitLate:
                    lates++;
                    break;
                case NoteResult.Miss:
                case NoteResult.WrongChar:
                    misses++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(noteResult), noteResult, null);
            }
        }

        public int CompareTo(BeatmapScore other) => Score.CompareTo(other.Score);

        public override string ToString() => Score.ToString(CultureInfo.CurrentCulture);
    }
}