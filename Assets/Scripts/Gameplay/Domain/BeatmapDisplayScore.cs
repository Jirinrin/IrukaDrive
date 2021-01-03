using System;
using Shared.Domain;

namespace Gameplay.Domain
{
    public class BeatmapDisplayScore : BeatmapScore
    {
        public int comboCounter;

        public BeatmapDisplayScore(int notesCount) : base(notesCount)
        {
            OnScoreChange?.Invoke(0);
        }

        public override void AddNoteResult(NoteResult noteResult)
        {
            base.AddNoteResult(noteResult);

            var prevComboCounter = comboCounter;
            
            if (noteResult == NoteResult.WrongChar || noteResult == NoteResult.Miss)
                comboCounter = 0;
            else if (++comboCounter > maxCombo)
            {
                maxCombo = comboCounter;
                OnMaxComboChange?.Invoke(maxCombo);
            }
            
            if (comboCounter != prevComboCounter)
                OnComboChange?.Invoke(comboCounter);
            
            OnScoreChange?.Invoke(Score);
        }
        
        public static event Action<int> OnScoreChange;
        public static event Action<int> OnComboChange;
        public static event Action<int> OnMaxComboChange;
    }
}