using System.Collections.Generic;
using System.Linq;
using Shared.Domain;
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
        private TCharObj _charObjPrefab;
        private TWordObj _emptyWordObjPrefab;
        private GameObject _rubbishBin;

        private RecyclerPool<TCharObj> _charRecyclerPool;
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
            if (characterTemplate.GetComponent<TCharObj>() == null)
                characterTemplate.gameObject.AddComponent<TCharObj>();
            _charObjPrefab = characterTemplate.GetComponent<TCharObj>();
        }
        
        // Methods necessary for RecyclerList

        private TCharObj CreateChar() => Instantiate(_charObjPrefab, transform);
        private TWordObj CreateWord() => Instantiate(_emptyWordObjPrefab, transform);

        protected abstract void InitCharObj(TCharObj charObj, TNote note);
        
        protected virtual void InitWord(TWordObj item, int index)
        {
            item.word = _wordLookup[index];
            var itemTransform = item.transform;
            itemTransform.localPosition = new Vector3(beatSpacing * index.IndexToBeat(), 0, 0);
            if (item.charObjRefs == null || item.charObjRefs.Count > 0)
                item.charObjRefs = new List<TCharObj>();

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

        private IEnumerable<int> GetNewLineIndicesInWindow(int from, int to)
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
        
        // Public
        
        public virtual void RefreshWindow() =>
            wordRecyclerList.SetVisibleWindow(GetCurrentWindow());
        
        // Init

        // Should be wrapped by a more specific Init, please call after LoadBeatmap
        protected void Init()
        {
            _charRecyclerPool = new RecyclerPool<TCharObj>(CreateChar);
            wordRecyclerList = new RecyclerList<TWordObj>(
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
            wordRecyclerList.Destroy();
            _charRecyclerPool.Destroy();
            Object.Destroy(gameObject);
        }
        
        // Coming from track
        
        protected float beatSpacing;

        protected float panX;
        protected void OnPan(float newPanX) => panX = newPanX;
    }
}