using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatmapEditor
{
    public class BeatmapEditorManager : MonoBehaviour
    {
        /* [NonSerialized] public static */ public Beatmap currentBeatmap;

        private void Start()
        {
            var editorWords = currentBeatmap.words.Select(word => new EditorWord(word)).ToList();
            EditorTrack.Instance.InitTrack(editorWords);
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
