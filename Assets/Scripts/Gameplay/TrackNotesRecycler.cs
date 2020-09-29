using System;
using System.Collections.Generic;
using System.Linq;
using Shared;
using TMPro;
using Tools.Commons;
using UnityEngine;
using Object = UnityEngine.Object;

// todo: shared base with editor notes recycler
namespace Gameplay
{
    public class TrackNotesRecycler : Singleton<TrackNotesRecycler>
    {
        [SerializeField] private TextMeshProUGUI characterPrefab = null;
        private RuntimeWordObject _emptyWordObject;
        private GameObject _rubbishBin;

        private RecyclerPool<TextMeshProUGUI> _charRecyclerPool;
        private RecyclerList<RuntimeWordObject> _wordRecyclerList;

        private SortedSet<int> _wordIndices;
        private Dictionary<int, RuntimeWord> _wordLookup;

        public Dictionary<int, RuntimeWordObject> VisibleWordObjects => _wordRecyclerList.visibleItemsLookup;

        private void Awake() 
        {
            _rubbishBin = new GameObject("Rubbish Bin");
            _rubbishBin.transform.SetParent(transform, false);
            
            var wordObjPrefab = new GameObject("Word Template");
            wordObjPrefab.AddComponent<RuntimeWordObject>();
            wordObjPrefab.transform.SetParent(transform, false);
            _emptyWordObject = wordObjPrefab.GetComponent<RuntimeWordObject>();
        }

        private TextMeshProUGUI CreateChar() => Instantiate(characterPrefab, transform);
        private RuntimeWordObject CreateWord() => Instantiate(_emptyWordObject, transform);
        private void InitWord(RuntimeWordObject item, int index)
        {
            item.Word = _wordLookup[index];
            var itemTransform = item.transform;
            itemTransform.localPosition = new Vector3(_beatSpacing * index.IndexToBeat(), 0, 0);
            if (item.CharObjRefs == null || item.CharObjRefs.Count > 0)
                item.CharObjRefs = new List<TextMeshProUGUI>();

            foreach (var note in item.Word.CharNotes)
            {
                var charObj = _charRecyclerPool.Request();
                charObj.text = note.Char.ToString();
                var charObjTransform = charObj.transform;
                charObjTransform.SetParent(itemTransform);
                charObjTransform.localPosition = new Vector3(_beatSpacing * note.Beat, 0, 0);
                charObj.gameObject.SetActive(true);
                charObj.color = Color.white;
                item.CharObjRefs.Add(charObj);
            }
        
            if (_wordAppearActionQueue.ContainsKey(index))
            {
                while (_wordAppearActionQueue[index].Any())
                    _wordAppearActionQueue[index].Dequeue().Invoke();
                _wordAppearActionQueue.Remove(index);
            }
        }
        private void CleanupWord(RuntimeWordObject item, int index)
        {
            foreach (var charObject in item.CharObjRefs)
            {
                charObject.gameObject.SetActive(false);
                charObject.transform.SetParent(_rubbishBin.transform);
                charObject.text = null;
                _charRecyclerPool.Add(charObject);
            }
            item.CharObjRefs = null;
        }

        private List<int> GetNewLineIndicesInWindow(int from, int to)
        {
            // todo: better, with accounting for last index / width spacing and stuff. But for now a slightly larger window will do
            return _wordIndices.GetViewBetween(from, to).ToList();
        }

        private int[] GetCurrentWindow()
        {
            var containerWidth = TrackManager.Instance.containerRect.width;
            var containerWidthExtension = containerWidth * 1f;
            var minBeat = Mathf.Max((_panX - containerWidthExtension) / _beatSpacing, 0f);
            var maxBeat = minBeat + (containerWidth + containerWidthExtension*2f) / _beatSpacing;
            return new[] {minBeat.BeatToIndex(), maxBeat.BeatToIndex()};
        }

        private void LoadBeatmap(List<RuntimeWord> words, float startBeatSpacing)
        {
            _wordLookup = new Dictionary<int, RuntimeWord>();
            _wordIndices = new SortedSet<int>();
            _beatSpacing = startBeatSpacing;
            _wordAppearActionQueue = new Dictionary<int, Queue<Action>>();
        
            foreach (var word in words)
            {
                var index = word.Beat.BeatToIndex();
                _wordIndices.Add(index);
                _wordLookup[index] = word;
            }
        }

        public void Init(List<RuntimeWord> words, float startBeatSpacing)
        {
            LoadBeatmap(words, startBeatSpacing);

            _charRecyclerPool = new RecyclerPool<TextMeshProUGUI>(CreateChar);
            _wordRecyclerList = new RecyclerList<RuntimeWordObject>(
                CreateWord, InitWord, GetNewLineIndicesInWindow, GetCurrentWindow(), CleanupWord);
        }

        public void Destroy()
        {
            _wordRecyclerList.Destroy();
            _charRecyclerPool.Destroy();
            Object.Destroy(gameObject);
        }

        public void RefreshWindow() =>
            _wordRecyclerList.SetVisibleWindow(GetCurrentWindow());

        // This is for when you want to do something once a certain word appears
        private Dictionary<int, Queue<Action>> _wordAppearActionQueue;
        public void EnqueueForWordAppear(int index, Action action)
        {
            if (_wordAppearActionQueue[index] == null)
                _wordAppearActionQueue[index] = new Queue<Action>();
            _wordAppearActionQueue[index].Enqueue(action);
        }

        // Coming from TrackManager

        private float _beatSpacing;

        private float _panX;
        private void OnPan(float panX) => _panX = panX;
        private void OnEnable()
        {
            TrackManager.OnPan += OnPan;
        }
        private void OnDisable()
        {
            TrackManager.OnPan -= OnPan;
        }
    }
}