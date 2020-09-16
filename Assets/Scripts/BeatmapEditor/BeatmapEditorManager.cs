using System.Collections.Generic;
using System.Linq;

namespace BeatmapEditor
{
    public class BeatmapEditorManager : Singleton<BeatmapEditorManager>
    {
        /* [NonSerialized] public static */ public Beatmap currentBeatmap;

        private void Start()
        {
            EditorTrack.Instance.InitTrack(currentBeatmap);
        }
    }

    public class EditorWord
    {
        private readonly BeatmapWord _word;

        public List<ParsedNote> CharNotes;

        // For this the original CharNotes cannot be kept
        public string Text
        { 
            get => _word.text;
            set
            {
                _word.text = value;
                CharNotes = _word.ParseNotes();
            }
        }
        public float Beat
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
                    note.Beat = newCharNotes[i].Beat;
                    return note;
                }).ToList();
            }
        }
        public float LastBeat => _word.LastBeat();
        public float BeatWidth => _word.BeatWidth();

        public EditorWord(BeatmapWord word)
        {
            _word = word;
            CharNotes = _word.ParseNotes();
        }

        public void Delete() => BeatmapEditorManager.Instance.currentBeatmap.words.Remove(_word);
    }
}
