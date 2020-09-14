using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;

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

    public class EditorNote : ParsedNote
    {
        // Purely performance optimization
        [CanBeNull] public TextMeshProUGUI CharObjRef;

        public EditorNote(ParsedNote baseNote)
        {
            Beat = baseNote.Beat;
            Char = baseNote.Char;
        }
    }
    
    public class EditorWord
    {
        private BeatmapWord _word;

        public List<EditorNote> CharNotes;

        private List<EditorNote> GetNotes() => 
            _word.ParseNotes().Select(note => new EditorNote(note)).ToList();

        public string Text
        { 
            get => _word.text;
            set
            {
                _word.text = value;
                CharNotes = GetNotes();
            }
        }
        public float Beat
        { 
            get => _word.beat;
            set
            {
                _word.beat = value;
                CharNotes = GetNotes();
            }
        }
        public float LastBeat => _word.LastBeat();
        public float BeatInterval
        { 
            get => _word.beatInterval;
            set
            {
                _word.beatInterval = value;
                CharNotes = GetNotes();
            }
        }

        public EditorWord(BeatmapWord word)
        {
            _word = word;
            CharNotes = GetNotes();
        }
    }
}
