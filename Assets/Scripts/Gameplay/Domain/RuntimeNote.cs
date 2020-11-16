using Shared.Domain;

namespace Gameplay.Domain
{
    public class RuntimeNote : ParsedNote
    {
        public NoteResult? Result;
        public float? ResultTiming;
        public readonly float BeatAbs; // Beat relative to the start of the song instead of the start of the word
        public RuntimeNote(ParsedNote baseNote, float beatAbs)
        {
            Beat = baseNote.Beat;
            Char = baseNote.Char;
            BeatAbs = beatAbs;
        }
    }
    
    public enum NoteResult
    {
        HitPerfect,
        HitNear,
        WrongChar,
        Miss,
    }
}