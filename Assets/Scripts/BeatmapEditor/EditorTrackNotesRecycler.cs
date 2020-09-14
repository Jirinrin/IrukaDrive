using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BeatmapEditor
{
    // todo: better 'shared container / coordinate' system to sync to EditorTrack
    public class EditorTrackNotesRecycler : Singleton<EditorTrackNotesRecycler>
    {
        [SerializeField] private TextMeshProUGUI characterPrefab;
        private EditorWordObject _emptyWordObject;
        private GameObject _rubbishBin;

        private RecyclerPool<TextMeshProUGUI> _charRecyclerPool;
        private RecyclerList<EditorWordObject> _wordRecyclerList;

        private Beatmap _currentBeatmap;

        private SortedSet<int> _wordIndices;
        private Dictionary<int, EditorWord> _wordLookup;

        private void Awake() 
        {
            _rubbishBin = new GameObject("Rubbish Bin");
            _rubbishBin.transform.SetParent(transform, false);
            
            var wordObjPrefab = new GameObject("Word Template");
            wordObjPrefab.AddComponent<EditorWordObject>();
            wordObjPrefab.transform.SetParent(transform, false);
            _emptyWordObject = wordObjPrefab.GetComponent<EditorWordObject>();
        }

        private TextMeshProUGUI CreateChar() => Instantiate(characterPrefab, transform);
        private EditorWordObject CreateWord() => Instantiate(_emptyWordObject, transform);
        private void InitWord(EditorWordObject item, int index)
        {
            var itemTransform = item.transform;
            itemTransform.localPosition = new Vector3(_beatSpacing * index / 1000, 0, 0);
            var word = _wordLookup[index];

            foreach (var note in word.CharNotes)
            {
                var charObj = _charRecyclerPool.Request();
                charObj.transform.SetParent(itemTransform);
                charObj.text = note.Char.ToString();
                charObj.transform.localPosition = new Vector3(_beatSpacing * note.Beat, 0, 0);
                charObj.enabled = true;
                note.CharObjRef = charObj;
            }
            // Debug.Log($"init word starting with char '{c0.Char}'");
        }
        private void CleanupWord(EditorWordObject item, int index)
        {
            foreach (var note in _wordLookup[index].CharNotes)
            {
                var charObject = note.CharObjRef;
                charObject.enabled = false;
                charObject.transform.SetParent(_rubbishBin.transform);
                _charRecyclerPool.Add(charObject);
                note.CharObjRef = null;
            }
        }

        private void UpdateSpacing()
        {
            var lookup = _wordRecyclerList.visibleItemsLookup;
            foreach (var index in lookup.Keys)
            {
                foreach (var note in _wordLookup[index].CharNotes)
                {
                    note.CharObjRef.transform.localPosition = new Vector3(_beatSpacing * note.Beat, 0, 0);
                }
                lookup[index].transform.localPosition = new Vector3(_beatSpacing * index/1000, 0, 0);
            }
        }

        private List<int> GetNewLineIndicesInWindow(int from, int to)
        {
            // todo: better, with accounting for last index / width spacing and stuff
            return _wordIndices.GetViewBetween(from, to).ToList();
        }

        private int[] GetCurrentWindow()
        {
            var containerWidth = EditorTrack.Instance.containerRect.width;
            var containerWidthExtension = containerWidth * .5f;
            var minBeat = Mathf.Max((_panX - containerWidthExtension) / _beatSpacing, 0f);
            var maxBeat = minBeat + (containerWidth + containerWidthExtension*2f) / _beatSpacing;
            Debug.Log($"bla, {maxBeat - minBeat}");
            return new[] {Mathf.RoundToInt(minBeat*1000f), Mathf.RoundToInt(maxBeat*1000f)};
        }
        
        public void Init(Beatmap beatmap)
        {
            _currentBeatmap = beatmap;
            _wordLookup = new Dictionary<int, EditorWord>();
            _wordIndices = new SortedSet<int>();
            foreach (var word in beatmap.words)
            {
                var index = Mathf.RoundToInt(word.beat * 1000);
                _wordIndices.Add(index);
                _wordLookup[index] = new EditorWord(word);
            }

            _charRecyclerPool = new RecyclerPool<TextMeshProUGUI>(CreateChar);
            _wordRecyclerList = new RecyclerList<EditorWordObject>(
                CreateWord, InitWord, GetNewLineIndicesInWindow, GetCurrentWindow(), CleanupWord);
        }

        public void RefreshWindow()
        {
            _wordRecyclerList.SetVisibleWindow(GetCurrentWindow());
            UpdateSpacing();
        }

        // Coming from EditorTrack
        
        private float _panX;
        private float _beatSpacing;
        private void OnPan(float panX) => _panX = panX;
        private void OnZoom(float beatSpacing) => _beatSpacing = beatSpacing;
        private void OnEnable()
        {
            EditorTrack.OnPan += OnPan;
            EditorTrack.OnZoom += OnZoom;
        }
        private void OnDisable()
        {
            EditorTrack.OnPan -= OnPan;
            EditorTrack.OnZoom -= OnZoom;
        }
    }
}