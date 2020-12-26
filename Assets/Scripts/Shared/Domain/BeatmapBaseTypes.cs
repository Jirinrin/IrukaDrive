using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace Shared.Domain
{
    public class ParsedNote
    {
        public float beat;
        public char character;
    }

    public class WordObject<TWord, TNote> : MonoBehaviour
        where TWord : ParsedWord<TNote>
        where TNote : ParsedNote // todo: a way to infer this?
    {
        [CanBeNull] public List<CharObject> charObjRefs;

        public TWord word;
        
        protected float _beatSpacing;
        
        protected void RefreshWord()
        {
            foreach (var charObj in charObjRefs)
                charObj.transform.localPosition = new Vector3(_beatSpacing * charObj.note.beat, 0, 0);
        }

        public void UpdateSpacing(float newSpacing)
        {
            _beatSpacing = newSpacing;
            RefreshWord();
        }
    }

    public class CharObject : MonoBehaviour
    {
        [NonSerialized] public ParsedNote note;
        [NonSerialized] public TextMeshProUGUI obj;

        public void Init(ParsedNote note)
        {
            this.note = note;
            obj = GetComponent<TextMeshProUGUI>();
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
    
    public abstract class ParsedWord<TNote> where TNote : ParsedNote
    {
        public List<TNote> CharNotes { get; set; }
        public virtual float Beat { get; set; }
    }
}
