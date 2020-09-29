using System.Collections.Generic;
using System.Linq;
using Gameplay;
using Gameplay.Domain;
using Shared;
using Shared.Domain;

namespace Tools
{
    public static class BeatmapTransformations
    {
        public static List<ParsedNote> ParseNotes(this BeatmapWord word)
        {
            return word.text.ToCharArray().Select((c, i) =>
                new ParsedNote
                {
                    Beat = i * word.beatInterval,
                    Char = c,
                }
            ).ToList();
        }

        public static float LastBeat(this BeatmapWord word) => 
            word.beat + word.BeatWidth();
    
        public static float BeatWidth(this BeatmapWord word) => 
            word.text.Length * word.beatInterval;

        public static IEnumerable<RuntimeNote> GetResults(this IEnumerable<RuntimeWord> words) =>
            words.SelectMany(word => word.CharNotes);
    }
}