using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Shared
{
    public class ParsedNote
    {
        public float Beat;
        public char Char;
    }

    public class WordObject<TCharObj, TWord, TNote> : MonoBehaviour
        where TCharObj : MonoBehaviour 
        where TWord : ParsedWord<TNote>
        where TNote : ParsedNote // todo: a way to infer this?
    {
        [CanBeNull] public List<TCharObj> charObjRefs;

        public TWord Word;
    }
    
    public abstract class ParsedWord<TNote> where TNote : ParsedNote
    {
        public List<TNote> CharNotes { get; set; }
        public virtual float Beat { get; set; }
    }
}
