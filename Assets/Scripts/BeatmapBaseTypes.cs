using System.Collections.Generic;
using System.Linq;

public class ParsedNote
{
    public float Beat;
    public char Char;
}

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
}
