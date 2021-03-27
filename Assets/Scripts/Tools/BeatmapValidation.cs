using System.Collections.Generic;
using Shared;
using Shared.Domain;

namespace Tools
{
    public static class BeatmapValidation
    {
        public class InvalidChordResult
        {
            public int index;
            public bool tooLarge;
            public bool repeated;
            public InvalidChordResult(int i) => index = i;
        }

        public class ValidationResult
        {
            public bool isEmpty;
            public readonly List<int> overlaps = new List<int>();
            public readonly List<int> emptyWords = new List<int>();
            public readonly List<InvalidChordResult> invalidChords = new List<InvalidChordResult>();
            public bool IsValid => !isEmpty && overlaps.IsEmpty() && emptyWords.IsEmpty() && invalidChords.IsEmpty();
        }

        // todo: write tests
        public static ValidationResult ValidateBeatmap(Beatmap b)
        {
            b.SortWords();
            var w = b.words;
            if (w.IsEmpty())
                return new ValidationResult {isEmpty = true};

            var res = new ValidationResult();
            for (var i = 1; i < w.Count; i++)
            {
                var currentWord = w[i];
                var prevWord = w[i - 1];
                if (currentWord.beat <= prevWord.LastBeat())
                    res.overlaps.Add(i);

                if (currentWord.text.Length == 0)
                    res.emptyWords.Add(i);

                if (currentWord.isChord)
                {
                    InvalidChordResult chRes = null;
                    if (currentWord.text.Length > C.MaxChordSize)
                        (chRes = new InvalidChordResult(i)).tooLarge = true;
                    if (new HashSet<char>(currentWord.text.ToCharArray()).Count < currentWord.text.Length)
                        (chRes ??= new InvalidChordResult(i)).repeated = true;
                    if (chRes != null)
                        res.invalidChords.Add(chRes);
                }
            }

            return res;
        }
    }
}