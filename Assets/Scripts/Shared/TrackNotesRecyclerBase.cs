using System.Collections.Generic;
using System.Linq;
using TMPro;
using Tools.Commons;
using UnityEngine;

namespace Shared
{
    public abstract class TrackNotesRecyclerBase<T, TWord, TNote, TCharObj, TWordObj> : Singleton<T> 
        where T : TrackNotesRecyclerBase<T, TWord, TNote, TCharObj, TWordObj>
        where TWord : ParsedWord<TNote>
        where TNote : ParsedNote // todo: infer from TWord?
        where TCharObj : MonoBehaviour
        where TWordObj : WordObject<TCharObj, TWord, TNote>
    {
        [SerializeField] protected TextMeshProUGUI characterPrefab = null;
        protected TCharObj charObjPrefab;
        protected TWordObj _emptyWordObjPrefab;
        protected GameObject _rubbishBin;
        
        protected RecyclerPool<TCharObj> _charRecyclerPool;
        protected RecyclerList<TWordObj> _wordRecyclerList;
        
        protected SortedSet<int> _wordIndices;
        protected Dictionary<int, TWord> _wordLookup;
        
        // Expected to be set from Init before calling base.Init
        protected float containerWidth;
        
        public Dictionary<int, TWordObj> VisibleWordObjects => _wordRecyclerList.visibleItemsLookup;
        
        private void Awake() 
        {
            _rubbishBin = new GameObject("Rubbish Bin");
            _rubbishBin.transform.SetParent(transform, false);
            
            var wordObjPrefab = new GameObject("Word Template");
            wordObjPrefab.AddComponent<TWordObj>();
            wordObjPrefab.transform.SetParent(transform, false);
            _emptyWordObjPrefab = wordObjPrefab.GetComponent<TWordObj>();
            
            var characterTemplate = Instantiate(characterPrefab);
            if (characterTemplate.GetComponent<TCharObj>() == null)
                characterTemplate.gameObject.AddComponent<TCharObj>();
            charObjPrefab = characterTemplate.GetComponent<TCharObj>();
        }
        
        // Methods necessary for RecyclerList
        
        protected TCharObj CreateChar() => Instantiate(charObjPrefab, transform);
        protected TWordObj CreateWord() => Instantiate(_emptyWordObjPrefab, transform);

        protected abstract void InitCharObj(TCharObj charObj, TNote note);
        
        protected virtual void InitWord(TWordObj item, int index)
        {
            item.Word = _wordLookup[index];
            var itemTransform = item.transform;
            itemTransform.localPosition = new Vector3(_beatSpacing * index.IndexToBeat(), 0, 0);
            if (item.charObjRefs == null || item.charObjRefs.Count > 0)
                item.charObjRefs = new List<TCharObj>();

            foreach (var note in item.Word.CharNotes)
            {
                var charObj = _charRecyclerPool.Request();
                InitCharObj(charObj, note);
                var charObjTransform = charObj.transform;
                charObjTransform.SetParent(itemTransform);
                charObjTransform.localPosition = new Vector3(_beatSpacing * note.Beat, 0, 0);
                charObj.gameObject.SetActive(true);
                item.charObjRefs.Add(charObj);
            }
        }
        
        protected virtual void CleanupWord(TWordObj item, int index)
        {
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
        
        protected List<int> GetNewLineIndicesInWindow(int from, int to)
        {
            // todo: better, with accounting for last index / width spacing and stuff. But for now a slightly larger window will do
            return _wordIndices.GetViewBetween(from, to).ToList();
        }
        
        protected int[] GetCurrentWindow()
        {
            var containerWidthExtension = containerWidth * .5f;
            var minBeat = Mathf.Max((_panX - containerWidthExtension) / _beatSpacing, 0f);
            var maxBeat = minBeat + (containerWidth + containerWidthExtension*2f) / _beatSpacing;
            return new[] {minBeat.BeatToIndex(), maxBeat.BeatToIndex()};
        }
        
        // Public
        
        public virtual void RefreshWindow() =>
            _wordRecyclerList.SetVisibleWindow(GetCurrentWindow());
        
        // Init

        // Should be wrapped by a more specific Init, please call after LoadBeatmap
        protected void Init()
        {
            _charRecyclerPool = new RecyclerPool<TCharObj>(CreateChar);
            _wordRecyclerList = new RecyclerList<TWordObj>(
                CreateWord, InitWord, GetNewLineIndicesInWindow, GetCurrentWindow(), CleanupWord);
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
            _wordRecyclerList.Destroy();
            _charRecyclerPool.Destroy();
            Object.Destroy(gameObject);
        }
        
        // Coming from track
        
        protected float _beatSpacing;

        protected float _panX;
        protected void OnPan(float panX) => _panX = panX;
    }
}