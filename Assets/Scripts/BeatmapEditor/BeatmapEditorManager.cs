using System.Collections.Generic;

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
        private BeatmapWord _word;
        
        public List<ParsedNote> CharNotes;

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
            set
            {
                _word.beat = value;
                CharNotes = _word.ParseNotes();
            }
        }
        public float LastBeat => _word.LastBeat();
        public float BeatInterval
        { 
            get => _word.beatInterval;
            set
            {
                _word.beatInterval = value;
                CharNotes = _word.ParseNotes();
            }
        }

        public EditorWord(BeatmapWord word)
        {
            _word = word;
            CharNotes = word.ParseNotes();
        }
    }
}
