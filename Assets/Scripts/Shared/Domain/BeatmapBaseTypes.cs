using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace Shared.Domain
{
    public class ParsedChar
    {
        public float beat;
        public char character;
    }

    public class WordObject<TWord, TChar> : MonoBehaviour
        where TWord : ParsedWord<TChar>
        where TChar : ParsedChar // todo: a way to infer this?
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

        protected void SetColor(Color color)
        {
            foreach (var charObj in charObjRefs)
                charObj.obj.color = color;
        }

        public void Cleanup([CanBeNull] Action<CharObject> cleanupChar = null)
        {
            OnDestroy?.Invoke();
            
            if (charObjRefs == null)
                return;
            
            foreach (var charObject in charObjRefs)
            {
                cleanupChar?.Invoke(charObject);
                charObject.Cleanup();
            }
            
            charObjRefs = null;
        }

        public event Action OnDestroy;
    }

    public class CharObject : MonoBehaviour
    {
        [NonSerialized] public ParsedChar note;
        [NonSerialized] public TextMeshProUGUI obj;

        public void Init(ParsedChar ch)
        {
            this.note = ch;
            obj = GetComponent<TextMeshProUGUI>();
            Text = ch.character.ToString();
        }
        
        public string Text
        {
            get => obj.text;
            set => obj.text = value;
        }
        
        public void Cleanup()
        {
            obj.color = Color.white;
            obj = null;
            note = null;
            gameObject.SetActive(false);
        }
    }
    
    public abstract class ParsedWord<TChar> where TChar : ParsedChar
    {
        public List<TChar> CharNotes { get; protected set; }
        public virtual float Beat { get; set; }
        public virtual float LastBeat { get; protected set; }
    }
}
