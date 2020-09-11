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
            EditorTrack.Instance.InitTrack(currentBeatmap.words.Select(word => 
                new EditorWord(word.ParseNotes(currentBeatmap.beatsPerBar))
            ).ToList());
        }

    }
    
    public class EditorNote : ParsedNote
    {
        public EditorNote(ParsedNote baseNote)
        {
            Beat = baseNote.Beat; 
            Char = baseNote.Char;
        }
        public NoteResult? Result;
        public float? ResultTiming;
    }
    
    public class EditorWord
    {
        public float Beat;
        public List<EditorNote> CharNotes;
        
        public EditorWord(List<ParsedNote> charNotes)
        {
            CharNotes = charNotes.Select(note => new EditorNote(note)).ToList();
            Beat = CharNotes.First().Beat;
        }
    }
}
