using System;
using Shared.Domain;
using TMPro;
using UnityEngine;

namespace BeatmapEditor.Components
{
    public class EditorCharObject : MonoBehaviour
    {
        [NonSerialized] public ParsedNote note;
        [NonSerialized] public TextMeshProUGUI obj;

        public void Init(ParsedNote note)
        {
            obj = GetComponent<TextMeshProUGUI>();
            this.note = note;
            Text = note.character.ToString();
        }
        
        public string Text
        {
            get => obj.text;
            set => obj.text = value;
        }

        public void Cleanup()
        {
            obj = null;
            note = null;
        }
    }
}