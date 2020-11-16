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
        public int score; // Range 0-10000000
        public int perfects;
        public int earlies;
        public int lates;
        public int misses;
        public int maxCombo; // todo: implement
        
        public BeatmapScore(IReadOnlyCollection<RuntimeNote> notes)
        {
            var perfectNotes = notes.Where(n => n.Result == NoteResult.HitPerfect);
            var earlyNotes = notes.Where(n => n.Result == NoteResult.HitEarly);
            var lateNotes = notes.Where(n => n.Result == NoteResult.HitLate);
            var missNotes = notes.Where(n => n.Result == NoteResult.WrongChar || n.Result == NoteResult.Miss);
            
            perfects = perfectNotes.Count();
            earlies = earlyNotes.Count();
            lates = lateNotes.Count();
            misses = missNotes.Count();
            
            var scoreNormalized = (perfects + (earlies + lates) * .5f) / notes.Count;
            score = Mathf.FloorToInt(scoreNormalized * 10000000);
        }
        
        public int CompareTo(BeatmapScore other) => score.CompareTo(other.score);

        public override string ToString() => score.ToString(CultureInfo.CurrentCulture);
    }
}