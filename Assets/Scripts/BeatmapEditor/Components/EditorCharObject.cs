using System;
using Shared.Domain;
using TMPro;
using UnityEngine;

namespace BeatmapEditor.Components
{
    public class EditorCharObject : MonoBehaviour
    {
        [NonSerialized] public ParsedNote Note;
        [NonSerialized] public TextMeshProUGUI Obj;

        public void Init(ParsedNote note)
        {
            Obj = GetComponent<TextMeshProUGUI>();
            Note = note;
            Text = note.Char.ToString();
        }
        
        public string Text
        {
            get => Obj.text;
            set => Obj.text = value;
        }

        public void Cleanup()
        {
            Obj = null;
            Note = null;
        }
    }
}