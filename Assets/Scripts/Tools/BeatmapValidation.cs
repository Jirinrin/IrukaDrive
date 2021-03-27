using System.Collections.Generic;
using Shared;
using Shared.Domain;
using UnityEngine;

namespace Tools
{
    public static class BeatmapValidation
    {
        public class InvalidChordResult
        {
            public readonly BeatmapWord word;
            public bool tooLarge;
            public bool repeated;
            public InvalidChordResult(BeatmapWord w) => word = w;
        }

        public class ValidationResult
        {
            public bool isEmpty;
            public readonly List<(BeatmapWord, BeatmapWord)> overlaps = new List<(BeatmapWord, BeatmapWord)>();
            public readonly List<BeatmapWord> emptyWords = new List<BeatmapWord>();
            public readonly List<InvalidChordResult> invalidChords = new List<InvalidChordResult>();
            public bool IsValid => !isEmpty && overlaps.IsEmpty() && emptyWords.IsEmpty() && invalidChords.IsEmpty();
        }

        // todo: write tests to verify this method works as expected
        public static ValidationResult ValidateBeatmap(Beatmap b)
        {
            b.SortWords();
            var w = b.words;
            if (w.IsEmpty())
                return new ValidationResult {isEmpty = true};

            var res = new ValidationResult();
            for (var i = 0; i < w.Count; i++)
            {
                var word = w[i];

                if (word.text.Length == 0)
                    res.emptyWords.Add(word);

                if (i > 0 && word.beat <= w[i-1].LastBeat())
                    res.overlaps.Add((w[i-1], word));

                if (word.isChord)
                {
                    InvalidChordResult chRes = null;
                    if (word.text.Length > C.MaxChordSize)
                        (chRes = new InvalidChordResult(word)).tooLarge = true;
                    if (new HashSet<char>(word.text.ToCharArray()).Count < word.text.Length)
                        (chRes ??= new InvalidChordResult(word)).repeated = true;
                    if (chRes != null)
                        res.invalidChords.Add(chRes);
                }
            }

            return res;
        }
    }
}