using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Domain;
using TMPro;
using Tools.Commons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shared
{
    public abstract class TrackNotesRecyclerBase<T, TWord, TChar, TWordObj> : Singleton<T> 
        where T : TrackNotesRecyclerBase<T, TWord, TChar, TWordObj>
        where TWord : ParsedWord<TChar>
        where TChar : ParsedChar // todo: infer from TWord?
        where TWordObj : WordObject<TWord, TChar>
    {
        [SerializeField] protected TextMeshProUGUI characterPrefab = null;
        private CharObject _charObjPrefab;
        private TWordObj _emptyWordObjPrefab;
        private GameObject _rubbishBin;

        private RecyclerPool<CharObject> _charRecyclerPool;
        protected WordRecyclerList wordRecyclerList;

        // Expected to be set from Init before calling base.Init
        protected float containerWidth;
        
        public Dictionary<int, ObjWidthItem> VisibleWordObjects => wordRecyclerList.visibleItemsLookup;
        
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

        protected virtual void InitCharObj(CharObject charObj, TChar ch) => charObj.Init(ch);
        
        protected virtual void InitWord(ObjWidthItem item)
        {
            item.obj.word = item.backingItem;
            var itemTransform = item.obj.transform;
            itemTransform.localPosition = new Vector3(BeatSpacing * item.startIndex.IndexToBeat(), 0, 0);

            var charObjRefs = item.backingItem.CharNotes.Select(c =>
            {
                var charObj = _charRecyclerPool.Request();
                InitCharObj(charObj, c);
                var charObjTransform = charObj.transform;
                charObjTransform.SetParent(itemTransform);
                charObj.gameObject.SetActive(true);
                return charObj;
            }).ToList();
            
            item.obj.Init(charObjRefs, BeatSpacing);
        }

        protected virtual void CleanupWord(TWordObj item, int index)
        {
            item.Cleanup(charObject =>
            {
                charObject.transform.SetParent(_rubbishBin.transform);
                _charRecyclerPool.Add(charObject);
            });
        }

        private int[] GetCurrentWindow()
        {
            var containerWidthExtension = containerWidth * .5f;
            var minBeat = Mathf.Max((PanX - containerWidthExtension) / BeatSpacing, 0f);
            var maxBeat = minBeat + (containerWidth + containerWidthExtension*2f) / BeatSpacing;
            return new[] {minBeat.BeatToIndex(), maxBeat.BeatToIndex()};
        }
        
        private void UpdateSpacing()
        {
            foreach (var item in wordRecyclerList.visibleItemsLookup.Values)
            {
                item.obj.transform.localPosition = new Vector3(BeatSpacing * item.startIndex.IndexToBeat(), 0, 0);
                item.obj.UpdateSpacing(BeatSpacing);
            }
        }
        
        // Public
        
        public void RefreshWindow(bool updateSpacing = false)
        {
            wordRecyclerList.SetVisibleWindow(GetCurrentWindow());
            if (updateSpacing)
                UpdateSpacing();
        }

        // Init

        protected void LoadBeatmap(IEnumerable<TWord> words)
        {
            _charRecyclerPool = new RecyclerPool<CharObject>(CreateChar);
            wordRecyclerList = new WordRecyclerList(
                CreateWord, InitWord, CleanupWord, GetCurrentWindow(), words);
        }
        
        protected void LoadNewWords(IEnumerable<TWord> words) =>
            wordRecyclerList.LoadNewWords(words);
        
        // Misc
        
        public void Destroy()
        {
            Cleanup();
            Object.Destroy(gameObject);
        }

        public void Cleanup()
        {
            wordRecyclerList?.Destroy();
            _charRecyclerPool?.Destroy();
        }

        protected abstract float PanX { get; }
        protected abstract float BeatSpacing { get; }
        
        ////////////////////////////
        // S U B C L A S S E S
        ////////////////////////////
        
        public class WidthItem
        {
            public int startIndex;
            public int endIndex;
            public TWord backingItem;
        }

        public class ObjWidthItem : WidthItem
        {
            public readonly TWordObj obj;
            public ObjWidthItem(WidthItem w, TWordObj obj)
            {
                startIndex = w.startIndex;
                endIndex = w.endIndex;
                backingItem = w.backingItem;
                this.obj = obj;
            }
        }
    
        protected class WordRecyclerList
        {
            private readonly RecyclerPool<TWordObj> _recyclerPool;

            private Dictionary<int, WidthItem> _allItemsLookup;
            private SortedSet<int> _itemStartIndices;
            private SortedSet<int> _itemEndIndices;
            private Dictionary<int, int> _itemEndToStartIndex;
            
            public readonly Dictionary<int, ObjWidthItem> visibleItemsLookup = new Dictionary<int, ObjWidthItem>();

            // Must have 2 items. todo: convert to tuple
            private int[] _window;
        
            private readonly Action<ObjWidthItem> _initItem;     // (item, index) => void
            private readonly Action<TWordObj, int> _cleanupItem; // (item, index) => void

            public WordRecyclerList(
                Func<TWordObj> createItem, Action<ObjWidthItem> initItem, Action<TWordObj, int> cleanupItem, 
                int[] startWindow, IEnumerable<TWord> words)
            {
                _initItem = initItem;
                _cleanupItem = cleanupItem;
                _recyclerPool = new RecyclerPool<TWordObj>(createItem);
                _window = startWindow;

                InitItems(words);
                AddNewWindow(startWindow);
            }

            private void InitItems(IEnumerable<TWord> words)
            {
                _allItemsLookup = new Dictionary<int, WidthItem>();
                _itemStartIndices = new SortedSet<int>();
                _itemEndIndices = new SortedSet<int>();
                _itemEndToStartIndex = new Dictionary<int, int>();
                
                foreach (var word in words)
                {
                    var startIndex = word.Beat.BeatToIndex();
                    var endIndex = word.LastBeat.BeatToIndex();
                    _allItemsLookup[startIndex] = new WidthItem
                    {
                        startIndex = startIndex,
                        endIndex = endIndex,
                        backingItem = word,
                    };
                    _itemStartIndices.Add(startIndex);
                    _itemEndIndices.Add(endIndex);
                    _itemEndToStartIndex[endIndex] = startIndex;
                }
            }
            
            private IEnumerable<int> GetItemIndicesInWindow(int from, int to, bool isLeftSide)
            {
                return isLeftSide
                    ? _itemEndIndices.GetViewBetween(from, to).Select(i => _itemEndToStartIndex[i])
                    : _itemStartIndices.GetViewBetween(from, to);
            }

            private void RemoveFromWindow(IEnumerable<ObjWidthItem> itemsToRemove)
            {
                var indices = itemsToRemove.Select(wi => wi.startIndex).ToArray();
                foreach (var index in indices)
                {
                    var item = visibleItemsLookup[index];
                    item.obj.gameObject.SetActive(false);
                    _cleanupItem(item.obj, item.startIndex);
                    visibleItemsLookup.Remove(item.startIndex);
                    _recyclerPool.Add(item.obj);
                }
            }
            private void RemoveFromWindow(int from, int to, bool isLeftSide)
            {
                var indices = isLeftSide
                    ? visibleItemsLookup.Values.Where(wItem => wItem.endIndex >= from && wItem.endIndex <= to)
                    : visibleItemsLookup.Values.Where(wItem => wItem.startIndex >= from && wItem.startIndex <= to);
                RemoveFromWindow(indices);
            }
        
            private void AddToWindow(IEnumerable<int> newIndices)
            {
                foreach (var itemIndex in newIndices.Except(visibleItemsLookup.Keys))
                {
                    var widthItem = _allItemsLookup[itemIndex];
                    var obj = _recyclerPool.Request();
                    obj.gameObject.SetActive(true);
                    visibleItemsLookup[itemIndex] = new ObjWidthItem(widthItem, obj);
                    _initItem(visibleItemsLookup[itemIndex]);
                }
            }
            private void AddToWindow(int from, int to, bool isLeftSide) => 
                AddToWindow(GetItemIndicesInWindow(from, to, isLeftSide));
            private void AddNewWindow(int[] newWindow)
            {
                var indices = _itemStartIndices.GetViewBetween(newWindow[0], newWindow[1]).Union(
                    _itemEndIndices.GetViewBetween(newWindow[0], newWindow[1]).Select(i => _itemEndToStartIndex[i]));
                AddToWindow(indices);
            }

            private void RecycleAll() => RemoveFromWindow(visibleItemsLookup.Values);

            private void ReplaceWindow(int[] newWindow)
            {
                RecycleAll();
                AddNewWindow(newWindow);
            }
            
            public void LoadNewWords(IEnumerable<TWord> words)
            {
                RecycleAll();
                InitItems(words);
                AddNewWindow(_window);
            }
            
            public void SetVisibleWindow(int[] newWindow)
            {
                if (newWindow[0] > _window[1] || newWindow[1] < _window[0])
                {
                    ReplaceWindow(newWindow);
                    _window = newWindow;
                    return;
                }
            
                if (newWindow[0] > _window[0])
                    RemoveFromWindow(_window[0], newWindow[0], true);
                else if (newWindow[0] < _window[0])
                    AddToWindow(newWindow[0], _window[0], true);
                if (newWindow[1] > _window[1])
                    AddToWindow(_window[1], newWindow[1], false);
                else if (newWindow[1] < _window[1])
                    RemoveFromWindow(newWindow[1], _window[1], false);
            
                _window = newWindow;
            }

            public void Destroy()
            {
                RecycleAll();
                _recyclerPool.Destroy();
            }
        }
    }
}