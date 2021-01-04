using System.Linq;
using Shared.Domain;
using Tools;

namespace BeatmapEditor.Domain
{
    public class EditorWord : EditorWordBase<BeatmapWord, ParsedChar>
    {
        public override string Text
        {
            get => word.text;
            set
            {
                word.text = value;
                CharNotes = word.ParseNotes();
            }
        }
        
        public float BeatInterval
        {
            get => word.beatInterval;
            set
            {
                word.beatInterval = value;
                var newCharNotes = word.ParseNotes();
                CharNotes = CharNotes.Select((note, i) =>
                {
                    note.beat = newCharNotes[i].beat;
                    return note;
                }).ToList();
            }
        }

        public EditorWord(BeatmapWord word) : base(word) =>
            CharNotes = word.ParseNotes();

        public BeatmapWord CloneWord(float? beat = null) => word.Clone(beat);
    }

    public class EditorChord : EditorWordBase<BeatmapChord, ParsedCharBase>
    {
        public override string Text
        {
            get => word.text;
            set
            {
                word.text = value;
                CharNotes = word.ParseNotes();
            }
        }
        
        public EditorChord(BeatmapChord word) : base(word) { }
        
        public BeatmapChord CloneWord(float? beat = null) => word.Clone();
    }
    
    // todo: chord version + base; difficult because multiple inheritance kinda needed

    public abstract class EditorWordBase : EditorWordBase<BeatmapWordBase, ParsedCharBase>
    {
        protected EditorWordBase(BeatmapWordBase word) : base(word) { }
    }
    public abstract class EditorWordBase<TWord, TChar> : ParsedWordBase<TChar>
        where TWord : BeatmapWordBase
        where TChar : ParsedCharBase
    {
        protected readonly TWord word;

        public abstract string Text { get; set; }
        public override float Beat
        {
            get => word.beat;
            set => word.beat = value;
        }
        public override float LastBeat => word.LastBeat();
        public float BeatWidth => word.BeatWidth;
        
        protected EditorWordBase(TWord word)
        {
            this.word = word;
        }
        
        public void Delete() => BeatmapEditorManager.currentBeatmap.words.Remove(word);
    }
}