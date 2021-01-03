using Shared.Domain;
using UnityEngine;

namespace Gameplay.Domain
{
    public class BeatmapDisplayScore : BeatmapScore
    {
        public int comboCounter;
        
        public BeatmapDisplayScore(int notesCount) : base(notesCount) { }

        public override void AddNoteResult(NoteResult noteResult)
        {
            base.AddNoteResult(noteResult);
            
            if (noteResult == NoteResult.WrongChar || noteResult == NoteResult.Miss)
                comboCounter = 0;
            else
                maxCombo = Mathf.Max(++comboCounter, maxCombo);
        }
    }
}