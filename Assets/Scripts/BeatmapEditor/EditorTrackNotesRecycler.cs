using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BeatmapEditor
{
    // todo: better 'shared container / coordinate' system to sync to EditorTrack
    public class EditorTrackNotesRecycler : Singleton<EditorTrackNotesRecycler>
    {
        [SerializeField] private TextMeshProUGUI characterPrefab = null;
        [SerializeField] private TMP_InputField inputFieldPrefab = null;
        private EditorCharObject _editorCharObjectPrefab;
        private EditorWordObject _emptyWordPrefab;
        private GameObject _rubbishBin;

        private RecyclerPool<EditorCharObject> _charRecyclerPool;
        private RecyclerList<EditorWordObject> _wordRecyclerList;

        private Beatmap _currentBeatmap;

        private SortedSet<int> _wordIndices;
        private Dictionary<int, EditorWord> _wordLookup;

        private void Awake() 
        {
            _rubbishBin = new GameObject("Rubbish Bin");
            _rubbishBin.transform.SetParent(transform, false);
            
            var wordObjTemplate = new GameObject("Word Template");
            wordObjTemplate.AddComponent<EditorWordObject>();
            wordObjTemplate.transform.SetParent(transform, false);
            _emptyWordPrefab = wordObjTemplate.GetComponent<EditorWordObject>();

            var characterTemplate = Instantiate(characterPrefab);
            characterTemplate.gameObject.AddComponent<EditorCharObject>();
            _editorCharObjectPrefab = characterTemplate.GetComponent<EditorCharObject>();
        }

        private EditorCharObject CreateChar() => Instantiate(_editorCharObjectPrefab, transform);
        private EditorWordObject CreateWord() => Instantiate(_emptyWordPrefab, transform);
        private void InitWord(EditorWordObject item, int index)
        {
            item.Word = _wordLookup[index];
            item.InputFieldPrefab = inputFieldPrefab;
            var itemTransform = item.transform;
            itemTransform.localPosition = new Vector3(_beatSpacing * index.IndexToBeat(), 0, 0);
            if (item.CharObjRefs == null || item.CharObjRefs.Count > 0)
                item.CharObjRefs = new List<EditorCharObject>();

            foreach (var note in item.Word.CharNotes)
            {
                var charObj = _charRecyclerPool.Request();
                charObj.Init(note);
                var charObjTransform = charObj.transform;
                charObjTransform.SetParent(itemTransform);
                charObjTransform.localPosition = new Vector3(_beatSpacing * note.Beat, 0, 0);
                charObj.gameObject.SetActive(true);
                item.CharObjRefs.Add(charObj);
            }
        }
        private void CleanupWord(EditorWordObject item, int index)
        {
            foreach (var charObject in item.CharObjRefs)
            {
                charObject.gameObject.SetActive(false);
                charObject.transform.SetParent(_rubbishBin.transform);
                charObject.Cleanup();
                _charRecyclerPool.Add(charObject);
            }
            item.CharObjRefs = null;
        }

        private void UpdateSpacing()
        {
            var lookup = _wordRecyclerList.visibleItemsLookup;
            foreach (var index in lookup.Keys)
            {
                lookup[index].transform.localPosition = new Vector3(_beatSpacing * index.IndexToBeat(), 0, 0);
                lookup[index].UpdateSpacing(_beatSpacing);
            }
        }

        private List<int> GetNewLineIndicesInWindow(int from, int to)
        {
            // todo: better, with accounting for last index / width spacing and stuff. But for now a slightly larger window will do
            return _wordIndices.GetViewBetween(from, to).ToList();
        }

        private int[] GetCurrentWindow()
        {
            var containerWidth = EditorTrack.Instance.containerRect.width;
            var containerWidthExtension = containerWidth * .5f;
            var minBeat = Mathf.Max((_panX - containerWidthExtension) / _beatSpacing, 0f);
            var maxBeat = minBeat + (containerWidth + containerWidthExtension*2f) / _beatSpacing;
            return new[] {minBeat.BeatToIndex(), maxBeat.BeatToIndex()};
        }

        public void LoadBeatmap(Beatmap beatmap)
        {
            _wordLookup = new Dictionary<int, EditorWord>();
            _wordIndices = new SortedSet<int>();
            foreach (var word in beatmap.words)
            {
                var index = word.beat.BeatToIndex();
                _wordIndices.Add(index);
                _wordLookup[index] = new EditorWord(word);
            }
        }

        public void Init(Beatmap beatmap)
        {
            _currentBeatmap = beatmap;
            LoadBeatmap(_currentBeatmap);

            _charRecyclerPool = new RecyclerPool<EditorCharObject>(CreateChar);
            _wordRecyclerList = new RecyclerList<EditorWordObject>(
                CreateWord, InitWord, GetNewLineIndicesInWindow, GetCurrentWindow(), CleanupWord);
        }

        public void RefreshBeatmap()
        {
            LoadBeatmap(_currentBeatmap);
            _wordRecyclerList.Refresh();
        }

        public void Destroy()
        {
            _wordRecyclerList.Destroy();
            _charRecyclerPool.Destroy();
            Object.Destroy(gameObject);
        }

        public void RefreshWindow()
        {
            _wordRecyclerList.SetVisibleWindow(GetCurrentWindow());
            UpdateSpacing();
        }

        public void EditWord(int index) =>
            _wordRecyclerList.visibleItemsLookup[index].Edit();

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