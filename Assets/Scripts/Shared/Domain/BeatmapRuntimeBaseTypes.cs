using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace Shared.Domain
{
    // Word object stuff
    
    // todo: make into interface?
    // todo: default implementation when c# 8.0
    public interface IWordObject
    {
        [CanBeNull] public List<CharObject> CharObjRefs { get; set; }
        protected float BeatSpacing { get; set; }
        
        public void UpdateSpacing(float newSpacing)
        {
            BeatSpacing = newSpacing;
            foreach (var charObj in CharObjRefs)
                charObj.transform.localPosition = new Vector3(BeatSpacing * charObj.note.beat, 0, 0);
        }
    }

    public static class IWordExtensions
    {
        
    }
    
    
    public class WordObject : WordObject<ParsedWord<ParsedChar>, ParsedChar> { }
    public class WordObject<TWord, TChar> : WordObjectBase<TWord, TChar>, IWordObject
        where TWord : ParsedWord<TChar>
        where TChar : ParsedChar // todo: a way to infer this?
    {
        public void UpdateSpacing(float newSpacing)
        {
            BeatSpacing = newSpacing;
            foreach (var charObj in CharObjRefs)
                charObj.transform.localPosition = new Vector3(BeatSpacing * charObj.note.beat, 0, 0);
        }

        protected void SetColor(Color color)
        {
            foreach (var charObj in CharObjRefs)
                charObj.obj.color = color;
        }

        public void Cleanup([CanBeNull] Action<CharObject> cleanupChar = null)
        {
            InvokeOnDestroy();
            
            if (CharObjRefs == null)
                return;
            
            foreach (var charObject in CharObjRefs)
            {
                cleanupChar?.Invoke(charObject);
                charObject.Cleanup();
            }
            
            CharObjRefs = null;
        }
    }

    // todo: make into interface?
    public class ChordObject : WordObjectBase<ParsedWordBase>
    {
        // todo: implement, with char obj refs & stuff
        
    }
    
    public abstract class WordObjectBase<TWord, TChar> : MonoBehaviour 
        where TWord : ParsedWordBase<TChar> 
        where TChar : ParsedCharBase
    {
        public TWord word;

        protected void InvokeOnDestroy() => OnDestroy?.Invoke();
        public event Action OnDestroy;
    }
    
    // Char object stuff

    public class CharObject : MonoBehaviour
    {
        [NonSerialized] public ParsedChar note;
        [NonSerialized] public TextMeshProUGUI obj;

        public void Init(ParsedChar note)
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
            obj.color = Color.white;
            obj = null;
            note = null;
            gameObject.SetActive(false);
        }
    }

    // Parsed word stuff

    public abstract class ParsedWord : ParsedWordBase<ParsedChar> { }
    public abstract class ParsedWord<TChar> : ParsedWordBase<TChar> where TChar : ParsedChar { }

    // todo: if this whole type checking thing works, maybe test if it also works to have the generic-less version implement the generic version (like I already do in other places)
    public abstract class ParsedWordBase<TChar> : ParsedWordBase where TChar : ParsedCharBase
    {
        public new List<TChar> CharNotes { get; protected set; }
    }
    public abstract class ParsedWordBase
    {
        public List<ParsedCharBase> CharNotes { get; protected set; }
        public virtual float Beat { get; set; }
        public virtual float LastBeat { get; protected set; }
    }
    
    // Parsed char stuff
    
    public class ParsedChar : ParsedCharBase
    {
        public float beat;
    }
    
    public class ParsedCharBase
    {
        public char character;
    }
}
