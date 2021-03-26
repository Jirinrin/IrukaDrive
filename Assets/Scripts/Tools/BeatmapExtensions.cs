using System.Collections.Generic;
using System.Linq;
using Gameplay.Domain;
using Shared.Domain;

namespace Tools
{
    public static class BeatmapExtensions
    {
        public static List<ParsedChar> ParseNotes(this BeatmapWord word)
        {
            if (word.isChord)
                return word.text.ToCharArray()
                    .Select(c => new ParsedChar { character = c })
                    .ToList();
            else
                return word.text.ToCharArray()
                    .Select((c, i) => new ParsedChar { beat = i * word.beatInterval, character = c })
                    .Where(note => note.character != ' ')
                    .ToList();
        }

        public static float LastBeat(this BeatmapWord word) => 
            word.beat + word.BeatWidth();
    
        public static float BeatWidth(this BeatmapWord word) => 
            word.isChord ? 0 : (word.text.Length-1) * word.beatInterval;

        public static IEnumerable<RuntimeChar> GetNotes(this IEnumerable<RuntimeWord> words) =>
            words.SelectMany(word => word.CharNotes);

        public static float SecToBeats(this Song s, float seconds) => (seconds - s.beatOffset) * s.BeatsPerSec;
        public static float BeatsToSecs(this Song s, float beats) => beats / s.BeatsPerSec + s.beatOffset;
    }
}