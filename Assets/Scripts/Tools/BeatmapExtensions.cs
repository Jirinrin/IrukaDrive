using System.Collections.Generic;
using System.Linq;
using Gameplay.Domain;
using Shared.Domain;

namespace Tools
{
    public static class BeatmapExtensions
    {
        public static List<ParsedNote> ParseNotes(this BeatmapWord word)
        {
            return word.text.ToCharArray()
                .Select((c, i) =>
                    new ParsedNote
                    {
                        beat = i * word.beatInterval,
                        character = c,
                    }
                )
                .Where(note => note.character != ' ')
                .ToList();
        }

        public static float LastBeat(this BeatmapWord word) => 
            word.beat + word.BeatWidth();
    
        public static float BeatWidth(this BeatmapWord word) => 
            (word.text.Length-1) * word.beatInterval;

        public static IEnumerable<RuntimeNote> GetNotes(this IEnumerable<RuntimeWord> words) =>
            words.SelectMany(word => word.CharNotes);
        
        public static float SecToBeats(this Beatmap b, float seconds) => (seconds - b.beatOffset) * b.BeatsPerSec;
        public static float BeatsToSecs(this Beatmap b, float beats) => beats / b.BeatsPerSec + b.beatOffset;
        
        public static void SortWords(this Beatmap b) => b.words = b.words.OrderBy(word => word.beat).ToList();
    }
}