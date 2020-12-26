using System.Collections.Generic;
using Shared.Domain;
using TMPro;
using Tools.Commons;
using UnityEngine;

namespace Shared
{
    public abstract class TrackNotesRecyclerBase<T, TWord, TNote, TWordObj> : Singleton<T> 
        where T : TrackNotesRecyclerBase<T, TWord, TNote, TWordObj>
        where TWord : ParsedWord<TNote>
        where TNote : ParsedNote // todo: infer from TWord?
        where TWordObj : WordObject<TWord, TNote>
    {
        [SerializeField] protected TextMeshProUGUI characterPrefab = null;
        private CharObject _charObjPrefab;
        private TWordObj _emptyWordObjPrefab;
        private GameObject _rubbishBin;

        private RecyclerPool<CharObject> _charRecyclerPool;
        protected RecyclerList<TWordObj> wordRecyclerList;

        private SortedSet<int> _wordIndices;
        private Dictionary<int, TWord> _wordLookup;
        
        // Expected to be set from Init before calling base.Init
        protected float containerWidth;
        
        public Dictionary<int, TWordObj> VisibleWordObjects => wordRecyclerList.visibleItemsLookup;
        
        private void Awake() 
        {
            _rubbishBin = new GameObject("Rubbish Bin");
            _rubbishBin.transform.SetParent(transform, false);
            
            var wordObjPrefab = new GameObject("Word Template");
            wordObjPrefab.AddComponent<TWordObj>();
            wordObjPrefab.transform.SetParent(transform, false);
            _emptyWordObjPrefab = wordObjPrefab.GetComponent<TWordObj>();
            
            var characterTemplate = Instantiate(characterPrefab);
            if (characterTemplate.GetComponent<CharObject>() == null)
                characterTemplate.gameObject.AddComponent<CharObject>();
            _charObjPrefab = characterTemplate.GetComponent<CharObject>();
        }
        
        // Methods necessary for RecyclerList

        private CharObject CreateChar() => Instantiate(_charObjPrefab, transform);
        private TWordObj CreateWord() => Instantiate(_emptyWordObjPrefab, transform);

        protected virtual void InitCharObj(CharObject charObj, TNote note) => charObj.Init(note);
        
        protected virtual void InitWord(TWordObj item, int index)
        {
            item.word = _wordLookup[index];
            var itemTransform = item.transform;
            itemTransform.localPosition = new Vector3(beatSpacing * index.IndexToBeat(), 0, 0);
            if (item.charObjRefs == null || item.charObjRefs.Count > 0)
                item.charObjRefs = new List<CharObject>();

            foreach (var note in item.word.CharNotes)
            {
                var charObj = _charRecyclerPool.Request();
                InitCharObj(charObj, note);
                var charObjTransform = charObj.transform;
                charObjTransform.SetParent(itemTransform);
                charObjTransform.localPosition = new Vector3(beatSpacing * note.beat, 0, 0);
                charObj.gameObject.SetActive(true);
                item.charObjRefs.Add(charObj);
            }
        }

        private void CleanupWord(TWordObj item, int index)
        {
            foreach (var charObject in item.charObjRefs)
                charObject.Cleanup();
            
            if (item.charObjRefs == null)
                return;
            
            foreach (var charObject in item.charObjRefs)
            {
                charObject.gameObject.SetActive(false);
                charObject.transform.SetParent(_rubbishBin.transform);
                _charRecyclerPool.Add(charObject);
            }
            item.charObjRefs = null;
        }

        private IEnumerable<int> GetNewNoteIndicesInWindow(int from, int to)
        {
            // todo: better, with accounting for last index / width spacing and stuff. But for now a slightly larger window will do
            return _wordIndices.GetViewBetween(from, to);
        }

        private int[] GetCurrentWindow()
        {
            var containerWidthExtension = containerWidth * .5f;
            var minBeat = Mathf.Max((panX - containerWidthExtension) / beatSpacing, 0f);
            var maxBeat = minBeat + (containerWidth + containerWidthExtension*2f) / beatSpacing;
            return new[] {minBeat.BeatToIndex(), maxBeat.BeatToIndex()};
        }
        
        private void UpdateSpacing()
        {
            var lookup = wordRecyclerList.visibleItemsLookup;
            foreach (var index in lookup.Keys)
            {
                lookup[index].transform.localPosition = new Vector3(beatSpacing * index.IndexToBeat(), 0, 0);
                lookup[index].UpdateSpacing(beatSpacing);
            }
        }
        
        // Public
        
        public void RefreshWindow()
        {
            // todo: have a separate handler for onlyl updating the pan? (For performance reasons)
            wordRecyclerList.SetVisibleWindow(GetCurrentWindow());
            UpdateSpacing();
        }

        // Init

        // Should be wrapped by a more specific Init, please call after LoadBeatmap
        protected void Init()
        {
            _charRecyclerPool = new RecyclerPool<CharObject>(CreateChar);
            wordRecyclerList = new RecyclerList<TWordObj>(
                CreateWord, InitWord, GetNewNoteIndicesInWindow, GetCurrentWindow(), CleanupWord);
        }
        
        // Should be wrapped by a more specific LoadBeatmap
        protected void LoadBeatmap(IEnumerable<TWord> words)
        {
            _wordLookup = new Dictionary<int, TWord>();
            _wordIndices = new SortedSet<int>();
            foreach (var word in words)
            {
                var index = word.Beat.BeatToIndex();
                _wordIndices.Add(index);
                _wordLookup[index] = word;
            }
        }
        
        // Misc
        
        public void Destroy()
        {
            wordRecyclerList.Destroy();
            _charRecyclerPool.Destroy();
            Object.Destroy(gameObject);
        }
        
        // Coming from track
        protected float panX;
        protected void OnPan(float newPanX) => panX = newPanX;
        
        protected float beatSpacing;
        protected void OnZoom(float newBeatSpacing) => beatSpacing = newBeatSpacing;
    }
}