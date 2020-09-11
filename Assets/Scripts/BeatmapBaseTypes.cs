using System.Collections.Generic;
using System.Linq;

public class ParsedNote
{
    public float Beat;
    public char Char;
}

public static class BeatmapTransformations
{
    public static List<ParsedNote> ParseNotes(this BeatmapWord word, int beatsPerBar)
    {
        return word.text.ToCharArray().Select((c, i) =>
            new ParsedNote
            {
                Beat = word.bar * beatsPerBar + word.beat + i * word.beatInterval,
                Char = c
            }
        ).ToList();
    }
}
