using Shared.Domain;

namespace Gameplay.Domain
{
    public class RuntimeNote : ParsedNote
    {
        public NoteResult? result;
        public float? resultTiming;
        public readonly float beatAbs; // Beat relative to the start of the song instead of the start of the word
        public RuntimeNote(ParsedNote baseNote, float beatAbs)
        {
            beat = baseNote.beat;
            character = baseNote.character;
            this.beatAbs = beatAbs;
        }
    }
    
    public enum NoteResult
    {
        Miss = 0,
        HitPerfect = 1,
        HitEarly = 2,
        HitLate = 3,
        WrongChar = 4,
    }
}