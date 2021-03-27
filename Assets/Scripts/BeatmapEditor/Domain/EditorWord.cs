using System.Linq;
using Shared;
using Shared.Domain;
using Tools;

namespace BeatmapEditor.Domain
{
    public class EditorWord : ParsedWord<ParsedChar>
    {
        private readonly BeatmapWord _word;

        // For this the original CharNotes cannot be kept
        public string Text
        {
            get => _word.isChord ? $"[[{_word.text}" : _word.text;
            set
            {
                // Readable regex: /^\[\[(.{0,5})/
                var chordMatch = value.Match($@"^\[\[(.{{1,{C.MaxChordSize}}})");
                // todo: also do such a thing for random word of length? e.g. {3} like syntax
                _word.isChord = chordMatch.Success;
                _word.text = _word.isChord ? chordMatch.Groups[1].Value : value;
                CharNotes = _word.ParseNotes();
            }
        }
        public override float Beat
        {
            get => _word.beat;
            set =>_word.beat = value;
        }
        public float BeatInterval
        {
            get => _word.beatInterval;
            set
            {
                _word.beatInterval = value;
                var newCharNotes = _word.ParseNotes();
                CharNotes = CharNotes.Select((note, i) =>
                {
                    note.beat = newCharNotes[i].beat;
                    return note;
                }).ToList();
            }
        }
        public override float LastBeat => _word.LastBeat();
        public float BeatWidth => _word.BeatWidth();
        public override bool IsChord => _word.isChord;

        public EditorWord(BeatmapWord word)
        {
            _word = word;
            CharNotes = _word.ParseNotes();
        }

        public void Delete() => BeatmapEditorManager.currentBeatmap.words.Remove(_word);

        public BeatmapWord CloneWord(float? beat = null) => _word.Clone(beat);
    }
}