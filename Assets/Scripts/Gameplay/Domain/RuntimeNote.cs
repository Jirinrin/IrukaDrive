using Shared.Domain;

namespace Gameplay.Domain
{
    public class RuntimeChar : ParsedChar
    {
        public NoteResult result = NoteResult.Null;
        public float? resultTiming;
        public char wrongChar;
        public readonly float beatAbs; // Beat relative to the start of the song instead of the start of the word
        public RuntimeChar(ParsedChar baseChar, float beatAbs)
        {
            beat = baseChar.beat;
            character = baseChar.character;
            this.beatAbs = beatAbs;
        }
    }
    
    public enum NoteResult
    {
        Null = -1,
        Miss = 0,
        HitPerfect = 1,
        HitEarly = 2,
        HitLate = 3,
        WrongChar = 4,
    }

    public enum ChordResult
    {
        AllPerfect = 5,
        AllGood = 6,
        AllWrong = 7,
        Partial = 8,
    }
}